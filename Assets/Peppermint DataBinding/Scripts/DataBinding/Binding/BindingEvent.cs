using System;
using System.Collections.Generic;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// Custom event class for better performance.
    /// </summary>
    public class BindingEvent
    {
        private List<Action<object, string>> actionList;

        public BindingEvent()
        {
            actionList = new List<Action<object, string>>();
        }

        // add action to the list, mimic Delegate.Combine
        public void Add(Action<object, string> action)
        {
            if (action == null)
            {
                return;
            }

            // does not check for duplicated entry
            actionList.Add(action);
        }

        // remove action from the list, mimic Delegate.Remove
        public void Remove(Action<object, string> action)
        {
            if (action == null)
            {
                return;
            }

            actionList.Remove(action);
        }

        public void Invoke(object sender, string propertyName)
        {
            int count = actionList.Count;
            for (int i = 0; i < count; i++)
            {
                var action = actionList[i];
                action.Invoke(sender, propertyName);
            }
        }
    }
}
