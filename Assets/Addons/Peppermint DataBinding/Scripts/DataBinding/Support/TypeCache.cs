using System;
using System.Collections.Generic;
using System.Reflection;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// A simple cache manager for data binding
    /// </summary>
    public class TypeCache
    {
        private MultiKeyDictionary<Type, string, PropertyAccessor> propertyAccessorDictionary;
        private Dictionary<MemberInfo, ImplicitOperatorAccessor> implicitOperatorAccessorDictionary;
        private Dictionary<Type, object> defaultValueDictionary;

        #region Singleton

        public static TypeCache Instance { get { return Nested.instance; } }

        private class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static Nested()
            {
            }

            internal static readonly TypeCache instance = new TypeCache();
        }

        #endregion

        private TypeCache()
        {
            propertyAccessorDictionary = new MultiKeyDictionary<Type, string, PropertyAccessor>();
            implicitOperatorAccessorDictionary = new Dictionary<MemberInfo, ImplicitOperatorAccessor>();
            defaultValueDictionary = new Dictionary<Type, object>();
        }

        public PropertyAccessor GetPropertyAccessor(PropertyInfo propertyInfo)
        {
            var sourceType = propertyInfo.DeclaringType;
            var propertyName = propertyInfo.Name;

            return GetPropertyAccessor(sourceType, propertyName);
        }

        public PropertyAccessor GetPropertyAccessor(Type sourceType, string propertyName)
        {
            // check if accessor exists
            PropertyAccessor accessor;
            if (propertyAccessorDictionary.TryGetValue(sourceType, propertyName, out accessor))
            {
                // just return it
                return accessor;
            }

            // get property info
            var propertyInfo = sourceType.GetProperty(propertyName);
            if (propertyInfo == null)
            {
                // invalid property name
                return null;
            }

            // create new accessor
            accessor = new PropertyAccessor(propertyInfo);

            // add it
            propertyAccessorDictionary.Add(sourceType, propertyName, accessor);

            return accessor;
        }

        public ImplicitOperatorAccessor GetImplicitOperatorAccessor(MethodInfo methodInfo)
        {
            // check if accessor exists
            ImplicitOperatorAccessor accessor;
            if (implicitOperatorAccessorDictionary.TryGetValue(methodInfo, out accessor))
            {
                // just return it
                return accessor;
            }

            // create new accessor
            accessor = new ImplicitOperatorAccessor(methodInfo);

            // add it
            implicitOperatorAccessorDictionary.Add(methodInfo, accessor);

            return accessor;
        }

        public object GetDefaultValue(Type type)
        {
            if (!type.IsValueType)
            {
                // null for reference type
                return null;
            }

            object value;
            if (defaultValueDictionary.TryGetValue(type, out value))
            {
                return value;
            }

            // create new instance
            value = Activator.CreateInstance(type);

            // add it
            defaultValueDictionary.Add(type, value);

            return value;
        }
    }
}
