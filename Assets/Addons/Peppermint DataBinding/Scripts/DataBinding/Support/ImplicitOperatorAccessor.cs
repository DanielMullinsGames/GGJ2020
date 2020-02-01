using System;
using System.Reflection;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// Accessor for implicit operator.
    /// </summary>
    public class ImplicitOperatorAccessor
    {
        private MethodInfo methodInfo;
        private Func<object, object> function;
        private object defaultValue;
        private bool isReady;

        public MethodInfo MethodInfo
        {
            get { return methodInfo; }
        }

        public ImplicitOperatorAccessor(MethodInfo methodInfo)
        {
            this.methodInfo = methodInfo;
        }

        public object Convert(object value)
        {
            if (!isReady)
            {
                // create delegate
                function = DelegateUtility.CreateImplicitOperator(methodInfo);

                // get default value
                defaultValue = TypeCache.Instance.GetDefaultValue(methodInfo.ReturnType);

                // set flag
                isReady = true;
            }

            if (value == null)
            {
                // use default value
                return defaultValue;
            }

            object converted = function.Invoke(value);
            return converted;
        }
    }
}
