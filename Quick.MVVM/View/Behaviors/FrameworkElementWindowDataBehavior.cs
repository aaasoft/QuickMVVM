using Quick.MVVM.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Interop;

namespace Quick.MVVM.View.Behaviors
{
    public class FrameworkElementWindowDataBehavior : Behavior<FrameworkElement>
    {
        public WindowState WindowState { get; set; }
        public WindowStyle WindowStyle { get; set; }
        public WindowStartupLocation WindowStartupLocation { get; set; }
        public SizeToContent SizeToContent { get; set; }
        public ResizeMode ResizeMode { get; set; }
        public String Title { get; set; }

        public FrameworkElementWindowDataBehavior()
        {
            WindowState = WindowState.Normal;
            WindowStyle = WindowStyle.SingleBorderWindow;
            WindowStartupLocation = WindowStartupLocation.Manual;
            SizeToContent = SizeToContent.Manual;
            ResizeMode = ResizeMode.CanResize;
            Title = String.Empty;
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            FrameworkElement element = base.AssociatedObject;
            RoutedEventHandler handler = null;
            handler = (sender, e) =>
                {
                    element.Loaded -= handler;
                    Window win = WindowUtils.GetWindow(element);

                    //如果是浏览器承载的窗口，则不支持设置窗体的属性
                    if (BrowserInteropHelper.IsBrowserHosted
                        && WindowUtils.IsWindowRootBrowserWindow(win))
                        return;

                    if (!String.IsNullOrEmpty(Title))
                        win.Title = Title;
                    element.Dispatcher.BeginInvoke(new Action(() => win.SizeToContent = SizeToContent));
                    element.Dispatcher.BeginInvoke(new Action(() => win.WindowState = WindowState));
                    element.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (win.AllowsTransparency == false)
                            win.WindowStyle = WindowStyle;
                    }));
                    element.Dispatcher.BeginInvoke(new Action(() => win.ResizeMode = ResizeMode));
                    element.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            win.WindowStartupLocation = WindowStartupLocation;
                            switch (WindowStartupLocation)
                            {
                                case WindowStartupLocation.CenterScreen:
                                    WindowUtils.SetWindowPositionToScreenCenter(win);
                                    break;
                                case WindowStartupLocation.CenterOwner:
                                    WindowUtils.SetWindowPositionToOwnerCenter(win);
                                    break;
                            }
                        }));
                };
            element.Loaded += handler;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
        }
    }
}
