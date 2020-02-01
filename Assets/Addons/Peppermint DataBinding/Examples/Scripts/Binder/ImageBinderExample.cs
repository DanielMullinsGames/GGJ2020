
namespace Peppermint.DataBinding.Example
{
    /// <summary>
    /// This example demonstrates how to setup ImageBinder.
    ///
    /// ImageBinder is different than other built-in binders, it must specify a converter name. So to
    /// setup the ImageBinder, you also need a SpriteNameConverter and a SpriteSet.
    ///
    /// The SpriteNameConverter is a string to sprite converter, it use the SpriteSet internally to
    /// find the sprite by its name. Both ImageBinder and SpriteNameConverter has a "Converter
    /// Name", if the name matches, the ImageBinder will use this converter.
    ///
    /// In this demo, the first row of icons use the "BinderIcon" converter, and the second row use
    /// the "ShopIcon". We set the sprite names directly in the code, but in real game you need to
    /// load external data to set the icon name properties.
    ///
    /// Notice that we assigned some invalid icon names. The SpriteNameConverter can be specified
    /// different dummy sprites if the sprite name is not found. In the code we specify the same
    /// invalid names, but the sprite we get from the converter is different.
    ///
    /// The built-in SpriteNameConverter just do a simple name lookup from its SpriteSet. You can
    /// easily create your own string to sprite converter to support different lookup, e.g. load the
    /// sprite from Resource folder.
    /// </summary>
    public class ImageBinderExample : BindableMonoBehaviour
    {
        private string iconA0;
        private string iconA1;
        private string iconA2;
        private string iconA3;
        private string iconA4;
        private string iconA5;

        private string iconB0;
        private string iconB1;
        private string iconB2;
        private string iconB3;
        private string iconB4;

        #region Bindable Properties

        public string IconA0
        {
            get { return iconA0; }
            set { SetProperty(ref iconA0, value, "IconA0"); }
        }

        public string IconA1
        {
            get { return iconA1; }
            set { SetProperty(ref iconA1, value, "IconA1"); }
        }

        public string IconA2
        {
            get { return iconA2; }
            set { SetProperty(ref iconA2, value, "IconA2"); }
        }

        public string IconA3
        {
            get { return iconA3; }
            set { SetProperty(ref iconA3, value, "IconA3"); }
        }

        public string IconA4
        {
            get { return iconA4; }
            set { SetProperty(ref iconA4, value, "IconA4"); }
        }

        public string IconA5
        {
            get { return iconA5; }
            set { SetProperty(ref iconA5, value, "IconA5"); }
        }

        public string IconB0
        {
            get { return iconB0; }
            set { SetProperty(ref iconB0, value, "IconB0"); }
        }

        public string IconB1
        {
            get { return iconB1; }
            set { SetProperty(ref iconB1, value, "IconB1"); }
        }

        public string IconB2
        {
            get { return iconB2; }
            set { SetProperty(ref iconB2, value, "IconB2"); }
        }

        public string IconB3
        {
            get { return iconB3; }
            set { SetProperty(ref iconB3, value, "IconB3"); }
        }

        public string IconB4
        {
            get { return iconB4; }
            set { SetProperty(ref iconB4, value, "IconB4"); }
        }

        #endregion

        void Start()
        {
            InitValue();

            BindingManager.Instance.AddSource(this, typeof(ImageBinderExample).Name);
        }

        void OnDestroy()
        {
            BindingManager.Instance.RemoveSource(this);
        }

        private void InitValue()
        {
            // set sprite name
            IconA0 = "OneWay";
            IconA1 = "TwoWay";
            IconA2 = "OneWayToSource";
            IconA3 = null;
            IconA4 = string.Empty;
            IconA5 = "InvalidName";

            IconB0 = "Gold";
            IconB1 = "Gem";
            IconB2 = null;
            IconB3 = string.Empty;
            IconB4 = "InvalidName";
        }
    }
}
