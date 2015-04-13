using Quick.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quick.MVVM
{
    /// <summary>
    /// 视图模型管理器接口
    /// </summary>
    public interface IViewModelManager
    {
        /// <summary>
        /// 注册视图模型
        /// </summary>
        /// <typeparam name="TViewModelType"></typeparam>
        /// <typeparam name="TViewModelImplType"></typeparam>
        void RegisterViewModel<TViewModelType, TViewModelImplType>();
        /// <summary>
        /// 注册视图模型
        /// </summary>
        /// <param name="viewModelType"></param>
        /// <param name="viewModelImplType"></param>
        void RegisterViewModel(Type viewModelType, Type viewModelImplType);
        /// <summary>
        /// 创建视图模型的实例
        /// </summary>
        /// <typeparam name="TViewModelType"></typeparam>
        /// <returns></returns>
        TViewModelType CreateInstance<TViewModelType>()
            where TViewModelType : class,IViewModel;
        /// <summary>
        /// 创建视图模型的实例
        /// </summary>
        /// <typeparam name="TViewModelType"></typeparam>
        /// <param name="initAction"></param>
        /// <returns></returns>
        TViewModelType CreateInstance<TViewModelType>(Action<TViewModelType> initAction)
            where TViewModelType : class,IViewModel;
        /// <summary>
        /// 创建视图模型的实例
        /// </summary>
        /// <param name="viewModelType"></param>
        /// <returns></returns>
        IViewModel CreateInstance(Type viewModelType);
        /// <summary>
        /// 创建视图模型的实例
        /// </summary>
        /// <param name="viewModelType"></param>
        /// <param name="initAction"></param>
        /// <returns></returns>
        IViewModel CreateInstance(Type viewModelType, Action<IViewModel> initAction);
        /// <summary>
        /// 得到视图模型对象的接口类型
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        Type GetViewModelInterfaceType(IViewModel viewModel);
    }
}
