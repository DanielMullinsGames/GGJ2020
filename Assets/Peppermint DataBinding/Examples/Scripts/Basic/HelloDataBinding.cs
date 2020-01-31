
namespace Peppermint.DataBinding.Example
{
    public class HelloDataBinding : BindableMonoBehaviour
    {
        private string message;

        #region Bindable Properties

        public string Message
        {
            get { return message; }
            set { SetProperty(ref message, value, "Message"); }
        }

        #endregion
        
        void Start()
        {
            Message = "Hello, DataBinding!";

            // add current object as binding source
            BindingManager.Instance.AddSource(this, typeof(HelloDataBinding).Name);
        }

        void OnDestroy()
        {
            // remove source
            BindingManager.Instance.RemoveSource(this);
        }
    }
}
