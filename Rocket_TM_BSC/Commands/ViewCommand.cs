using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Rocket_TM_BSC.Commands
{
    public class ViewCommand : ICommand
    {
        public Action<object> ExecuteViewCommand { get; set; }
        public Func<object, bool> CanExecuteViewCommand { get; set; }

        public ViewCommand(Action<object> executeViewCommand, Func<object, bool> canExecuteViewCommand)
        {
            this.ExecuteViewCommand = executeViewCommand;
            this.CanExecuteViewCommand = canExecuteViewCommand;
        }

        public bool CanExecute(object parameter)
        {
            return CanExecuteViewCommand(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, new EventArgs());
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            ExecuteViewCommand(parameter);
        }
    }
}
