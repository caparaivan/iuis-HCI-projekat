using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NetworkService
{
    public class ClassICommand : ICommand
    {
        Action TargetExecuteMethod;
        Func<bool> TargetCanExecuteMethod;

        public ClassICommand(Action executeMethod)
        {
            TargetExecuteMethod = executeMethod;
        }

        public ClassICommand(Action executeMethod, Func<bool> canExecuteMethod)
        {
            TargetExecuteMethod = executeMethod;
            TargetCanExecuteMethod = canExecuteMethod;
        }

        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, EventArgs.Empty);
            }
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            if (TargetCanExecuteMethod != null)
            {
                return TargetCanExecuteMethod();
            }

            if (TargetExecuteMethod != null)
            {
                return true;
            }

            return false;
        }

        public void Execute(object parameter)
        {
            TargetExecuteMethod?.Invoke();
        }
    }

    public class ClassICommand<T> : ICommand
    {
        Action<T> TargetExecuteMethod;
        Func<T, bool> TargetCanExecuteMethod;

        public ClassICommand(Action<T> executeMethod)
        {
            TargetExecuteMethod = executeMethod;
        }

        public ClassICommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod)
        {
            TargetExecuteMethod = executeMethod;
            TargetCanExecuteMethod = canExecuteMethod;
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged(this, EventArgs.Empty);
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            if (TargetCanExecuteMethod != null)
            {
                T tparm = (T)parameter;
                return TargetCanExecuteMethod(tparm);
            }

            if (TargetExecuteMethod != null)
            {
                return true;
            }

            return false;
        }

        public void Execute(object parameter)
        {
            TargetExecuteMethod?.Invoke((T)parameter);
        }
    }
}
