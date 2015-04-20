using Quick.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Quick.MVVM
{
    /// <summary>
    /// 视图管理器接口
    /// </summary>
    public interface IViewManager
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
        /// 视图文件目录
        /// </summary>
        String ViewFileFolder { get; set; }
        /// <summary>
        /// 获取或设置当前主题
        /// </summary>
        String CurrentTheme { get; set; }
        /// <summary>
        /// 获取或设置当前语言
        /// </summary>
        String CurrentLanguage { get; set; }
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
    }
}
