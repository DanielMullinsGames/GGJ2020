using System;
using System.Collections.Generic;
using UnityEngine;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// The core of data binding. It maintains weak connections between source and data context,
    /// which means the source does not known whether a specified data context exists, and vice
    /// versa.
    ///
    /// The source object you added must has a unique name, if the name match the requirement of data
    /// context, the manager will bind the source with matched data context. When the source is
    /// removed, the manager will unbind the source with data context.
    ///
    /// Usually the source is added from the view model class, and data context is added by
    /// DataContextRegister.
    /// </summary>
    public class BindingManager
    {
        private Dictionary<string, List<IDataContext>> dataContextDictionary;
        private Dictionary<IDataContext, string> dataContextNameDictionary;

        private Dictionary<string, object> sourceDictionary;
        private Dictionary<object, string> sourceNameDictionary;
        private bool enableDebug = false;

        #region Singleton

        public static BindingManager Instance { get { return Nested.instance; } }

        private class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static Nested()
            {
            }

            internal static readonly BindingManager instance = new BindingManager();
        }

        #endregion

        private BindingManager()
        {
            dataContextDictionary = new Dictionary<string, List<IDataContext>>();
            dataContextNameDictionary = new Dictionary<IDataContext, string>();

            sourceDictionary = new Dictionary<string, object>();
            sourceNameDictionary = new Dictionary<object, string>();
        }

        private void BindSource(string sourceName)
        {
            // get context list
            List<IDataContext> list;
            if (!dataContextDictionary.TryGetValue(sourceName, out list))
            {
                // no context
                return;
            }

            // get source
            object source;
            if (!sourceDictionary.TryGetValue(sourceName, out source))
            {
                // no source
                return;
            }

            if (enableDebug)
            {
                Debug.Log("BindSource: " + sourceName);
            }

            // set source
            foreach (var item in list)
            {
                item.Source = source;
            }
        }

        private void UnbindSource(string sourceName)
        {
            // get context list
            List<IDataContext> list;
            if (!dataContextDictionary.TryGetValue(sourceName, out list))
            {
                // no context
                return;
            }

            if (enableDebug)
            {
                Debug.Log("UnbindSource: " + sourceName);
            }

            // reset source
            foreach (var item in list)
            {
                item.Source = null;
            }
        }

        public void AddDataContext(IDataContext dataContext, string sourceName)
        {
            if (dataContext == null)
            {
                throw new ArgumentNullException("dataContext");
            }

            if (sourceName == null)
            {
                throw new ArgumentNullException("sourceName");
            }

            if (enableDebug)
            {
                Debug.LogFormat("AddDataContext: {0}, requiredSource: {1}", dataContext, sourceName);
            }

            if (dataContextNameDictionary.ContainsKey(dataContext))
            {
                Debug.LogErrorFormat("DataContext {0} is already registered.", dataContext);
                return;
            }

            List<IDataContext> list;
            if (!dataContextDictionary.TryGetValue(sourceName, out list))
            {
                // add new list
                list = new List<IDataContext>();

                // add to dictionary
                dataContextDictionary.Add(sourceName, list);
            }

            // add to list
            list.Add(dataContext);

            // add to dictionary
            dataContextNameDictionary.Add(dataContext, sourceName);

            // check if source exists
            object source = null;
            if (sourceDictionary.TryGetValue(sourceName, out source))
            {
                // set source
                dataContext.Source = source;
            }
        }

        public void RemoveDataContext(IDataContext dataContext)
        {
            if (dataContext == null)
            {
                return;
            }

            if (enableDebug)
            {
                Debug.LogFormat("RemoveDataContext: {0}", dataContext);
            }

            string sourceName;
            if (!dataContextNameDictionary.TryGetValue(dataContext, out sourceName))
            {
                Debug.LogErrorFormat("DataContext {0} is unregistered.", dataContext);
                return;
            }

            List<IDataContext> list;
            if (!dataContextDictionary.TryGetValue(sourceName, out list))
            {
                // context is unregistered
                Debug.LogErrorFormat("DataContext {0} is unregistered.", dataContext);
                return;
            }

            // remove from list
            list.Remove(dataContext);

            dataContextNameDictionary.Remove(dataContext);

            // remove empty list
            if (list.Count == 0)
            {
                dataContextDictionary.Remove(sourceName);
            }

            // reset source
            dataContext.Source = null;
        }

        public void AddSource(object source, string sourceName)
        {
            if (sourceName == null)
            {
                throw new ArgumentNullException("sourceName");
            }

            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (enableDebug)
            {
                Debug.LogFormat("AddSource, sourceName={0}, source={1}", sourceName, source);
            }

            if (sourceDictionary.ContainsKey(sourceName))
            {
                Debug.LogWarningFormat("SourceName {0} is already added.", sourceName);
                return;
            }

            if (sourceNameDictionary.ContainsKey(source))
            {
                Debug.LogWarningFormat("Source {0} is already added.", source);
                return;
            }

            // add to dictionary
            sourceDictionary.Add(sourceName, source);
            sourceNameDictionary.Add(source, sourceName);

            BindSource(sourceName);
        }

        public void RemoveSource(object source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (enableDebug)
            {
                Debug.LogFormat("RemoveSource, source={0}", source);
            }

            string name = null;
            if (!sourceNameDictionary.TryGetValue(source, out name))
            {
                // unregistered source
                return;
            }

            sourceDictionary.Remove(name);
            sourceNameDictionary.Remove(source);

            UnbindSource(name);
        }

        public object GetSource(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name is null");
            }

            object source = null;
            sourceDictionary.TryGetValue(name, out source);

            return source;
        }

        public void Clear()
        {
            foreach (var item in dataContextDictionary)
            {
                var list = item.Value;

                // iterate all data context
                foreach (var dataContext in list)
                {
                    // reset source
                    dataContext.Source = null;
                }
            }

            // clear dictionary
            sourceDictionary.Clear();
            sourceNameDictionary.Clear();
            dataContextDictionary.Clear();
            dataContextNameDictionary.Clear();
        }

        public void NotifyPropertyChanged(object source, string propertyName)
        {
            DoNotifyProperty(source, source, propertyName);
        }

        public void NotifyPropertyChanged(object source, object bindingSource, string propertyName)
        {
            DoNotifyProperty(source, bindingSource, propertyName);
        }

        public void NotifyItemPropertyChanged(object source, object collectionObject, object itemObject, string propertyName)
        {
            DoNotifyItemProperty(source, source, collectionObject, itemObject, itemObject, propertyName);
        }

        public void NotifyItemPropertyChanged(object source, object bindingSource, object collectionObject, object itemObject, object itemBindingSource, string propertyName)
        {
            DoNotifyItemProperty(source, bindingSource, collectionObject, itemObject, itemBindingSource, propertyName);
        }

        private void DoNotifyProperty(object source, object bindingSource, string propertyName)
        {
            // get context list
            List<IDataContext> list = GetDataContextList(source);

            if (list == null)
            {
                return;
            }

            foreach (var dataContext in list)
            {
                NotifyDataContext(dataContext, bindingSource, propertyName);
            }
        }

        private void DoNotifyItemProperty(object source, object bindingSource, object collectionObject, object itemObject, object itemBindingSource, string propertyName)
        {
            // get context list
            List<IDataContext> list = GetDataContextList(source);

            if (list == null)
            {
                return;
            }

            foreach (var dataContext in list)
            {
                if (!dataContext.IsBound)
                {
                    continue;
                }

                foreach (var binding in dataContext.BindingList)
                {
                    // check source
                    if (binding.Source != bindingSource)
                    {
                        continue;
                    }

                    var collectionBinding = binding as CollectionBinding;
                    if (collectionBinding == null)
                    {
                        // skip non collection binding
                        continue;
                    }

                    // check collection
                    if (collectionBinding.ItemsSource != collectionObject)
                    {
                        continue;
                    }

                    var itemDataContext = collectionBinding.GetDataContext(itemObject);
                    if (itemDataContext == null)
                    {
                        // invalid item
                        continue;
                    }

                    NotifyDataContext(itemDataContext, itemBindingSource, propertyName);
                }
            }
        }

        private void NotifyDataContext(IDataContext dataContext, object bindingSource, string propertyName)
        {
            if (!dataContext.IsBound)
            {
                return;
            }

            foreach (var binding in dataContext.BindingList)
            {
                // check source
                if (binding.Source != bindingSource)
                {
                    continue;
                }

                binding.HandleSourcePropertyChanged(bindingSource, propertyName);
            }
        }

        private List<IDataContext> GetDataContextList(object source)
        {
            if (source == null)
            {
                return null;
            }

            string sourceName = null;
            if (!sourceNameDictionary.TryGetValue(source, out sourceName))
            {
                // unregistered source
                return null;
            }

            // get context list
            List<IDataContext> list;
            if (!dataContextDictionary.TryGetValue(sourceName, out list))
            {
                // no context
                return null;
            }

            return list;
        }

    }
}
