using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quick.MVVM.ViewModel
{
    /// <summary>
    /// 界面导航器
    /// </summary>
    public interface INavigator
    {
        Object ViewModel { get; }
        event EventHandler Navigated;
        void Navigate<TViewModel>() where TViewModel : class, IViewModel;

        void Navigate(IViewModel viewModel);

        void Navigate(IViewModel viewModel, Type viewModelType);
    }
}
