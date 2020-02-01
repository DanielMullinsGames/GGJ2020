using UnityEngine;
using System.Collections;
using Peppermint.DataBinding;

public class PlayerDatabinder : BindableMonoBehaviour
{
    #region bindable properties

    #endregion

    private void Start()
    {
        BindingManager.Instance.AddSource(this, typeof(PlayerDatabinder).Name);
    }

    private void OnDestroy()
    {
        BindingManager.Instance.RemoveSource(this);
    }
}
