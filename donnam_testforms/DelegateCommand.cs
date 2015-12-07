using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace donnam_testforms
{
    public class DelegateCommand : ICommand
    {
        private readonly Func<object, bool> canExecuteHandler;
        private readonly Action<object> executeHandler;

        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action<object> handler)
            : this(handler, (o) => true)
        {

        }

        public DelegateCommand(Action<object> handler, Func<object, bool> canExecuteHandler)
        {
            this.executeHandler = handler;
            this.canExecuteHandler = canExecuteHandler;
        }

        public bool CanExecute(object parameter)
        {
            return this.canExecuteHandler(parameter);
        }

        public void Execute(object parameter)
        {
            this.executeHandler(parameter);
        }

        protected virtual void RaiseCanExecuteChanged()
        {
            var temp = CanExecuteChanged;
            if (temp != null)
            {
                temp(this, EventArgs.Empty);
            }
        }
    }
}