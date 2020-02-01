
namespace Peppermint.DataBinding
{
    public interface IBinding
    {
        // Check if the binding object is bound with source
        bool IsBound { get; }

        // Get binding source
        object Source { get; }

        // Bind with source
        void Bind(object source);

        // Unbind with source
        void Unbind();

        // Handle source property changed
        void HandleSourcePropertyChanged(object sender, string propertyName);
    }
}
