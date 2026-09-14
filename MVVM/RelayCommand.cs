using System.Windows.Input;
using TCM_Launcher.Core.Utils;

namespace TCM_Launcher.MVVM
{
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> execute;
        private readonly Predicate<T> canExecute;

        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter)
            => canExecute == null || canExecute((T)parameter!);

        public void Execute(object? parameter)
            => execute((T)parameter);
    }

    public class AsyncRelayCommand<T> : ICommand
    {
        private readonly Func<T, Task> execute;
        private readonly Predicate<T>? canExecute;
        private bool isExecuting;

        public AsyncRelayCommand(Func<T, Task> execute, Predicate<T>? canExecute = null)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter)
        {
            if (isExecuting) return false;

            return canExecute == null || canExecute((T)parameter!);
        }

        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter)) return;

            try
            {
                isExecuting = true;
                CommandManager.InvalidateRequerySuggested();

                await execute((T)parameter!);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception occurred during async command execution.", ex);
            }
            finally
            {
                isExecuting = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }
}
