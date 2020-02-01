using System;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// ICommand implementation.
    ///
    /// The constructor has two parameters, the first parameter is compulsory, It is used for the
    /// execute method. The second parameter is optional, it is a delegate for the CanExecute method.
    /// </summary>
    public class DelegateCommand : ICommand
    {
        public readonly Action executeAction;
        public readonly Func<bool> canExecuteAction;

        public DelegateCommand(Action execute, Func<bool> canExecute = null)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }

            executeAction = execute;
            canExecuteAction = canExecute;
        }

        #region ICommand Members

        public bool CanExecute()
        {
            return (canExecuteAction == null) ? true : canExecuteAction.Invoke();
        }

        public void Execute()
        {
            executeAction.Invoke();
        }

        #endregion
    }
}