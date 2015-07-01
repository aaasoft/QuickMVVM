using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Quick.MVVM.View.TriggerActions
{
    public class InvokeCommandAction : TriggerAction<DependencyObject>
    {
        // Fields
        private string commandName;
        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter", typeof(object), typeof(InvokeCommandAction), null);
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(InvokeCommandAction), null);

        // Methods
        protected override void Invoke(object parameter)
        {
            if (base.AssociatedObject != null)
            {
                Object currentParameter = this.CommandParameter;
                if (currentParameter == null)
                    currentParameter = parameter;

                ICommand command = this.ResolveCommand();
                if ((command != null) && command.CanExecute(currentParameter))
                {
                    command.Execute(currentParameter);
                }
            }
        }

        private ICommand ResolveCommand()
        {
            ICommand command = null;
            if (this.Command != null)
            {
                return this.Command;
            }
            if (base.AssociatedObject != null)
            {
                foreach (PropertyInfo info in base.AssociatedObject.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (typeof(ICommand).IsAssignableFrom(info.PropertyType) && string.Equals(info.Name, this.CommandName, StringComparison.Ordinal))
                    {
                        command = (ICommand)info.GetValue(base.AssociatedObject, null);
                    }
                }
            }
            return command;
        }

        // Properties
        public ICommand Command
        {
            get
            {
                return (ICommand)base.GetValue(CommandProperty);
            }
            set
            {
                base.SetValue(CommandProperty, value);
            }
        }

        public string CommandName
        {
            get
            {
                base.ReadPreamble();
                return this.commandName;
            }
            set
            {
                if (this.CommandName != value)
                {
                    base.WritePreamble();
                    this.commandName = value;
                    base.WritePostscript();
                }
            }
        }

        public object CommandParameter
        {
            get
            {
                return base.GetValue(CommandParameterProperty);
            }
            set
            {
                base.SetValue(CommandParameterProperty, value);
            }
        }
    }
}
