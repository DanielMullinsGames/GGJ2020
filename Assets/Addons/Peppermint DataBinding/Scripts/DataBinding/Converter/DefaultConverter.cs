using System;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// Default value converter, it uses System.Convert.ChangeType to convert type.
    /// </summary>
    public class DefaultConverter : IValueConverter
    {
        private readonly Type interfaceType;
        private readonly Type stringType;
        private readonly Type decimalType;

        public DefaultConverter()
        {
            // cache types
            interfaceType = typeof(IConvertible);
            stringType = typeof(string);
            decimalType = typeof(Decimal);
        }

        public bool IsSupported(Type sourceType, Type targetType)
        {
            if (targetType == stringType)
            {
                // convert to string is always supported
                return true;
            }

            // check nullable type
            var underlyingType = Nullable.GetUnderlyingType(sourceType);
            if (underlyingType != null)
            {
                // only handle if the type is the same
                if (underlyingType == targetType)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            if (!interfaceType.IsAssignableFrom(sourceType))
            {
                // not implemented IConvertible
                return false;
            }

            if (targetType.IsPrimitive || targetType.IsEnum || targetType == decimalType)
            {
                // target type is supported
                return true;
            }

            return false;
        }

        private object ChangeType(object value, Type targetType)
        {
            if (targetType == null)
            {
                throw new ArgumentNullException("targetType");
            }

            if (value == null)
            {
                return null;
            }

            var sourceType = value.GetType();
            if (targetType.IsAssignableFrom(sourceType))
            {
                // does not need convert
                return value;
            }

            if (targetType.IsEnum)
            {
                // convert to enum
                if (value is string)
                {
                    var enumValue = Enum.Parse(targetType, (string)value);
                    return enumValue;
                }
                else
                {
                    var number = System.Convert.ChangeType(value, Enum.GetUnderlyingType(targetType));
                    var enumValue = Enum.ToObject(targetType, number);
                    return enumValue;
                }
            }

            if (targetType == stringType)
            {
                // convert to string
                var stringValue = value.ToString();
                return stringValue;
            }

            // use internal convert
            var objectValue = System.Convert.ChangeType(value, targetType);
            return objectValue;
        }

        #region IValueConverter Members

        public object Convert(object value, Type targetType)
        {
            return ChangeType(value, targetType);
        }

        public object ConvertBack(object value, Type targetType)
        {
            return ChangeType(value, targetType);
        }

        #endregion
    }
}