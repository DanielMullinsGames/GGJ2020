using System;
using System.Collections.Generic;
using Peppermint.DataBinding;
using UnityEngine;

public class SynchronousEventSystem 
{
    public static SynchronousEventSystem EventSystem;

    private Dictionary<Type, EventPosterBase> mHandlers = new Dictionary<Type, EventPosterBase>(100);

    private abstract class EventPosterBase
    {
        public abstract bool IsEmpty { get; }
        public abstract void DeliverEvent(EventBase evt);

    }

    private class EventPoster<T> : EventPosterBase where T : EventBase
    {
        private event EventHandler<T> mListeners = null;
        public event EventHandler<T> Listen
        {
            add
            {
#if UNITY_EDITOR
                //Check for dupes
                if (mListeners != null)
                {
                    if (Array.IndexOf(mListeners.GetInvocationList(), value) >= 0)
                    {
                        Debug.LogError(string.Format("Attempted to add duplicate delegate {0} to lister of type {1}. Ignoring!", value, typeof(T)));
                        return;
                    }
                }
#endif
                mListeners += value;
            }
            remove
            {
#if UNITY_EDITOR
                //Check for existence
                bool found = false;
                int nlisteners = 0;
                if (mListeners != null)
                {
                    Delegate[] listeners = mListeners.GetInvocationList();
                    nlisteners = listeners.Length;
                    if (Array.IndexOf(listeners, value) >= 0)
                    {
                        found = true;
                    }
                }

                if (!found)
                {
                    Debug.LogError(string.Format("Synchronous event receiver {0} not currently registered to event type {1} ({2} current listeners)", value, typeof(T), nlisteners));
                    return;
                }
#endif
                mListeners -= value;
            }
        }

        public override bool IsEmpty { get { return mListeners == null; } }
        public override void DeliverEvent(EventBase evt)
        {
            mListeners?.Invoke((T)evt);
        }
    }

    static SynchronousEventSystem()
    {
        EventSystem = new SynchronousEventSystem();
    }

    public SynchronousEventSystem()
    {
    }

    public void AddHandler<T>(EventHandler<T> handler) where T : EventBase
    {
        EventPosterBase poster;
        if (!mHandlers.TryGetValue(typeof(T), out poster))
        {
            poster = new EventPoster<T>();
            mHandlers[typeof(T)] = poster;
        }

        ((EventPoster<T>)poster).Listen += handler;
    }

    public void RemoveHandler<T>(EventHandler<T> handler) where T : EventBase
    {
        EventPosterBase basePoster;
        if (!mHandlers.TryGetValue(typeof(T), out basePoster))
        {
            Debug.LogError(string.Format("Unable to remove handler {0} of type {1} because no handlers are currently registered for this type", handler, typeof(T)));
            return;
        }

        EventPoster<T> poster = (EventPoster<T>)basePoster;
        poster.Listen -= handler;

        if (poster.IsEmpty)
            mHandlers.Remove(typeof(T));
    }

    public void RemoveAllHandlers()
    {
        mHandlers.Clear();
    }

    public void Post(EventBase evt)
    {
        Type posterType = evt.GetType();
        do
        {
            if (mHandlers.TryGetValue(posterType, out EventPosterBase handler))
                handler.DeliverEvent(evt);

            posterType = posterType.BaseType;
        }
        while (posterType != typeof(EventBase));
    }

    public bool HasHandler(EventBase evt)
    {
        bool foundReceiver = false;
        Type posterType = evt.GetType();
        do
        {
            EventPosterBase handler;
            if (mHandlers.TryGetValue(posterType, out handler))
            {
                foundReceiver = true;
                break;
            }

            posterType = posterType.BaseType;
        }
        while (posterType != typeof(EventBase));

        return foundReceiver;
    }
}

public delegate void EventHandler<T>(T ev) where T : EventBase;