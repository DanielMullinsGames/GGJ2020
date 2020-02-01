
namespace Peppermint.DataBinding
{
    /// <summary>
    /// Interface for serializable canvas node.
    /// </summary>
    public interface ISerializableCanvasNode
    {
        // serialize to string format
        string Serialize();

        // deserialize from string format
        void Deserialize(string str);
    }
}
