using System;
using System.Reflection;
using UnityEngine;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// Accessor for property.
    /// </summary>
    public class PropertyAccessor
    {
        private PropertyInfo propertyInfo;
        private Type propertyType;

        private Action<object, object> setter;
        private Func<object, object> getter;
        private object defaultValue;

        private bool isSetterReady;
        private bool isGetterReady;

        public Type PropertyType
        {
            get { return propertyType; }
        }

        public PropertyAccessor(PropertyInfo propertyInfo)
        {
            this.propertyInfo = propertyInfo;
            this.propertyType = propertyInfo.PropertyType;
        }

        public void SetValue(object obj, object value)
        {
            if (!isSetterReady)
            {
                if (propertyInfo.CanWrite && (propertyInfo.GetSetMethod() != null))
                {
                    // create delegate
                    setter = DelegateUtility.CreateSetter(propertyInfo);

                    // get default value
                    defaultValue = TypeCache.Instance.GetDefaultValue(propertyType);
                }

                // set flag
                isSetterReady = true;
            }

            if (setter == null)
            {
                Debug.LogFormat("Property {0}.{1} has no setter", propertyInfo.DeclaringType, propertyInfo.Name);
                return;
            }

            if (value == null)
            {
                // set default value
                setter.Invoke(obj, defaultValue);
            }
            else
            {
                setter.Invoke(obj, value);
            }
        }

        public object GetValue(object obj)
        {
            if (!isGetterReady)
            {
                if (propertyInfo.CanRead && (propertyInfo.GetGetMethod() != null))
                {
                    // create delegate
                    getter = DelegateUtility.CreateGetter(propertyInfo);
                }

                // set flag
                isGetterReady = true;
            }

            if (getter == null)
            {
                Debug.LogFormat("Property {0}.{1} has no getter", propertyInfo.DeclaringType, propertyInfo.Name);
                return null;
            }

            return getter.Invoke(obj);
        }
    }
}
