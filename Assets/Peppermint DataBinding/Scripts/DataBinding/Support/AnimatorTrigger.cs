using System;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// Event container for AnimatorBinder
    /// </summary>
    public class AnimatorTrigger
    {
        public event Action SetEvent;
        public event Action ResetEvent;

        public void SetTrigger()
        {
            if (SetEvent != null)
            {
                SetEvent.Invoke();
            }
        }

        public void ResetTrigger()
        {
            if (ResetEvent != null)
            {
                ResetEvent.Invoke();
            }
        }
    }
}
