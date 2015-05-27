using Quick.MVVM.Localization;
using Quick.MVVM.Utils;
using Quick.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        //类的语言资源字典
        private static Dictionary<String, Dictionary<Int32, String>> typeLanguageResourceDict = new Dictionary<String, Dictionary<int, string>>();
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
        /// 配置
        /// </summary>
        public ViewManagerConfig Config { get; set; }
        
        private String currentTheme = CONST_DEFAULT_THEME;
        private String currentLanguage = CONST_DEFAULT_LANGUAGE;

        private Dictionary<Type, Type> viewModelTypeViewTypeDict = new Dictionary<Type, Type>();
        private HashSet<IViewModel> currentVisiableViewModelHashSet = new HashSet<IViewModel>();
        //转义符字典
        Dictionary<String, String> xmlReplaceDict;

        /// <summary>
        /// 主题改变时事件
        /// </summary>
        public event EventHandler ThemeChanged;
        /// <summary>
        /// 语言改变时事件
        /// </summary>
        public event EventHandler LanguageChanged;

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
                fireEvent(ThemeChanged);
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
                fireEvent(LanguageChanged);
                reloadView();
                lock (typeLanguageResourceDict)
                {
                    typeLanguageResourceDict.Clear();
                }
            }
        }

        private void fireEvent(EventHandler eventHandler)
        {
            if (eventHandler != null)
                eventHandler.Invoke(this, EventArgs.Empty);
        }

        public ViewManager(ViewManagerConfig config)
        {
            this.Config = config;

            //XML转义符
            xmlReplaceDict = new Dictionary<string, string>();
            xmlReplaceDict.Add("&", "&amp;");
            xmlReplaceDict.Add("<", "&lt;");
            xmlReplaceDict.Add(">", "&gt;");
            xmlReplaceDict.Add("\"", "&quot;");
            xmlReplaceDict.Add("'", "&apos;");
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

        private Dictionary<Int32, String> getLanguageResourceDict(Type type)
        {
            //读取类的TextAttribute特性
            Object[] objs = type.GetCustomAttributes(typeof(TextAttribute), false);
            if (objs != null
                && objs.Length > 0)
                return getLanguageResourceDict(type.Assembly, type.FullName);
            return getLanguageResourceDict(type.Assembly, type.FullName, objs.Select(t => (TextAttribute)t).ToArray());
        }

        private Dictionary<Int32, String> getLanguageResourceDict(Assembly assembly, String resourcePath, params TextAttribute[] textAttributes)
        {
            String key = String.Format("{0};{1}", assembly.GetName().Name, resourcePath);
            lock (typeLanguageResourceDict)
            {
                if (!typeLanguageResourceDict.ContainsKey(key))
                {
                    Dictionary<Int32, String> languageResourceDict = new Dictionary<int, string>();
                    //添加到字典中
                    typeLanguageResourceDict.Add(key, languageResourceDict);

                    //==========================
                    //先从类特性中读取
                    //==========================
                    if (textAttributes != null)
                    {
                        foreach (TextAttribute textAttribute in textAttributes)
                        {
                            languageResourceDict.Add(textAttribute.Index, textAttribute.Value);
                        }
                    }
                    //==========================
                    //然后尝试从资源文件中读取
                    //==========================
                    //视图模型接口类所在的程序集名称
                    String assemblyName = assembly.GetName().Name;

                    //==========================
                    //最后搜索语言目录和主题目录下的文件
                    //==========================
                    //要搜索的可能的语言文件名称
                    List<String> languageFileNameList = new List<string>();

                    if (resourcePath.StartsWith(assemblyName + "."))
                    {
                        String shortName = resourcePath.Substring((assemblyName + ".").Length);
                        languageFileNameList.Add(shortName + this.Config.LanguageFileExtension);
                    }
                    languageFileNameList.Add(resourcePath + this.Config.LanguageFileExtension);

                    //语言目录下的语言文件内容
                    String languageBaseFolder = Path.Combine(this.Config.LanguageFolder, this.CurrentLanguage, assemblyName);
                    String languageContent = Quick.MVVM.Utils.ResourceUtils.GetResourceText(
                            languageFileNameList,
                            assembly,
                            languageBaseFolder,
                            "{0}." + this.Config.LanguagePathInAssembly + ".{1}.[fileName]",
                            assemblyName, this.CurrentLanguage
                        );
                    if (languageContent != null)
                    {
                        var tmpDict = Quick.MVVM.Utils.ResourceUtils.GetLanguageResourceDictionary(languageContent);
                        foreach (int index in tmpDict.Keys)
                        {
                            if (languageResourceDict.ContainsKey(index))
                                languageResourceDict.Remove(index);
                            languageResourceDict.Add(index, tmpDict[index]);
                        }
                    }

                    //主题目录下的语言文件内容
                    String viewBaseFolder = Path.Combine(this.Config.ThemeFolder, this.CurrentTheme, assemblyName);
                    languageContent = Quick.MVVM.Utils.ResourceUtils.GetResourceText(
                            languageFileNameList,
                            assembly,
                            Path.Combine(viewBaseFolder, "Language", this.CurrentLanguage),
                            "{0}." + this.Config.ThemePathInAssembly + ".{1}.{2}.[fileName]",
                            assemblyName, "Language", this.CurrentLanguage
                        );
                    if (languageContent != null)
                    {
                        var tmpDict = Quick.MVVM.Utils.ResourceUtils.GetLanguageResourceDictionary(languageContent);
                        foreach (int index in tmpDict.Keys)
                        {
                            if (languageResourceDict.ContainsKey(index))
                                languageResourceDict.Remove(index);
                            languageResourceDict.Add(index, tmpDict[index]);
                        }
                    }
                }
                return typeLanguageResourceDict[key];
            }
        }

        /// <summary>
        /// 获取语言文字
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="index">序号，从1开始</param>
        /// <returns></returns>
        public String GetText<T>(Int32 index)
        {
            return GetText(index, typeof(T));
        }

        /// <summary>
        /// 获取语言文字
        /// </summary>
        /// <param name="index">序号，从1开始</param>
        /// <returns></returns>
        public String GetText(Int32 index, Type type)
        {
            Dictionary<Int32, String> languageResourceDict = getLanguageResourceDict(type);
            if (languageResourceDict == null
                || !languageResourceDict.ContainsKey(index))
                return String.Format("Language Resource[Type:{0}, Index:{1}] not found!", type.FullName, index);
            return languageResourceDict[index];
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

        /// <summary>
        /// 获取资源文件
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        public String GetResourceText(Assembly assembly, String resourceName)
        {
            Stream stream = GetResource(assembly, resourceName);
            if (stream == null)
                return null;
            using (stream)
            {
                String fileContent = null;
                StreamReader streamReader = new StreamReader(stream);
                fileContent = streamReader.ReadToEnd();
                streamReader.Close();
                stream.Close();
                return fileContent;
            }
        }

        /// <summary>
        /// 获取资源
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        public Stream GetResource(Assembly assembly, String resourceName)
        {
            //视图模型接口类所在的程序集名称
            String assemblyName = assembly.GetName().Name;
            String viewBaseFolder = Path.Combine(this.Config.ThemeFolder, CurrentTheme, assemblyName);

            return ResourceUtils.GetResource(
                new List<String>() { resourceName },
                assembly,
                viewBaseFolder,
                "{0}." + this.Config.ThemePathInAssembly + ".[fileName]",
                assemblyName);
        }

        private String getXamlContent(String resourcePath, Assembly assembly)
        {
            //视图模型接口类所在的程序集名称
            String assemblyName = assembly.GetName().Name;
            String themeFolder = Path.Combine(this.Config.ThemeFolder, CurrentTheme, assemblyName);

            //要搜索的可能的xaml文件名称
            List<String> viewXamlFileNameList = new List<string>();

            if (resourcePath.StartsWith(assemblyName + "."))
            {
                String shortName = resourcePath.Substring((assemblyName + ".").Length);
                viewXamlFileNameList.Add(shortName + this.Config.ViewFileExtension);
            }
            viewXamlFileNameList.Add(resourcePath + this.Config.ViewFileExtension);

            //xaml文件内容
            String xamlContent = ResourceUtils.GetResourceText(
                viewXamlFileNameList,
                assembly,
                themeFolder,
                "{0}." + this.Config.ThemePathInAssembly + ".[fileName]",
                assemblyName);
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
                String newValue = matchValue.Replace(clrNamespaceValue, String.Format("{0};assembly={1}", clrNamespaceValue, assemblyName));
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

                String resourceFullPath = Path.Combine(themeFolder, resourceValue);
                //如果资源文件存在
                if (File.Exists(resourceFullPath))
                    newResourceUri = new Uri(resourceFullPath).AbsoluteUri;
                //否则从程序集资源中获取
                else
                    newResourceUri = String.Format("pack://application:,,,/{0};component/" + this.Config.ThemePathInAssembly + "/{1}", assemblyName, resourceValue);
                return String.Format("\"{0}\"", newResourceUri);
            });
            //替换语言资源
            Dictionary<Int32, String> languageDict = getLanguageResourceDict(assembly, resourcePath);
            if (languageDict != null)
            {
                //"(?'value'{}.*?)"
                regex = new Regex("\"(?'value'{}.*?)\"");
                Int32 languageIndex = 0;
                xamlContent = regex.Replace(xamlContent, match =>
                    {
                        languageIndex++;
                        if (languageDict.ContainsKey(languageIndex))
                        {
                            String value = languageDict[languageIndex];
                            foreach (String replaceKey in xmlReplaceDict.Keys)
                                if (value.Contains(replaceKey))
                                    value = value.Replace(replaceKey, xmlReplaceDict[replaceKey]);
                            return String.Format("\"{0}\"", value);
                        }
                        return match.Value;
                    });
            }
            return xamlContent;
        }

        public FrameworkElement GetView(Assembly assembly, String resourcePath)
        {
            String xamlContent = getXamlContent(resourcePath, assembly);

            //如果得到了xaml内容
            if (String.IsNullOrEmpty(xamlContent))
                return null;
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
                        currentAssembly = assembly;
                        if (path.StartsWith("."))
                        {
                            String viewModelFolderPath = Path.GetDirectoryName(resourcePath.Replace('.', Path.DirectorySeparatorChar));
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

        public FrameworkElement GetView(Type viewModelType)
        {
            //先从注册的视图中加载
            if (viewModelTypeViewTypeDict.ContainsKey(viewModelType))
            {
                Type viewType = viewModelTypeViewTypeDict[viewModelType];
                return (FrameworkElement)Activator.CreateInstance(viewType);
            }
            //然后从主题目录和程序集资源中加载
            return GetView(viewModelType.Assembly, viewModelType.FullName);
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


        public string[] GetThemes()
        {
            Collection<String> collection = new Collection<string>();
            System.IO.DirectoryInfo viewDi = new System.IO.DirectoryInfo(this.Config.ThemeFolder);
            if (viewDi.Exists)
            {
                foreach (var themeDi in viewDi.GetDirectories())
                    collection.Add(themeDi.Name);
            }
            if (!collection.Contains(ViewManager.CONST_DEFAULT_THEME))
                collection.Insert(0, ViewManager.CONST_DEFAULT_THEME);
            return collection.ToArray();
        }

        public string[] GetLanguages(string theme)
        {
            Collection<String> collection = new Collection<string>();

            System.IO.DirectoryInfo viewDi = new System.IO.DirectoryInfo(Path.Combine(this.Config.ThemeFolder, this.CurrentTheme));
            //先从文件中读取
            if (viewDi.Exists)
            {
                foreach (var assemblyDi in viewDi.GetDirectories())
                {
                    System.IO.DirectoryInfo languageDi = new DirectoryInfo(Path.Combine(assemblyDi.FullName, "Language"));
                    if (!languageDi.Exists)
                        continue;
                    foreach (String languageName in languageDi.GetDirectories().Select(t => t.Name))
                    {
                        if (!collection.Contains(languageName))
                            collection.Add(languageName);
                    }
                }
            }
            if (!collection.Contains(ViewManager.CONST_DEFAULT_LANGUAGE))
                collection.Insert(0, ViewManager.CONST_DEFAULT_LANGUAGE);
            return collection.ToArray();
        }
    }
}
