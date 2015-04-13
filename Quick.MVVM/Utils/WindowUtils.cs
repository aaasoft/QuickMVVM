using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

namespace Quick.MVVM.Utils
{
    public class WindowUtils
    {
        /// <summary>
        /// 设置窗体居中
        /// </summary>
        /// <param name="window"></param>
        public static void SetWindowPositionToScreenCenter(Window window)
        {
            //设置窗体居中
            window.Left = (SystemParameters.FullPrimaryScreenWidth - window.Width) / 2;
            window.Top = (SystemParameters.FullPrimaryScreenHeight - window.Height) / 2;
        }

        /// <summary>
        /// 设置窗体居于Owner中间
        /// </summary>
        /// <param name="window"></param>
        public static void SetWindowPositionToOwnerCenter(Window window)
        {
            Window ownerWindow = window.Owner;
            if (ownerWindow == null || ownerWindow.WindowState == WindowState.Maximized)
            {
                SetWindowPositionToScreenCenter(window);
                return;
            }
            //计算Left
            double left = ownerWindow.Left + (ownerWindow.Width - window.Width) / 2;
            if (left + window.Width > SystemParameters.FullPrimaryScreenWidth)
                left = SystemParameters.FullPrimaryScreenWidth - window.Width;
            if (left < 0) left = 0;
            //计算Top
            double top = ownerWindow.Top + (ownerWindow.Height - window.Height) / 2;
            if (top + window.Height > SystemParameters.FullPrimaryScreenHeight)
                top = SystemParameters.FullPrimaryScreenHeight - window.Height;
            if (top < 0) top = 0;

            window.Left = left;
            window.Top = top;
        }

        /// <summary>
        /// 设置窗体的大小适合内容
        /// </summary>
        /// <param name="window"></param>
        public static void SetWindowSizeFitContent(Window window)
        {
            window.SizeToContent = SizeToContent.WidthAndHeight;
            window.SizeToContent = SizeToContent.Manual;
        }

        public static Boolean IsWindowRootBrowserWindow(Window owner)
        {
            return owner != null
                && owner.GetType().FullName == "MS.Internal.AppModel.RootBrowserWindow";
        }

        /// <summary>
        /// 得到对象所属的窗口
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static Window GetWindow(Object element)
        {
            DependencyObject currentDpObj = element as DependencyObject;
            Window win = null;
            while (true)
            {
                if (currentDpObj == null || currentDpObj is Window)
                {
                    win = currentDpObj as Window;
                    break;
                }
                currentDpObj = VisualTreeHelper.GetParent(currentDpObj);
            }
            return win;
        }
    }
}
