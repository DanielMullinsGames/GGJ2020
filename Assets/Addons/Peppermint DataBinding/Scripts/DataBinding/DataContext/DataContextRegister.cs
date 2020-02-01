using UnityEngine;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// Register data context to BindingManger with required source name. See more information on
    /// DataContext.
    /// </summary>
    [AddComponentMenu("Peppermint/Data Binding/Data Context/Data Context Register")]
    public class DataContextRegister : MonoBehaviour
    {
        public string requiredSource;
        private IDataContext dataContext;

        void Awake()
        {
            dataContext = GetComponent<IDataContext>();
            if (dataContext == null)
            {
                Debug.LogError("No data context", this);
                return;
            }

            BindingManager.Instance.AddDataContext(dataContext, requiredSource);
        }

        void OnDestroy()
        {
            BindingManager.Instance.RemoveDataContext(dataContext);
        }
    }
}
