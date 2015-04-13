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
        private String viewFileFolder;
        private String currentTheme = "Default";
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
        public String CurrentTheme
        {
            get { return currentTheme; }
            set
            {
                currentTheme = value;
                changeTheme(value);
            }
        }

        public ViewManager(String viewFileFolder)
        {
            this.viewFileFolder = viewFileFolder;

            
        }

        //改变主题
        private void changeTheme(string themeName)
        {

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
                    currentVisiableViewModelHashSet.Add(viewModel);
                else
                    currentVisiableViewModelHashSet.Remove(viewModel);
            };
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

        public FrameworkElement GetView(Type viewModelType)
        {
            //视图模型接口类所在的程序集
            Assembly viewModelAssembly = viewModelType.Assembly;
            //视图模型接口类所在的程序集名称
            String viewModelAssemblyName = viewModelAssembly.GetName().Name;
            ////视图模型接口类完整名称
            String viewModelTypeFullName = viewModelType.FullName;
            //当前View的基础目录
            String currentViewBaseFolder = Path.Combine(viewFileFolder, currentTheme, viewModelAssemblyName);

            //要搜索的可能的xaml文件名称
            List<String> viewXamlFileNameList = new List<string>()
            {
                viewModelTypeFullName
            };
            if (viewModelTypeFullName.StartsWith(viewModelAssemblyName + "."))
            {
                String shortName = viewModelTypeFullName.Substring((viewModelAssemblyName + ".").Length);
                viewXamlFileNameList.Add(shortName);
                if (shortName.StartsWith("ViewModel."))
                    viewXamlFileNameList.Add(shortName.Substring("ViewModel.".Length));
            }

            //视图的xaml文件是否存在
            Boolean isViewXamlFileExists = false;
            String viewXamlFilePath = null;
            foreach (String viewXamlFileName in viewXamlFileNameList)
            {
                viewXamlFilePath = Path.Combine(currentViewBaseFolder, viewXamlFileName + ".xaml");
                isViewXamlFileExists = File.Exists(viewXamlFilePath);
                if (isViewXamlFileExists)
                    break;
            }

            String xamlContent = null;

            //先尝试从View目录加载视图
            if (isViewXamlFileExists)
            {
                xamlContent = File.ReadAllText(viewXamlFilePath);
            }
            //然后尝试从程序集资源中加载
            else
            {
                foreach (String viewXamlFileName in viewXamlFileNameList)
                {
                    String resourceName = String.Format("{0}.View.{1}.xaml", viewModelAssemblyName, viewXamlFileName);
                    Stream resourceStream = viewModelAssembly.GetManifestResourceStream(resourceName);
                    if (resourceStream != null)
                    {
                        StreamReader streamReader = new StreamReader(resourceStream);
                        xamlContent = streamReader.ReadToEnd();
                        streamReader.Close();
                        resourceStream.Close();
                        break;
                    }
                }
            }

            //如果得到了xaml内容
            if (!String.IsNullOrEmpty(xamlContent))
            {
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

                    String resourceFullPath = Path.Combine(currentViewBaseFolder, resourceValue);
                    //如果资源文件存在
                    if (File.Exists(resourceFullPath))
                        newResourceUri = new Uri(resourceFullPath).AbsoluteUri;
                    //否则从程序集资源中获取
                    else
                        newResourceUri = String.Format("pack://application:,,,/{0};component/View/{1}", viewModelAssemblyName, resourceValue);
                    return String.Format("\"{0}\"", newResourceUri);
                });

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
