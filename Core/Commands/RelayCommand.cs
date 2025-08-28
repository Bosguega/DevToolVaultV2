// DevToolVaultV2/Core/Commands/RelayCommand.cs
using System;
using System.Windows.Input;

namespace DevToolVaultV2.Core.Commands
{
    public class RelayCommand : ICommand
    {
        // Corrigido: readonly
        private readonly Action _execute;
        // Corrigido: readonly
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;
        public void Execute(object parameter) => _execute();
        public event EventHandler CanExecuteChanged
        {
            // Corrigido: RequerySuggested
            add { CommandManager.RequerySuggested += value; }
            // Corrigido: RequerySuggested
            remove { CommandManager.RequerySuggested -= value; }
        }
    }

    public class RelayCommand<T> : ICommand
    {
        // Corrigido: readonly
        private readonly Action<T> _execute;
        // Corrigido: readonly
        private readonly Func<T, bool> _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke((T)parameter) ?? true;
        public void Execute(object parameter) => _execute((T)parameter);
        public event EventHandler CanExecuteChanged
        {
            // Corrigido: RequerySuggested
            add { CommandManager.RequerySuggested += value; }
            // Corrigido: RequerySuggested
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}