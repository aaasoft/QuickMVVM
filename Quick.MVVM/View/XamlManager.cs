using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xaml;

namespace Quick.MVVM.View
{
    /// <summary>
    /// 用于加载UI目录下面的xaml文件的管理器
    /// </summary>
    public class XamlManager
    {
        private String xamlFileFolder;
        private List<IXamlFilter> filterList = new List<IXamlFilter>();

        public XamlManager(String xamlFileFolder)
        {
            this.xamlFileFolder = xamlFileFolder;
        }

        /// <summary>
        /// 注册过滤器
        /// </summary>
        /// <param name="filter"></param>
        public void RegisterFilter(IXamlFilter filter)
        {
            filterList.Add(filter);
        }

        /// <summary>
        /// 取消注册过滤器
        /// </summary>
        /// <param name="filter"></param>
        public void UnregisterFilter(IXamlFilter filter)
        {
            filterList.Remove(filter);
        }

        /// <summary>
        /// 从xaml文件解析XAML
        /// </summary>
        /// <param name="filePath">xaml文件路径</param>
        /// <returns></returns>
        public Object ParseFromFile(String filePath)
        {
            return ParseFromFile(filePath, null);
        }

        /// <summary>
        /// 从xaml文件解析XAML
        /// </summary>
        /// <param name="filePath">xaml文件路径</param>
        /// <param name="loadCompatedAction">加载完成后要执行的操作</param>
        /// <returns></returns>
        public Object ParseFromFile(String filePath, Action<Object> loadCompatedAction)
        {
            String fullFilePath = null;
            if (File.Exists(filePath))
                fullFilePath = filePath;
            else
                fullFilePath = Path.Combine(xamlFileFolder, filePath);

            ////如果能远程下载UI文件
            //if (DcimsHttpUtils.CanDownloadFile())
            //{
            //    RemoteResourceControl rrc = new RemoteResourceControl(fullFilePath);
            //    if (loadCompatedAction != null)
            //    {
            //        rrc.LoadCompleted += (sender, e) =>
            //        {
            //            loadCompatedAction.Invoke(rrc.Content);
            //        };
            //    }
            //    rrc.Init();
            //    return rrc;
            //}
            //else
            //{

            //如果xaml文件不存在，则抛出异常
            if (!File.Exists(fullFilePath))
                throw new ApplicationException(String.Format("Xaml File '{0}' not found!", fullFilePath));
            String xamlContent = File.ReadAllText(fullFilePath);
            Object obj = Parse(xamlContent);
            if (loadCompatedAction != null)
                loadCompatedAction.Invoke(obj);
            return obj;
        }

        /// <summary>
        /// 从文本内容解析XAML
        /// </summary>
        /// <param name="xamlContent">xaml文本内容</param>
        /// <returns></returns>
        public Object Parse(String xamlContent)
        {
            //解析之前，对XAML的文本内容进行处理
            filterList.ForEach(filter => xamlContent = filter.Before(xamlContent));

            //WPF架构上下文
            XamlSchemaContext wpfSchemaContext = System.Windows.Markup.XamlReader.GetWpfSchemaContext();

            XamlXmlReader reader = new XamlXmlReader(new StringReader(xamlContent), wpfSchemaContext);
            XamlObjectWriter writer = new XamlObjectWriter(wpfSchemaContext);

            String[] notAllowPropertyNames = new String[] { "Foreground", "Background", "Content", "Triggers", "Items", "DataContext", "RenderTransformOrigin" };
            Type warnControlType = typeof(ContentControl);
            Boolean isMemeberUnknown = false;

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XamlNodeType.NamespaceDeclaration:
                        writer.WriteNamespace(reader.Namespace);
                        break;
                    case XamlNodeType.StartObject:
                        if(isMemeberUnknown)
                        {
                            reader.Skip();
                            continue;
                        }
                        XamlType xamlType = reader.Type;
                        if (!xamlType.IsUnknown)
                        {
                            writer.WriteStartObject(xamlType);
                            continue;
                        }
                        
                        //对象
                        writer.WriteStartObject(new XamlType(warnControlType, wpfSchemaContext));
                        //内容
                        writer.WriteStartMember(new XamlMember(warnControlType.GetProperty("Content"), wpfSchemaContext));
                        {
                            //ViewBox层
                            writer.WriteStartObject(new XamlType(typeof(Viewbox), wpfSchemaContext));
                            //Child属性
                            writer.WriteStartMember(new XamlMember(typeof(Viewbox).GetProperty("Child"), wpfSchemaContext));
                            {
                                writer.WriteStartObject(new XamlType(typeof(TextBlock), wpfSchemaContext));
                                //设置Text属性
                                writer.WriteStartMember(new XamlMember(typeof(TextBlock).GetProperty("Text"), wpfSchemaContext));
                                writer.WriteValue("ERROR");
                                writer.WriteEndMember();
                                //设置Background属性
                                writer.WriteStartMember(new XamlMember(typeof(TextBlock).GetProperty("Background"), wpfSchemaContext));
                                writer.WriteValue("Black");
                                writer.WriteEndMember();
                                //设置Background属性
                                writer.WriteStartMember(new XamlMember(typeof(TextBlock).GetProperty("Foreground"), wpfSchemaContext));
                                writer.WriteValue("Red");
                                writer.WriteEndMember();

                                writer.WriteEndObject();
                            }
                            writer.WriteEndMember();
                            //设置Stretch属性
                            writer.WriteStartMember(new XamlMember(typeof(Viewbox).GetProperty("Stretch"), wpfSchemaContext));
                            writer.WriteValue(Enum.GetName(typeof(System.Windows.Media.Stretch), System.Windows.Media.Stretch.Fill));
                            writer.WriteEndMember();
                            //设置ToolTip属性
                            writer.WriteStartMember(new XamlMember(typeof(Viewbox).GetProperty("ToolTip"), wpfSchemaContext));
                            writer.WriteValue(String.Format("未能找到类型：{0}", xamlType.ToString()));
                            writer.WriteEndMember();

                            writer.WriteEndObject();
                        }
                        writer.WriteEndMember();
                        break;
                    case XamlNodeType.EndObject:
                        if (isMemeberUnknown)
                            continue;
                        writer.WriteEndObject();
                        break;
                    case XamlNodeType.StartMember:
                        if (reader.Member.IsUnknown)
                        {
                            var propertyInfo = warnControlType.GetProperty(reader.Member.Name);
                            if (propertyInfo == null || notAllowPropertyNames.Contains(reader.Member.Name))
                            {
                                isMemeberUnknown = true;
                                continue;
                            }
                            writer.WriteStartMember(new XamlMember(propertyInfo, wpfSchemaContext));
                        }
                        else
                            writer.WriteStartMember(reader.Member);
                        break;
                    case XamlNodeType.EndMember:
                        if (isMemeberUnknown)
                        {
                            isMemeberUnknown = false;
                            continue;
                        }
                        writer.WriteEndMember();
                        break;
                    case XamlNodeType.Value:
                        if (isMemeberUnknown)
                            continue;
                        writer.WriteValue(reader.Value);
                        break;
                    case XamlNodeType.GetObject:
                        writer.WriteGetObject();
                        break;
                }
            }

            //解析
            Object obj = writer.Result;
            //解析之后，对解析的结果进行处理
            filterList.ForEach(filter => obj = filter.After(obj));
            //返回
            return obj;
        }
    }
}
