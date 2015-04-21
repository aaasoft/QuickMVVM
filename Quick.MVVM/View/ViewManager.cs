﻿using Quick.MVVM.Utils;
using Quick.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Quick.MVVM.View
{
    /// <summary>
    /// 用于加载视图模型对应视图的管理器
    /// </summary>
    public class ViewManager : IViewManager
    {
        public const String CONST_DEFAULT_THEME = "Default";
        public const String CONST_DEFAULT_LANGUAGE = "zh-CN";

        private static DependencyProperty CurrentThemeProperty = DependencyProperty.RegisterAttached("CurrentTheme", typeof(String), typeof(FrameworkElement), new PropertyMetadata(CONST_DEFAULT_THEME));
        private static DependencyProperty CurrentLanguageProperty = DependencyProperty.RegisterAttached("CurrentLanguage", typeof(String), typeof(FrameworkElement), new PropertyMetadata(CONST_DEFAULT_LANGUAGE));

        // 获取视图的当前主题
        private static String GetViewCurrentTheme(FrameworkElement element)
        {
            return element.GetValue(CurrentThemeProperty) as String;
        }
        // 设置视图的当前主题
        private static void SetViewCurrentTheme(FrameworkElement element, String currentTheme)
        {
            element.SetValue(CurrentThemeProperty, currentTheme);
        }
        // 获取视图的当前语言
        private static String GetViewCurrentLanguage(FrameworkElement element)
        {
            return element.GetValue(CurrentLanguageProperty) as String;
        }
        // 设置视图的当前主题
        private static void SetViewCurrentLanguage(FrameworkElement element, String currentLanguage)
        {
            element.SetValue(CurrentLanguageProperty, currentLanguage);
        }

        /// <summary>
        /// 视图文件目录
        /// </summary>
        public String ViewFileFolder { get; set; }

        private String currentTheme = CONST_DEFAULT_THEME;
        private String currentLanguage = CONST_DEFAULT_LANGUAGE;

        private Dictionary<Type, Type> viewModelTypeViewTypeDict = new Dictionary<Type, Type>();
        private HashSet<IViewModel> currentVisiableViewModelHashSet = new HashSet<IViewModel>();

        /// <summary>
        /// 视图模型管理器
        /// </summary>
        public IViewModelManager ViewModelManager { get; set; }
        /// <summary>
        /// 默认错误显示模板
        /// </summary>
        public ControlTemplate DefaultErrorTemplate { get; set; }
        /// <summary>
        /// 当前主题
        /// </summary>
        public String CurrentTheme
        {
            get { return currentTheme; }
            set
            {
                currentTheme = value;
                reloadView();
            }
        }
        /// <summary>
        /// 当前语言
        /// </summary>
        public String CurrentLanguage
        {
            get { return currentLanguage; }
            set
            {
                currentLanguage = value;
                reloadView();
            }
        }

        public ViewManager(String viewFileFolder)
        {
            this.ViewFileFolder = viewFileFolder;
        }

        //重新加载视图
        private void reloadView()
        {
            foreach (IViewModel viewModel in currentVisiableViewModelHashSet)
                reloadView(viewModel);
        }

        //改变主题
        private void reloadView(IViewModel viewModel)
        {
            FrameworkElement preView = viewModel.View as FrameworkElement;
            if (preView == null)
                return;
            viewModel.View = null;
            try
            {
                FrameworkElement nextView = GetView(viewModel) as FrameworkElement;
                FrameworkElementUtils.Exchange(preView, nextView);
            }
            catch (Exception ex)
            {
                viewModel.View = String.Format("Failed to reload view.CurrentTheme:[{0}];CurrentLanguage:[{1}].", CurrentTheme, CurrentLanguage);
                throw ex;
            }
        }

        public void RegisterView<TViewModelType, TViewType>()
            where TViewModelType : IViewModel
            where TViewType : class
        {
            RegisterView(typeof(TViewModelType), typeof(TViewType));
        }

        /// <summary>
        /// 注册视图
        /// </summary>
        /// <param name="viewModelType"></param>
        /// <param name="viewType"></param>
        public void RegisterView(Type viewModelType, Type viewType)
        {
            viewModelTypeViewTypeDict[viewModelType] = viewType;
        }

        /// <summary>
        /// 获取View对象
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public Object GetView(IViewModel viewModel)
        {
            Type viewModelType = ViewModelManager.GetViewModelInterfaceType(viewModel);
            if (viewModelType == null)
                return null;
            return GetView(viewModel, viewModelType);
        }

        public Object GetView(IViewModel viewModel, Type viewModelType)
        {
            //如果视图已经生成，则直接返回
            if (viewModel.View != null)
                return viewModel.View;

            FrameworkElement element = GetView(viewModelType);
            if (element == null)
                throw new ApplicationException(String.Format("Cann't found view file for ViewModel '{0}'.", viewModelType.FullName));
            //当可见性发生改变时
            element.IsVisibleChanged += (sender, e) =>
            {
                if (viewModel.View == null || viewModel.View != element)
                    return;

                if (element.IsVisible)
                {
                    if (GetViewCurrentTheme(element) == CurrentTheme
                        && GetViewCurrentLanguage(element) == CurrentLanguage)
                        currentVisiableViewModelHashSet.Add(viewModel);
                    else
                        try { reloadView(viewModel); }
                        catch { }
                }
                else
                    currentVisiableViewModelHashSet.Remove(viewModel);
            };
            SetViewCurrentTheme(element, CurrentTheme);
            SetViewCurrentLanguage(element, CurrentLanguage);
            element.DataContext = viewModel;
            viewModel.View = element;
            //设置所有控件的默认错误模板
            if (DefaultErrorTemplate != null)
                setDefaultErrorTemplate(element);

            return element;
        }

        //判断是否是WPF默认的错误模板
        private Boolean isWpfDefaultErrorTemplate(ControlTemplate controlTemplate)
        {
            if (controlTemplate.TargetType != typeof(System.Windows.Controls.Control))
                return false;
            if (!controlTemplate.IsSealed)
                return false;

            FrameworkElementFactory rootElement = controlTemplate.VisualTree;
            if (rootElement == null)
                return false;
            if (rootElement.Type != typeof(Border))
                return false;
            if (rootElement.Name != "Border")
                return false;

            return true;
        }

        private void setDefaultErrorTemplate(DependencyObject dObject)
        {
            ControlTemplate controlTemplate = Validation.GetErrorTemplate(dObject);
            //只有当是WPF默认错误模板时，才替换
            if (controlTemplate != null && isWpfDefaultErrorTemplate(controlTemplate))
                Validation.SetErrorTemplate(dObject, DefaultErrorTemplate);
            foreach (Object obj in LogicalTreeHelper.GetChildren(dObject))
            {
                if (obj is DependencyObject)
                    setDefaultErrorTemplate((DependencyObject)obj);
            }
        }

        private String getTextResourceFileContent(List<String> fileNameList, Assembly assembly, String baseFolder, String fullFileNameTemplate, params Object[] templateParams)
        {
            //文件是否存在
            Boolean isFileExists = false;
            String filePath = null;
            foreach (String fileName in fileNameList)
            {
                filePath = Path.Combine(baseFolder, fileName);
                isFileExists = File.Exists(filePath);
                if (isFileExists)
                    break;
            }

            String fileContent = null;

            //先尝试从目录加载文件
            if (isFileExists)
            {
                fileContent = File.ReadAllText(filePath);
            }
            //然后尝试从程序集资源中加载
            else
            {
                foreach (String fileName in fileNameList)
                {
                    //"{0}.View.{1}"
                    String resourceName = String.Format(fullFileNameTemplate, templateParams);
                    resourceName = resourceName.Replace("[fileName]", fileName);
                    resourceName = resourceName.Replace("-", "_");
                    Stream resourceStream = assembly.GetManifestResourceStream(resourceName);
                    if (resourceStream != null)
                    {
                        StreamReader streamReader = new StreamReader(resourceStream);
                        fileContent = streamReader.ReadToEnd();
                        streamReader.Close();
                        resourceStream.Close();
                        break;
                    }
                }
            }
            return fileContent;
        }

        private String getXamlContent(String viewModelTypeFullName, Assembly viewModelAssembly)
        {
            //视图模型接口类所在的程序集名称
            String viewModelAssemblyName = viewModelAssembly.GetName().Name;
            String viewBaseFolder = Path.Combine(ViewFileFolder, CurrentTheme, viewModelAssemblyName);

            //要搜索的可能的xaml文件名称
            List<String> viewXamlFileNameList = new List<string>() { viewModelTypeFullName + ".xaml" };
            //要搜索的可能的xaml的语言文件名称
            List<String> viewXamlLanguageFileNameList = new List<string>() { viewModelTypeFullName + ".txt" };

            if (viewModelTypeFullName.StartsWith(viewModelAssemblyName + "."))
            {
                String shortName = viewModelTypeFullName.Substring((viewModelAssemblyName + ".").Length);
                viewXamlFileNameList.Add(shortName + ".xaml");
                viewXamlLanguageFileNameList.Add(shortName + ".txt");

                if (shortName.StartsWith("ViewModel."))
                {
                    viewXamlFileNameList.Add(shortName.Substring("ViewModel.".Length) + ".xaml");
                    viewXamlLanguageFileNameList.Add(shortName.Substring("ViewModel.".Length) + ".txt");
                }
            }

            //xaml文件内容
            String xamlContent = getTextResourceFileContent(
                viewXamlFileNameList,
                viewModelAssembly,
                viewBaseFolder,
                "{0}.View.[fileName]",
                viewModelAssemblyName);
            if (xamlContent == null)
                return null;

            //此xaml文件对应的语言文件内容
            String xamlLanguageContent = getTextResourceFileContent(
                    viewXamlLanguageFileNameList,
                    viewModelAssembly,
                    Path.Combine(viewBaseFolder,"Language",CurrentLanguage),
                    "{0}.View.{1}.{2}.[fileName]",
                    viewModelAssemblyName, "Language", CurrentLanguage
                );
            if (xamlContent == null)
                return null;

            Regex regex = null;
            //替换clr namespace
            //(?'String'xmlns:.*?="(?'ClrNamespace'clr-namespace:.*?[^\\])")
            regex = new Regex("(?'String'xmlns:.*?=\"(?'ClrNamespace'clr-namespace:.*?[^\\\\])\")");
            xamlContent = regex.Replace(xamlContent, match =>
            {
                var clrNamespaceGroup = match.Groups["ClrNamespace"];

                String matchValue = match.Value;
                String clrNamespaceValue = clrNamespaceGroup.Value;

                if (clrNamespaceValue.Contains("assembly="))
                    return matchValue;
                String newValue = matchValue.Replace(clrNamespaceValue, String.Format("{0};assembly={1}", clrNamespaceValue, viewModelAssemblyName));
                return newValue;
            });
            //替换资源路径的相对路径为绝对路径
            //"(?'Resource'\.{0,2}/.*?[^\\](?'Extension'\.png|\.jpg|\.xml|\.xaml))"
            //"./Images/Folder.png"
            regex = new Regex("\"(?'Resource'\\.{0,2}/.*?[^\\\\](?'Extension'\\.png|\\.jpg|\\.xml|\\.xaml))\"");
            xamlContent = regex.Replace(xamlContent, match =>
            {
                var resourceGroup = match.Groups["Resource"];

                String matchValue = match.Value;
                String resourceValue = resourceGroup.Value;
                String newResourceUri = String.Empty;

                String resourceFullPath = Path.Combine(viewBaseFolder, resourceValue);
                //如果资源文件存在
                if (File.Exists(resourceFullPath))
                    newResourceUri = new Uri(resourceFullPath).AbsoluteUri;
                //否则从程序集资源中获取
                else
                    newResourceUri = String.Format("pack://application:,,,/{0};component/View/{1}", viewModelAssemblyName, resourceValue);
                return String.Format("\"{0}\"", newResourceUri);
            });
            //替换语言资源
            if (xamlLanguageContent != null)
            {
                //转义符字典
                Dictionary<String, String> xmlReplaceDict = new Dictionary<string, string>();
                xmlReplaceDict.Add("&", "&amp;");
                xmlReplaceDict.Add("<", "&lt;");
                xmlReplaceDict.Add(">", "&gt;");
                xmlReplaceDict.Add("\"", "&quot;");
                xmlReplaceDict.Add("'", "&apos;");

                Dictionary<Int32, String> languageDict = new Dictionary<int, string>();

                //(?'index'\d+)\s*=(?'value'.+).+
                regex = new Regex(@"(?'index'\d+)\s*=(?'value'.+)");
                MatchCollection languageMatchCollection = regex.Matches(xamlLanguageContent);
                foreach (Match match in languageMatchCollection)
                {
                    var indexGroup = match.Groups["index"];
                    var valueGroup = match.Groups["value"];

                    if (!indexGroup.Success || !valueGroup.Success)
                        continue;
                    Int32 key = Int32.Parse(indexGroup.Value);
                    String value = valueGroup.Value;
                    //替换需要转义的字符
                    if (value.Contains("{") && !value.StartsWith("{}"))
                        value = "{}" + value;
                    foreach (String replaceKey in xmlReplaceDict.Keys)
                        if (value.Contains(replaceKey))
                            value = value.Replace(replaceKey, xmlReplaceDict[replaceKey]);

                    if (languageDict.ContainsKey(key))
                        languageDict.Remove(key);
                    languageDict.Add(key, value);
                }
                //"(?'value'{}.*?)"
                regex = new Regex("\"(?'value'{}.*?)\"");
                Int32 languageIndex = 0;
                xamlContent = regex.Replace(xamlContent, match =>
                    {
                        languageIndex++;
                        if (languageDict.ContainsKey(languageIndex))
                            return String.Format("\"{0}\"", languageDict[languageIndex]);
                        return match.Value;
                    });
            }
            return xamlContent;
        }

        public FrameworkElement GetView(Type viewModelType)
        {
            //视图模型接口类所在的程序集
            Assembly viewModelAssembly = viewModelType.Assembly;
            //视图模型接口类所在的程序集名称
            String viewModelAssemblyName = viewModelAssembly.GetName().Name;
            ////视图模型接口类完整名称
            String viewModelTypeFullName = viewModelType.FullName;
            
            String xamlContent = getXamlContent(viewModelTypeFullName, viewModelAssembly);

            //如果得到了xaml内容
            if (!String.IsNullOrEmpty(xamlContent))
            {
                Regex regex = null;

                //处理#include预处理指令
                //<!--#include\("path=(?'path'.*?)(;assembly=(?'assembly'.*?))?"\)-->
                regex = new Regex("<!--#include\\(\"path=(?'path'.*?)(;assembly=(?'assembly'.*?))?\"\\)-->");
                while (regex.IsMatch(xamlContent))
                {
                    xamlContent = regex.Replace(xamlContent, match =>
                    {
                        var pathGroup = match.Groups["path"];
                        var assemblyGroup = match.Groups["assembly"];

                        String path = pathGroup.Value;
                        Assembly currentAssembly = null;

                        String fullPath = null;
                        if (assemblyGroup.Success)
                        {
                            currentAssembly = Assembly.Load(assemblyGroup.Value);
                            fullPath = path;
                        }
                        else
                        {
                            currentAssembly = viewModelAssembly;
                            if (path.StartsWith("."))
                            {
                                String viewModelFolderPath = Path.GetDirectoryName(viewModelTypeFullName.Replace('.', Path.DirectorySeparatorChar));
                                fullPath = Path.Combine(viewModelFolderPath, path).Replace(Path.DirectorySeparatorChar, '.');
                            }
                            else
                                fullPath = path;
                        }
                        return getXamlContent(fullPath, currentAssembly);
                    });
                }
                return (FrameworkElement)System.Windows.Markup.XamlReader.Parse(xamlContent);
            }

            //最后从注册的视图中加载
            if (!viewModelTypeViewTypeDict.ContainsKey(viewModelType))
                return null;
            Type viewType = viewModelTypeViewTypeDict[viewModelType];
            return (FrameworkElement)Activator.CreateInstance(viewType);
        }

        public Object GetView<TViewModelType>()
            where TViewModelType : class,IViewModel
        {
            return GetView<TViewModelType>(null);
        }

        public Object GetView<TViewModelType>(Action<TViewModelType> initAction)
            where TViewModelType : class,IViewModel
        {
            IViewModel viewModel = ViewModelManager.CreateInstance<TViewModelType>(initAction);
            if (viewModel == null)
                return null;
            return GetView(viewModel, typeof(TViewModelType));
        }
    }
}
