using Quick.MVVM.Localization;
using Quick.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Demo
{
    public interface ILogin : IViewModel
    {
        /// <summary>
        /// 用户名
        /// </summary>
        String UserName { get; set; }
        /// <summary>
        /// 登录命令(PasswordBox控件作为此命令的参数)
        /// </summary>
        ICommand Login { get; }
        /// <summary>
        /// 改变主题
        /// </summary>
        ICommand ChangeTheme { get; }
        /// <summary>
        /// 改变语言
        /// </summary>
        ICommand ChangeLanguage { get; }
        /// <summary>
        /// 测试命令
        /// </summary>
        ICommand Test { get; }
    }

    class _ILogin : QM_ViewModelBase, ILogin
    {
        /// <summary>
        /// 文本资源
        /// </summary>
        [TextResource]
        public enum Texts
        {
            [Text("正在准备数据库连接...")]
            WAIT_PREPERA_DB,
            [Text("正在验证用户名和密码...")]
            WAIT_VERIFY_USER_PASSWORD,
            [Text("正在准备数据...")]
            WAIT_PREPERA_DATA,
            [Text("用户名密码验证失败!")]
            ERROR_USER_PASSWORD_INCORRECT,
            [Text("设置")]
            BUTTON_SETTING
        }

        private String _UserName;
        public string UserName
        {
            get { return _UserName; }
            set
            {
                _UserName = value;
                ((DelegateCommand)Login).RaiseCanExecuteChanged();
            }
        }

        public System.Windows.Input.ICommand Login { get; private set; }
        public System.Windows.Input.ICommand ChangeTheme { get; private set; }
        public System.Windows.Input.ICommand ChangeLanguage { get; private set; }
        public System.Windows.Input.ICommand Test { get; private set; }

        public override void Init()
        {
            var abc = Start.ViewManager.GetText(Texts.WAIT_PREPERA_DATA);
            Login = new DelegateCommand()
            {
                ExecuteCommand = executeCommand_Login,
                CanExecuteCommand = canExecuteCommand_Login
            };
            ChangeTheme = new DelegateCommand() { ExecuteCommand = executeCommand_ChangeTheme };
            ChangeLanguage = new DelegateCommand() { ExecuteCommand = executeCommand_ChangeLanguage };
            Test = new DelegateCommand() { ExecuteCommand = executeCommand_Test };
        }

        /// <summary>
        /// 执行登录命令
        /// </summary>
        /// <param name="obj"></param>
        private void executeCommand_Login(Object obj)
        {
            Navigator.Navigate<IMain>();
        }

        /// <summary>
        /// 返回能否执行登录命令
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool canExecuteCommand_Login(Object obj)
        {
            return !String.IsNullOrWhiteSpace(UserName);
        }

        private void executeCommand_ChangeTheme(object argument)
        {
            Start.ViewManager.CurrentTheme = argument.ToString();
        }

        private void executeCommand_ChangeLanguage(object argument)
        {
            Start.ViewManager.CurrentLanguage = argument.ToString();
        }

        private void executeCommand_Test(object obj)
        {
            new Window()
            {
                Content = Start.ViewManager.GetView<Test.IView>()
            }.ShowDialog();
        }
    }
}