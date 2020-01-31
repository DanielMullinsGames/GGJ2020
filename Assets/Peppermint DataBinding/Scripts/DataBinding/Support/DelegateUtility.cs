using System;
using System.Reflection;
using UnityEngine;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// Delegate utility that create fast delegate from methodInfo
    /// </summary>
    public static class DelegateUtility
    {
        private static MethodInfo getterOpenImpl;
        private static MethodInfo setterOpenImpl;
        private static MethodInfo implicitOperatorOpenImpl;

        private static readonly bool enableDebug = true;

        public static Func<object, object> CreateGetter(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                throw new ArgumentNullException("propertyInfo");
            }

            var sourceType = propertyInfo.DeclaringType;
            var methodInfo = propertyInfo.GetGetMethod();

            if (methodInfo == null)
            {
                throw new InvalidOperationException("methodInfo is null");
            }

            if (getterOpenImpl == null)
            {
                // get the generic method
                getterOpenImpl = typeof(DelegateUtility).GetMethod("CreateGetterImpl", BindingFlags.Static | BindingFlags.Public);
            }

            try
            {
                // make generic method
                MethodInfo closedImpl = getterOpenImpl.MakeGenericMethod(sourceType, methodInfo.ReturnType);

                // call method
                Func<object, object> fastCall = (Func<object, object>)closedImpl.Invoke(null, new object[] { methodInfo });
                return fastCall;
            }
            catch (Exception)
            {
                if (enableDebug)
                {
                    Debug.LogErrorFormat("Call CreateGetterImpl<{0}, {1}>() failed, property={2}.{3}",
                        sourceType, methodInfo.ReturnType, propertyInfo.DeclaringType, propertyInfo.Name);
                }

                // call method failed, use reflection
                Func<object, object> reflectionCall = (object obj) => propertyInfo.GetValue(obj, null);
                return reflectionCall;
            }
        }

        public static Func<object, object> CreateGetterImpl<TSource, TResult>(MethodInfo methodInfo)
        {
            // create a strongly typed delegate
            Func<TSource, TResult> getter = (Func<TSource, TResult>)Delegate.CreateDelegate(typeof(Func<TSource, TResult>), methodInfo);

            // create a untyped delegate
            Func<object, object> function = (object source) => getter((TSource)source);
            return function;
        }

        public static Action<object, object> CreateSetter(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                throw new ArgumentNullException("propertyInfo");
            }

            var sourceType = propertyInfo.DeclaringType;
            var methodInfo = propertyInfo.GetSetMethod();

            if (methodInfo == null)
            {
                throw new InvalidOperationException("methodInfo is null");
            }

            if (setterOpenImpl == null)
            {
                // get the generic method
                setterOpenImpl = typeof(DelegateUtility).GetMethod("CreateSetterImpl", BindingFlags.Static | BindingFlags.Public);
            }

            Type parameterType = methodInfo.GetParameters()[0].ParameterType;

            try
            {
                // make generic method
                MethodInfo closedImpl = setterOpenImpl.MakeGenericMethod(sourceType, parameterType);

                // call method
                Action<object, object> fastCall = (Action<object, object>)closedImpl.Invoke(null, new object[] { methodInfo });
                return fastCall;
            }
            catch (Exception)
            {
                if (enableDebug)
                {
                    Debug.LogErrorFormat("Call CreateSetterImpl<{0}, {1}>() failed, property={2}.{3}",
                        sourceType, parameterType, propertyInfo.DeclaringType, propertyInfo.Name);
                }

                // method failed, use reflection
                Action<object, object> reflectionCall = (object obj, object value) => propertyInfo.SetValue(obj, value, null);
                return reflectionCall;
            }
        }

        public static Action<object, object> CreateSetterImpl<TSource, TParam>(MethodInfo methodInfo)
        {
            // create a strongly typed delegate
            Action<TSource, TParam> setter = (Action<TSource, TParam>)Delegate.CreateDelegate(typeof(Action<TSource, TParam>), methodInfo);

            // create a untyped delegate
            Action<object, object> action = (object source, object parameter) => setter((TSource)source, (TParam)parameter);
            return action;
        }

        public static Func<object, object> CreateImplicitOperator(MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                return null;
            }

            if (implicitOperatorOpenImpl == null)
            {
                // get the generic method
                implicitOperatorOpenImpl = typeof(DelegateUtility).GetMethod("CreateImplicitOperatorImpl", BindingFlags.Static | BindingFlags.Public);
            }

            Type paramterType = methodInfo.GetParameters()[0].ParameterType;

            try
            {
                // make generic method
                MethodInfo closedImpl = implicitOperatorOpenImpl.MakeGenericMethod(paramterType, methodInfo.ReturnType);

                // call method
                Func<object, object> fastCall = (Func<object, object>)closedImpl.Invoke(null, new object[] { methodInfo });
                return fastCall;
            }
            catch (Exception)
            {
                if (enableDebug)
                {
                    Debug.LogErrorFormat("Call CreateImplicitOperatorImpl<{0}, {1}>() failed, method={2}.{3}",
                        paramterType, methodInfo.ReturnType, methodInfo.DeclaringType, methodInfo.Name);
                }

                // method failed, use reflection
                Func<object, object> reflectionCall = (object source) => methodInfo.Invoke(null, new object[] { source });
                return reflectionCall;
            }
        }

        public static Func<object, object> CreateImplicitOperatorImpl<TParam, TResult>(MethodInfo methodInfo)
        {
            // create a strongly typed delegate
            Func<TParam, TResult> implicitOperator = (Func<TParam, TResult>)Delegate.CreateDelegate(typeof(Func<TParam, TResult>), methodInfo);

            // create a untyped delegate
            Func<object, object> function = (object source) => implicitOperator((TParam)source);
            return function;
        }
    }
}
