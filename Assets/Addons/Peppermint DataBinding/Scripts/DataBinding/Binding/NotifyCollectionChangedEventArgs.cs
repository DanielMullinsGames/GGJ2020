using System;
using System.Collections;
using System.Collections.Generic;

namespace Peppermint.DataBinding
{
    public enum NotifyCollectionChangedAction
    {
        Add,
        Remove,
        Replace,
        Move,
        Reset,
    }

    public struct NotifyCollectionChangedEventArgs
    {
        private object oldItem;
        private object newItem;
        private IEnumerable newItems;
        private IEnumerable oldItems;

        public NotifyCollectionChangedAction Action { get; private set; }

        public ObjectCollection NewItems
        {
            get
            {
                if (newItems != null)
                {
                    // items
                    return new ObjectCollection(newItems);
                }
                else
                {
                    // single item
                    return new ObjectCollection(newItem);
                }
            }
        }

        public ObjectCollection OldItems
        {
            get
            {
                if (oldItems != null)
                {
                    // items
                    return new ObjectCollection(oldItems);
                }
                else
                {
                    // single item
                    return new ObjectCollection(oldItem);
                }
            }
        }

        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action) : this()
        {
            if (action != NotifyCollectionChangedAction.Reset)
            {
                throw new ArgumentException("Only support Reset");
            }

            Action = action;
        }

        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object changedItem) : this()
        {
            if (action == NotifyCollectionChangedAction.Add)
            {
                newItem = changedItem;
            }
            else if (action == NotifyCollectionChangedAction.Remove)
            {
                oldItem = changedItem;
            }
            else
            {
                throw new ArgumentException("Unhandled action " + action);
            }

            Action = action;
        }

        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IEnumerable items) : this()
        {
            if (action == NotifyCollectionChangedAction.Add)
            {
                newItems = items;
            }
            else if (action == NotifyCollectionChangedAction.Remove)
            {
                oldItems = items;
            }
            else if (action == NotifyCollectionChangedAction.Move)
            {
                newItems = items;
            }
            else
            {
                throw new ArgumentException("Unhandled action " + action);
            }

            Action = action;
        }

        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object newItem, object oldItem) : this()
        {
            if (action != NotifyCollectionChangedAction.Replace)
            {
                throw new ArgumentException("Only support Replace action");
            }

            Action = action;
            this.newItem = newItem;
            this.oldItem = oldItem;
        }
    }
}
