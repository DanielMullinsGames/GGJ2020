
namespace Peppermint.DataBinding.Example
{
    public enum ColorTag
    {
        Red,
        Green,
        Blue,
    }

    public class CollectionItem : BindableObject
    {
        private ColorTag tag;
        private int index;
        private string name;
        private ICommand removeCommand;

        public override string ToString()
        {
            return string.Format("Item {0}", index);
        }

        #region Properties

        public ColorTag Tag
        {
            get { return tag; }
            set { SetProperty(ref tag, value, "Tag"); }
        }

        public int Index
        {
            get { return index; }
            set { SetProperty(ref index, value, "Index"); }
        }

        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value, "Name"); }
        }

        public ICommand RemoveCommand
        {
            get { return removeCommand; }
            set { SetProperty(ref removeCommand, value, "RemoveCommand"); }
        }

        #endregion
    }
}
