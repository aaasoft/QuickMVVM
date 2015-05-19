using Quick.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Demo.Test
{
    public interface IView : IViewModel
    {

    }

    class _IView : QM_ViewModelBase, IView
    {
        public override void Init()
        {

        }
    }
}