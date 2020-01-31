using UnityEngine;

namespace Peppermint.DataBinding.Example
{
    public class Rectangle : BindableScriptableObject
    {
        public int id;
        public Color color;
        public string description;

        #region Bindable Properties

        public int ID
        {
            get { return id; }
            set { SetProperty(ref id, value, "ID"); }
        }

        public Color Color
        {
            get { return color; }
            set { SetProperty(ref color, value, "Color"); }
        }

        public string Description
        {
            get { return description; }
            set { SetProperty(ref description, value, "Description"); }
        }

        #endregion
    }
}