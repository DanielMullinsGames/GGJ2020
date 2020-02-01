#if PEPPERMINT_UNIT_TEST

using NUnit.Framework;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Peppermint.DataBinding.UnitTest
{
    [TestFixture]
    public class DelegateUtilityTests
    {
        public class Model
        {
            public int IntValue { get; set; }
            public string StringValue { get; set; }
        }

        [Test]
        public void TestGetterAndSetter()
        {
            var model = new Model();

            {
                var property = typeof(Model).GetProperty("IntValue");
                var getter = DelegateUtility.CreateGetter(property);
                var setter = DelegateUtility.CreateSetter(property);

                // call setter
                setter(model, 11);
                Assert.AreEqual(11, model.IntValue);

                // call getter
                int value = (int)getter(model);
                Assert.AreEqual(11, value);
            }

            {
                var property = typeof(Model).GetProperty("StringValue");
                var getter = DelegateUtility.CreateGetter(property);
                var setter = DelegateUtility.CreateSetter(property);

                // call setter
                setter(model, "Test");
                Assert.AreEqual("Test", model.StringValue);

                // call getter
                string value = (string)getter(model);
                Assert.AreEqual("Test", value);
            }
        }

        [Test]
        public void TestImplicitOperator()
        {
            {
                var method = GetImplicitConverter(typeof(Vector2), typeof(Vector2), typeof(Vector3));
                var function = DelegateUtility.CreateImplicitOperator(method);
                Assert.IsNotNull(function);

                Vector3 v3 = (Vector3)function.Invoke(Vector2.one);
                Assert.AreEqual(new Vector3(1f, 1f, 0f), v3);
            }

            {
                var method = GetImplicitConverter(typeof(Vector2), typeof(Vector3), typeof(Vector2));
                var function = DelegateUtility.CreateImplicitOperator(method);
                Assert.IsNotNull(function);

                Vector2 v2 = (Vector2)function.Invoke(Vector3.one);
                Assert.AreEqual(new Vector2(1f, 1f), v2);
            }
        }

        [Test]
        public void TestImplicitOperatorWithNullValue()
        {
            {
                var method = GetImplicitConverter(typeof(Vector2), typeof(Vector2), typeof(Vector3));
                var function = DelegateUtility.CreateImplicitOperator(method);
                Assert.IsNotNull(function);

                // will return default value
                Vector3 value = (Vector3)method.Invoke(null, new object[] { null });
                Assert.AreEqual(Vector3.zero, value);

                try
                {
                    // call delegate will failed
                    function.Invoke(null);
                }
                catch (Exception ex)
                {
                    Assert.IsTrue(ex is NullReferenceException);
                }
            }

            {
                var method = GetImplicitConverter(typeof(Vector2), typeof(Vector3), typeof(Vector2));
                var function = DelegateUtility.CreateImplicitOperator(method);
                Assert.IsNotNull(function);

                // will return default value
                Vector2 value = (Vector2)method.Invoke(null, new object[] { null });
                Assert.AreEqual(Vector2.zero, value);

                try
                {
                    // call delegate will failed
                    function.Invoke(null);
                }
                catch (Exception ex)
                {
                    Assert.IsTrue(ex is NullReferenceException);
                }
            }
        }

        private MethodInfo GetImplicitConverter(Type sourceType, Type from, Type to)
        {
            var result = sourceType.GetMethods(BindingFlags.Public | BindingFlags.Static).Where(method =>
            {
                if (method.Name != "op_Implicit")
                {
                    return false;
                }

                var parameters = method.GetParameters();
                if (parameters.Length != 1)
                {
                    // with 1 parameter
                    return false;
                }

                var parameter = parameters[0];
                if (parameter.ParameterType != from)
                {
                    return false;
                }

                if (method.ReturnType != to)
                {
                    return false;
                }

                return true;
            }).FirstOrDefault();

            return result;
        }

    }
}

#endif