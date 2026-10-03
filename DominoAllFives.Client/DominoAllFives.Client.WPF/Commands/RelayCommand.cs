using System;
using System.Windows.Input;

namespace DominoAllFives.Client.WPF.Commands
{
    /// <summary>
    /// Implements interface ICommand to delegate actions to UI views.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class.
        /// </summary>
        public RelayCommand()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class with parameterized delegates.
        /// </summary>
        /// <param name="execute">The execution logic delegate.</param>
        /// <param name="canExecute">The execution status evaluation delegate.</param>
        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class with parameterless delegates.
        /// </summary>
        /// <param name="execute">The execution logic delegate.</param>
        /// <param name="canExecute">The execution status evaluation delegate.</param>
        public RelayCommand(Action execute, Func<bool> canExecute = null)
            : this(
                execute != null ? new Action<object>(unusedParameter => execute()) : null,
                canExecute != null ? new Predicate<object>(unusedParameter => canExecute()) : null)
        {
        }

        /// <summary>
        /// Determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command.</param>
        /// <returns>True if this command can be executed; otherwise, false.</returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        /// <summary>
        /// Executes the command on the current command target.
        /// </summary>
        /// <param name="parameter">Data used by the command.</param>
        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event to force UI re-evaluation of the command status.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
