using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


namespace Peppermint.DataBinding
{
    public static class BindingUtility
    {
        public const char PathSeperatorChar = '.';

        public static IDataContext FindDataContext(Transform current)
        {
            IDataContext result = null;

            while (current != null)
            {
                // get component
                result = current.GetComponent<IDataContext>();
                if (result != null)
                {
                    break;
                }

                // go up
                current = current.parent;
            }

            return result;
        }

        public static string GetPropertyName(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path is null");
            }

            string result = null;

            if (path.IndexOf(PathSeperatorChar) == -1)
            {
                // use source path as property name
                result = path;
            }
            else
            {
                int startIndex = path.LastIndexOf(PathSeperatorChar) + 1;
                if (startIndex >= (path.Length - 1))
                {
                    throw new ArgumentException("Invalid path " + path);
                }

                // get the last property name
                result = path.Substring(startIndex);
            }

            return result;
        }

        public static object GetBindingObject(object obj, string path)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj is null");
            }

            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path is null");
            }

            // check if path is nested path
            if (path.IndexOf(PathSeperatorChar) == -1)
            {
                return obj;
            }

            // get nested object
            var names = path.Split(PathSeperatorChar);

            Type type = obj.GetType();
            object nestedObject = obj;

            for (int i = 0; i < names.Length - 1; i++)
            {
                var propertyName = names[i];

                // get property from type
                var propertyInfo = type.GetProperty(propertyName);
                if (propertyInfo == null)
                {
                    throw new ArgumentException(string.Format("Failed to find property {0} in {1}", path, obj));
                }

                // get value
                nestedObject = propertyInfo.GetValue(nestedObject, null);

                // get nested type
                type = propertyInfo.PropertyType;
            }

            // return the binding object
            return nestedObject;
        }

        public static object GetPropertyValue(object obj, string propertyName)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj is null");
            }

            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException("propertyName is null");
            }

            var sourcePropertyInfo = obj.GetType().GetProperty(propertyName);
            if (sourcePropertyInfo == null)
            {
                UnityEngine.Debug.LogError(string.Format("Failed to find property {0} in {1}", propertyName, obj.GetType()));
                return null;
            }

            var value = sourcePropertyInfo.GetValue(obj, null);
            return value;
        }

        public static void SetGameObjectActive(GameObject gameObject, bool value)
        {
            if (gameObject.activeSelf != value)
            {
                gameObject.SetActive(value);
            }
        }

        public static void AddBinding(IBinding binding, Transform transform, out IDataContext dataContext)
        {
            dataContext = FindDataContext(transform);

            if (dataContext == null)
            {
                UnityEngine.Debug.LogError("Failed to find DataContext", transform);
            }
            else
            {
                // add to data context
                dataContext.AddBinding(binding);
            }
        }

        public static void AddBinding(IEnumerable<IBinding> bindings, Transform transform, out IDataContext dataContext)
        {
            dataContext = FindDataContext(transform);

            if (dataContext == null)
            {
                UnityEngine.Debug.LogError("Failed to find DataContext", transform);
            }
            else
            {
                // add to data context
                dataContext.AddBinding(bindings);
            }
        }

        public static void RemoveBinding(IBinding binding, IDataContext dataContext)
        {
            if (dataContext != null)
            {
                dataContext.RemoveBinding(binding);
            }
        }

        public static void RemoveBinding(IEnumerable<IBinding> bindings, IDataContext dataContext)
        {
            if (dataContext != null)
            {
                dataContext.RemoveBinding(bindings);
            }
        }

        public static T GetAttribute<T>(Type type, bool inherit) where T : Attribute
        {
            var attributes = (T[])type.GetCustomAttributes(typeof(T), inherit);

            if (attributes.Length == 0)
            {
                return null;
            }

            return attributes[0];
        }

        public static Dictionary<Tuple<Type, Type>, MethodInfo> GetImplicitOperators(IEnumerable<Type> types)
        {
            var dictionary = new Dictionary<Tuple<Type, Type>, MethodInfo>();

            // search all types
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);

                foreach (var method in methods)
                {
                    if (method.Name != "op_Implicit")
                    {
                        continue;
                    }

                    var parameters = method.GetParameters();
                    if (parameters.Length != 1)
                    {
                        // with 1 parameter
                        continue;
                    }

                    var parameter = parameters[0];

                    // add operator
                    var key = Tuple.Create(parameter.ParameterType, method.ReturnType);
                    dictionary.Add(key, method);
                }
            }

            return dictionary;
        }

        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }


        public static void SyncListToDictionary<T1, T2>(IList<T1> sourceList, IDictionary<T1, T2> targetDictionary, IList<T1> tempList,
            Action<T1> addItem, Action<T1> removeItem)
            where T1 : class
            where T2 : class
        {
            tempList.Clear();

            // check removed items
            foreach (var kvp in targetDictionary)
            {
                if (!sourceList.Contains(kvp.Key))
                {
                    // mark for delete
                    tempList.Add(kvp.Key);
                }
            }

            if (tempList.Count > 0)
            {
                foreach (var item in tempList)
                {
                    // remove item
                    removeItem.Invoke(item);
                }

                tempList.Clear();
            }

            // check added items
            foreach (var item in sourceList)
            {
                if (!targetDictionary.ContainsKey(item))
                {
                    // add item
                    addItem.Invoke(item);
                }
            }
        }

        public static void SyncListOrder<T1, T2>(IList<T1> sourceList, IList<T2> targetList, IDictionary<T1, T2> sourceToTargetMap, IList<T1> tempList)
            where T1 : class
            where T2 : class
        {
            if (targetList.Count != sourceToTargetMap.Count)
            {
                throw new ArgumentException("Invalid parameters");
            }

            var observableList = targetList as ObservableList<T2>;
            if (observableList != null)
            {
                // disable notifying
                observableList.IsNotifying = false;
            }

            int index = 0;
            bool dirty = false;

            tempList.Clear();
            foreach (var item in sourceList)
            {
                if (tempList.Contains(item))
                {
                    // skip duplicate item
                    continue;
                }

                tempList.Add(item);

                var target = sourceToTargetMap[item];

                if (targetList[index] != target)
                {
                    // update list entry
                    targetList[index] = target;

                    // mark dirty
                    dirty = true;
                }

                // update index
                index++;
            }
            tempList.Clear();

            if (observableList != null)
            {
                // enable notify and notify move event
                observableList.IsNotifying = true;

                if (dirty)
                {
                    observableList.NotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, observableList));
                }
            }
        }

        public static void SyncViewOrder<T1, T2>(IList<T1> sourceList, IDictionary<T1, T2> sourceToTargetMap, IList<T1> tempList) where T2 : Component
        {
            tempList.Clear();

            // update view order
            int index = 0;
            foreach (var item in sourceList)
            {
                // get view
                T2 view;
                if (!sourceToTargetMap.TryGetValue(item, out view))
                {
                    UnityEngine.Debug.LogErrorFormat("Unknown item {0}", item);
                    continue;
                }

                if (tempList.Contains(item))
                {
                    // skip duplicate item
                    continue;
                }

                tempList.Add(item);

                // set order
                int siblingIndex = view.transform.GetSiblingIndex();
                if (siblingIndex != index)
                {
                    view.transform.SetSiblingIndex(index);
                }

                // update index
                index++;
            }

            tempList.Clear();
        }

        public static void SyncViewOrder<T1>(IList<T1> sourceList, IDictionary<T1, GameObject> sourceToTargetMap, IList<T1> tempList)
        {
            tempList.Clear();

            // update view order
            int index = 0;
            foreach (var item in sourceList)
            {
                // get view
                GameObject view;
                if (!sourceToTargetMap.TryGetValue(item, out view))
                {
                    UnityEngine.Debug.LogErrorFormat("Unknown item {0}", item);
                    continue;
                }

                if (tempList.Contains(item))
                {
                    // skip duplicate item
                    continue;
                }

                tempList.Add(item);

                // set order
                int siblingIndex = view.transform.GetSiblingIndex();
                if (siblingIndex != index)
                {
                    view.transform.SetSiblingIndex(index);
                }

                // update index
                index++;
            }

            tempList.Clear();
        }

        public static bool IsScriptAssembly(Assembly assembly)
        {
            var name = assembly.GetName().Name;

            if (name.StartsWith("Unity"))
            {
                // ignore unity
                return false;
            }

            if (name.StartsWith("System") || name.StartsWith("Mono") || name.StartsWith("mscorlib"))
            {
                // ignore system
                return false;
            }

            return true;
        }
    }
}
