using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// Value converter for implicit operator.
    /// </summary>
    [InitializeValueConverter]
    public partial class ImplicitConverter : IValueConverter
    {
        private ImplicitOperatorAccessor convert;
        private ImplicitOperatorAccessor convertBack;

        static ImplicitConverter()
        {
            CreateConverters();
        }

        private static void CreateConverters()
        {
            // get type list
            var types = GetTypes();

            // get implicit operators
            var operatorDictionary = BindingUtility.GetImplicitOperators(types);

            var converterDictionary = new Dictionary<Tuple<Type, Type>, ImplicitConverter>();
            foreach (var item in operatorDictionary)
            {
                var key = item.Key;
                var pairedKey = Tuple.Create(key.Item2, key.Item1);

                // get methods
                MethodInfo convertMethod = item.Value;
                MethodInfo convertBackMethod;
                operatorDictionary.TryGetValue(pairedKey, out convertBackMethod);

                // create new converter
                var converter = new ImplicitConverter(convertMethod, convertBackMethod);

                // add it
                converterDictionary.Add(key, converter);
            }

            // add converters
            foreach (var item in converterDictionary)
            {
                var key = item.Key;
                var converter = item.Value;

                ValueConverterProvider.Instance.AddConverter(key.Item1, key.Item2, converter);
            }
        }

        public static List<Type> GetTypes()
        {
            var typeList = new List<Type>();

            // get generated type names
            var names = GetTypeNames();

            var missingList = new List<string>();

            // check all loaded assemblies
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var name in names)
            {
                bool found = false;

                foreach (var assembly in assemblies)
                {
                    try
                    {
                        // get type
                        var type = assembly.GetType(name);

                        if (type != null)
                        {
                            // add it
                            typeList.Add(type);

                            // set flag and break
                            found = true;
                            break;
                        }
                    }
                    catch (Exception)
                    {
                        // ignore types that cannot be loaded
                        continue;
                    }
                }

                if (!found)
                {
                    // add to missing list
                    missingList.Add(name);
                }
            }

            // check type missing
            if (missingList.Count != 0)
            {
                var sb = new StringBuilder();
                for (int i = 0; i < missingList.Count; i++)
                {
                    if (i != 0)
                    {
                        sb.Append(", ");
                    }

                    sb.Append(missingList[i]);
                }

                Debug.LogWarningFormat("Type not found, count: {0}, typeNames: [{1}].", missingList.Count, sb.ToString());
            }

            return typeList;
        }

        public ImplicitConverter(MethodInfo convertMethod, MethodInfo convertBackMethod)
        {
            if (convertMethod != null)
            {
                convert = TypeCache.Instance.GetImplicitOperatorAccessor(convertMethod);
            }

            if (convertBackMethod != null)
            {
                convertBack = TypeCache.Instance.GetImplicitOperatorAccessor(convertBackMethod);
            }
        }

        public object Convert(object value, Type targetType)
        {
            if (convert == null)
            {
                throw new InvalidOperationException("convert is null");
            }

            var targetValue = convert.Convert(value);
            return targetValue;
        }

        public object ConvertBack(object value, Type targetType)
        {
            if (convertBack == null)
            {
                throw new InvalidOperationException("convertBack is null");
            }

            var targetValue = convertBack.Convert(value);
            return targetValue;
        }
    }
}
