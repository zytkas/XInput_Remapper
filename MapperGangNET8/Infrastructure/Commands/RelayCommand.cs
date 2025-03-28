using System;
using System.Windows.Input;

namespace MapperGangNET8.Infrastructure.Commands
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>
        /// Создает новую команду
        /// </summary>
        /// <param name="execute">Делегат выполнения команды</param>
        /// <param name="canExecute">Делегат для проверки возможности выполнения команды</param>
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Определяет, может ли команда выполняться в текущем состоянии
        /// </summary>
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        /// <summary>
        /// Выполняет команду
        /// </summary>
        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}