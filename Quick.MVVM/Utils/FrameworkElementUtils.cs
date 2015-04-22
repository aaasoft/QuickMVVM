using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Quick.MVVM.Utils
{
    public class FrameworkElementUtils
    {
        /// <summary>
        /// 交换FrameworkElement
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public static void Exchange(FrameworkElement source, FrameworkElement target)
        {
            Window win = Window.GetWindow(source);
            if (win != null)
            {
                if (win.Content == source)
                {
                    win.Content = null;
                    win.Content = target;
                    return;
                }
            }

            //source的父控件
            Object sourceParent = LogicalTreeHelper.GetParent(source);
            if (sourceParent == null)
            {
                
                if (source is ContentControl)
                {
                    ContentControl elementControl = (ContentControl)source;
                    elementControl.Content = null;
                    elementControl.Content = target;
                }
            }
            else if (sourceParent is ContentControl)
            {
                ContentControl parent = (ContentControl)sourceParent;
                parent.Content = null;
                parent.Content = target;
            }
            else if (sourceParent is Page)
            {
                Page parent = (Page)sourceParent;
                parent.Content = null;
                parent.Content = target;
            }
            else if (sourceParent is Panel)
            {
                UIElement targetElement = target as UIElement;
                UIElement sourceElement = source as UIElement;

                Panel parent = (Panel)sourceParent;
                int preIndex = parent.Children.IndexOf(sourceElement);
                parent.Children.Remove(sourceElement);
                parent.Children.Insert(preIndex, targetElement);
            }
        }
    }
}
