
namespace Peppermint.DataBinding
{
    public interface ICommand
    {
        // Return true if command can be executed, otherwise return false
        bool CanExecute();

        // Execute command
        void Execute();
    }
}
