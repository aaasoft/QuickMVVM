using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Quick.MVVM.ViewModel
{
    public class DelegateNavigator : INavigator
    {
        private object _ViewModel;
        public object ViewModel { get { return _ViewModel; } }

        public event EventHandler Navigated;

        /// <summary>
        /// 导航Action
        /// </summary>
        public Action<IViewModel, Type> NavigateAction { get; set; }

        public void Navigate<TViewModel>() where TViewModel : class, IViewModel
        {
            Type viewModelType = typeof(TViewModel);
            IViewModel viewModel = ViewModelManager.Instance.CreateInstance<TViewModel>();
            Navigate(viewModel, viewModelType);
        }

        public void Navigate(IViewModel viewModel)
        {
            Navigate(viewModel, ViewModelManager.Instance.GetViewModelInterfaceType(viewModel));
        }

        public void Navigate(IViewModel viewModel, Type viewModelType)
        {
            if (NavigateAction == null)
                throw new ApplicationException("In class DelegateNavigator, NavigateAction is null!");
            viewModel.Navigator = this;
            _ViewModel = viewModel;
            NavigateAction.Invoke(viewModel, viewModelType);

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (Navigated != null)
                        Navigated(this, EventArgs.Empty);
                }));
        }
    }
}
