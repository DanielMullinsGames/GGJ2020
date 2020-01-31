using System;
using System.Collections.Generic;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// A simple object pool for CLR object.
    /// </summary>
    /// <typeparam name="T">Target type</typeparam>
    public class ObjectPool<T>
    {
        private Func<T> createFunc;
        private Action<T> resetAction;
        private Stack<T> freeObjects;

        public ObjectPool(Func<T> createFunc, Action<T> resetAction)
        {
            if (createFunc == null)
            {
                throw new ArgumentNullException("createFunc");
            }

            this.createFunc = createFunc;
            this.resetAction = resetAction;

            freeObjects = new Stack<T>();
        }

        public T GetObject()
        {
            T item;

            if (freeObjects.Count > 0)
            {
                // use pool
                item = freeObjects.Pop();
            }
            else
            {
                // create new
                item = createFunc.Invoke();
            }

            return item;
        }

        public void PutObject(T item)
        {
            if (resetAction != null)
            {
                // invoke reset
                resetAction.Invoke(item);
            }

            freeObjects.Push(item);
        }
    }
}