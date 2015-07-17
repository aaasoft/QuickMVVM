using Quick.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Quick.MVVM
{
    /// <summary>
    /// 视图管理器接口
    /// </summary>
    public interface IViewManager : IDisposable
    {
        /// <summary>
        /// 获取或设置视图模型管理器
        /// </summary>
        IViewModelManager ViewModelManager { get; set; }
        /// <summary>
        /// 获取或设置默认错误显示模板
        /// </summary>
        ControlTemplate DefaultErrorTemplate { get; set; }
        /// <summary>
        /// 获取或设置当前主题
        /// </summary>
        String CurrentTheme { get; set; }
        /// <summary>
        /// 获取或设置当前语言
        /// </summary>
        String CurrentLanguage { get; set; }
        /// <summary>
        /// 主题改变时事件
        /// </summary>
        event EventHandler ThemeChanged;
        /// <summary>
        /// 语言改变时事件
        /// </summary>
        event EventHandler LanguageChanged;
        /// <summary>
        /// 注册视图模型与视图的关系
        /// </summary>
        /// <typeparam name="TViewModelType"></typeparam>
        /// <typeparam name="TViewType"></typeparam>
        void RegisterView<TViewModelType, TViewType>()
            where TViewModelType : IViewModel
            where TViewType : class;
        /// <summary>
        /// 注册视图模型与视图的关系
        /// </summary>
        /// <param name="viewModelType"></param>
        /// <param name="viewType"></param>
        void RegisterView(Type viewModelType, Type viewType);
        /// <summary>
        /// 获取View对象
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        Object GetView(IViewModel viewModel);
        /// <summary>
        /// 获取View对象
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="viewModelType"></param>
        /// <returns></returns>
        Object GetView(IViewModel viewModel, Type viewModelType);
        /// <summary>
        /// 获取View对象
        /// </summary>
        /// <param name="viewModelType"></param>
        /// <returns></returns>
        FrameworkElement GetView(Type viewModelType);
        /// <summary>
        /// 获取View对象
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="resourcePath"></param>
        /// <returns></returns>
        FrameworkElement GetView(Assembly assembly, String resourcePath);
        /// <summary>
        /// 获取View对象
        /// </summary>
        /// <typeparam name="TViewModelType"></typeparam>
        /// <returns></returns>
        Object GetView<TViewModelType>()
            where TViewModelType : class,IViewModel;
        /// <summary>
        /// 获取View对象
        /// </summary>
        /// <typeparam name="TViewModelType"></typeparam>
        /// <param name="initAction"></param>
        /// <returns></returns>
        Object GetView<TViewModelType>(Action<TViewModelType> initAction)
            where TViewModelType : class,IViewModel;
        /// <summary>
        /// 获取语言文本
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="type">类型</param>
        /// <returns></returns>
        String GetText(String key, Type type);
        /// <summary>
        /// 获取语言文本
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns></returns>
        String GetText(Enum key);
        /// <summary>
        /// 获取全部可用的主题列表
        /// </summary>
        /// <returns></returns>
        String[] GetThemes();
        /// <summary>
        /// 得到当前主题下全部可用的语言列表
        /// </summary>
        /// <returns></returns>
        String[] GetLanguages();
        /// <summary>
        /// 获取资源文件
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        String GetResourceText(Assembly assembly, String resourceName);
        /// <summary>
        /// 获取资源
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        Stream GetResource(Assembly assembly, String resourceName);
        /// <summary>
        /// 获取资源Uri
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        Uri GetResourceUri(Assembly assembly, String resourceName);
        /// <summary>
        /// 获取指定程序集的所有资源信息
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        String[] GetResourcePaths(Assembly assembly);
    }
}
