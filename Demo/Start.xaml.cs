using Quick.MVVM;
using Quick.MVVM.View;
using Quick.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace Demo
{
    /// <summary>
    /// Start.xaml 的交互逻辑
    /// </summary>
    public partial class Start : Page
    {
        public static IViewManager ViewManager;
        public static IViewModelManager ViewModelManager;

        private Boolean isLoaded = false;

        static Start()
        {
            String baseDirectory = Path.GetDirectoryName(typeof(Start).Assembly.Location);
            ViewModelManager = new ViewModelManager();
            ViewManager = new ViewManager(Path.Combine(baseDirectory, "Theme"), "Theme")
            {
                ViewModelManager = ViewModelManager
            };
        }

        public Start()
        {
            
            InitializeComponent();
        }
        
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (isLoaded) return;
            isLoaded = true;

            //隐藏导航栏
            this.ShowsNavigationUI = false;
            NavigateToLogin();
        }

        private void NavigateToLogin()
        {
            NavigationService mainNavigationService = this.NavigationService;
            mainNavigationService.Navigated += (sender2, e2) =>
            {
                while (mainNavigationService.CanGoBack)
                    mainNavigationService.RemoveBackEntry();
            };

            //视图模型导航器
            INavigator navigator = new DelegateNavigator(ViewModelManager)
            {
                NavigateAction = (arg_viewModel, arg_viewModelType) =>
                {
                    Object view = ViewManager.GetView(arg_viewModel, arg_viewModelType);
                    if (view == null)
                        throw new ApplicationException(String.Format("Navigate to '{0}' failed!", arg_viewModelType.FullName));
                    mainNavigationService.Navigate(view);
                }
            };
            navigator.Navigate<Demo.ILogin>();
        }
    }
}
