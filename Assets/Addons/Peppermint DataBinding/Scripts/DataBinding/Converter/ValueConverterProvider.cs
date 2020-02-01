using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// ValueConverterProvider manages all value converters.
    /// </summary>
    public class ValueConverterProvider
    {
        private Dictionary<string, IValueConverter> nameDictionary;
        private MultiKeyDictionary<Type, Type, IValueConverter> typeDictionary;

        private DefaultConverter defaultConverter;

        public IValueConverter DefaultConverter
        {
            get { return defaultConverter; }
        }

        #region Singleton

        public static ValueConverterProvider Instance { get { return Nested.instance; } }

        private class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static Nested()
            {
                instance = new ValueConverterProvider();
                instance.Init();
            }

            internal static readonly ValueConverterProvider instance;
        }

        #endregion

        private ValueConverterProvider()
        {
            nameDictionary = new Dictionary<string, IValueConverter>();
            typeDictionary = new MultiKeyDictionary<Type, Type, IValueConverter>();

            // create default converter
            defaultConverter = new DefaultConverter();
        }

        private void Init()
        {
            InitializeClasses();
        }

        private void InitializeClasses()
        {
            var list = new List<Tuple<int, Type>>();
            var attributeType = typeof(InitializeValueConverterAttribute);

            // check all loaded assemblies
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (!BindingUtility.IsScriptAssembly(assembly))
                {
                    // only handle script assembly
                    continue;
                }

                // get all types
                Type[] types = null;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException)
                {
                    // ignore types that cannot be loaded
                    continue;
                }

                // iterate all types
                foreach (var type in types)
                {
                    if (!type.IsDefined(attributeType, false))
                    {
                        continue;
                    }

                    // get attribute
                    var attribute = BindingUtility.GetAttribute<InitializeValueConverterAttribute>(type, false);

                    // add it
                    list.Add(Tuple.Create(attribute.Order, type));
                }
            }

            // sort based on order
            list.Sort((lhs, rhs) => lhs.Item1.CompareTo(rhs.Item1));

            foreach (var item in list)
            {
                var type = item.Item2;

                // run class constructor
                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);
            }
        }

        /// <summary>
        /// Add a named converter
        /// </summary>
        /// <param name="name">Converter name</param>
        /// <param name="converter">Converter object</param>
        public void AddNamedConverter(string name, IValueConverter converter)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (converter == null)
            {
                throw new ArgumentNullException("converter");
            }

            if (nameDictionary.ContainsKey(name))
            {
                Debug.LogErrorFormat("Converter {0} is already added", name);
                return;
            }

            // add it
            nameDictionary.Add(name, converter);
        }

        /// <summary>
        /// Remove a named converter
        /// </summary>
        /// <param name="name">Converter name</param>
        /// <param name="converter">Converter object</param>
        public void RemoveNamedConverter(string name, IValueConverter converter)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (converter == null)
            {
                throw new ArgumentNullException("converter");
            }

            if (!nameDictionary.ContainsKey(name))
            {
                Debug.LogErrorFormat("Converter {0} is not exist", name);
                return;
            }

            // remove it
            nameDictionary.Remove(name);
        }

        /// <summary>
        /// Get a named converter
        /// </summary>
        /// <param name="name">The name to find</param>
        /// <returns>Return the converter if found, otherwise return null</returns>
        public IValueConverter GetNamedConverter(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            IValueConverter converter;
            if (!nameDictionary.TryGetValue(name, out converter))
            {
                return null;
            }

            return converter;
        }

        /// <summary>
        /// Add a converter as general converter.
        /// </summary>
        /// <param name="sourceType">The source type of this converter</param>
        /// <param name="targetType">The target type of this converter</param>
        /// <param name="converter">The converter instance</param>
        public void AddConverter(Type sourceType, Type targetType, IValueConverter converter)
        {
            if (converter == null)
            {
                throw new ArgumentNullException("converter");
            }

            if (sourceType == null)
            {
                throw new ArgumentNullException("sourceType");
            }

            if (targetType == null)
            {
                throw new ArgumentNullException("targetType");
            }

            if (sourceType == targetType)
            {
                throw new ArgumentException("source type and the target type must be different");
            }

            // check key
            if (typeDictionary.ContainsKey(sourceType, targetType))
            {
                Debug.LogErrorFormat("Converter ({0}, {1}) already exist", sourceType, targetType);
                return;
            }

            // add it
            typeDictionary.Add(sourceType, targetType, converter);
        }

        /// <summary>
        /// Remove a general converter
        /// </summary>
        /// <param name="sourceType">The source type of this converter</param>
        /// <param name="targetType">The target type of this converter</param>
        /// <param name="converter">The converter instance</param>
        public void RemoveConverter(Type sourceType, Type targetType, IValueConverter converter)
        {
            if (converter == null)
            {
                throw new ArgumentNullException("converter");
            }

            if (sourceType == null)
            {
                throw new ArgumentNullException("sourceType");
            }

            if (targetType == null)
            {
                throw new ArgumentNullException("targetType");
            }

            if (sourceType == targetType)
            {
                throw new ArgumentException("source type and the target type must be different");
            }

            // check key
            if (!typeDictionary.ContainsKey(sourceType, targetType))
            {
                Debug.LogErrorFormat("Converter ({0}, {1}) not exist", sourceType, targetType);
                return;
            }

            // remove it
            typeDictionary.Remove(sourceType, targetType);
        }


        /// <summary>
        /// Get a general converter
        /// </summary>
        /// <param name="sourceType">The source type to find</param>
        /// <param name="targetType">The target type to find</param>
        /// <returns>Return the converter if found, otherwise return null</returns>
        public IValueConverter GetConverter(Type sourceType, Type targetType)
        {
            if (typeDictionary.Count == 0)
            {
                return null;
            }

            // get converter
            IValueConverter converter = null;
            if (!typeDictionary.TryGetValue(sourceType, targetType, out converter))
            {
                return null;
            }

            return converter;
        }

        /// <summary>
        /// Get a best match converter
        /// </summary>
        /// <param name="sourceType">The source type to find</param>
        /// <param name="targetType">The target type to find</param>
        /// <returns>Return the converter if found, otherwise return null</returns>
        public IValueConverter MatchConverter(Type sourceType, Type targetType)
        {
            if (targetType.IsAssignableFrom(sourceType))
            {
                // dose not need converter
                return null;
            }

            // check if the specified value converter exists
            IValueConverter converter = null;
            if (typeDictionary.TryGetValue(sourceType, targetType, out converter))
            {
                // return found converter
                return converter;
            }

            // check if default converter is supported
            if (defaultConverter.IsSupported(sourceType, targetType))
            {
                return defaultConverter;
            }

            Debug.LogErrorFormat("Failed to find the converter. sourceType={0}, targetType={1}", sourceType, targetType);
            return null;
        }
    }
}