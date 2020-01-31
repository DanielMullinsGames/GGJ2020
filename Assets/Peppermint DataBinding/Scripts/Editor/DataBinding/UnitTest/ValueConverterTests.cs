#if PEPPERMINT_UNIT_TEST

using NUnit.Framework;
using System;
using System.Reflection;
using UnityEngine;

namespace Peppermint.DataBinding.UnitTest
{
    [TestFixture]
    public class ValueConverterTests
    {
        public class SystemTypeContainer
        {
            public Boolean booleanValue;
            public SByte sbyteValue;
            public Byte byteValue;
            public Int16 int16Value;
            public Int32 int32Value;
            public Int64 int64Value;
            public UInt16 uint16Value;
            public UInt32 uint32Value;
            public UInt64 uint64Value;
            public Single singleValue;
            public Double doubleValue;
            public Decimal decimalValue;
            public String stringValue;
            public Binding.BindingMode enumValue;
        }

        public class UnityTypeContainer
        {
            public Vector2 vector2Value;
            public Vector3 vector3Value;
            public Vector4 vector4Value;
            public Quaternion quaternionValue;
            public Color colorValue;
            public Color32 color32Value;
            public Rect rectValue;
        }

        [Test]
        public void TestEnum()
        {
            var defaultConverter = ValueConverterProvider.Instance.DefaultConverter;

            Assert.AreEqual(0, defaultConverter.Convert(Binding.BindingMode.OneWay, typeof(int)));
            Assert.AreEqual(Binding.BindingMode.OneWay, defaultConverter.ConvertBack(0, typeof(Binding.BindingMode)));

            Assert.AreEqual("OneWay", defaultConverter.Convert(Binding.BindingMode.OneWay, typeof(string)));
            Assert.AreEqual(Binding.BindingMode.OneWay, defaultConverter.ConvertBack("OneWay", typeof(Binding.BindingMode)));

            Assert.AreEqual(1.0f, defaultConverter.Convert(Binding.BindingMode.TwoWay, typeof(float)));
            Assert.AreEqual(Binding.BindingMode.TwoWay, defaultConverter.ConvertBack(1.0f, typeof(Binding.BindingMode)));

            Assert.AreEqual(2m, defaultConverter.Convert(Binding.BindingMode.OneWayToSource, typeof(Decimal)));
            Assert.AreEqual(Binding.BindingMode.OneWayToSource, defaultConverter.ConvertBack(2m, typeof(Binding.BindingMode)));
        }

        [Test]
        public void TestSupportedType()
        {
            var defaultConverter = ValueConverterProvider.Instance.DefaultConverter;

            // create container instance
            var container = new SystemTypeContainer();

            // get all fields
            var fields = typeof(SystemTypeContainer).GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var targetField in fields)
            {
                foreach (var sourceField in fields)
                {
                    var sourceValue = sourceField.GetValue(container);

                    // convert to target
                    var targetValue = defaultConverter.Convert(sourceValue, targetField.FieldType);

                    // convert back from target
                    var newSourceValue = defaultConverter.ConvertBack(targetValue, sourceField.FieldType);

                    // test value
                    Assert.AreEqual(sourceValue, newSourceValue);
                }
            }
        }

        [Test]
        public void TestUnityTypeToString()
        {
            var defaultConverter = ValueConverterProvider.Instance.DefaultConverter;

            // create container instance
            var container = new UnityTypeContainer();

            // get all fields
            var fields = typeof(UnityTypeContainer).GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var sourceField in fields)
            {
                var sourceValue = sourceField.GetValue(container);

                // convert to string
                var targetValue = defaultConverter.Convert(sourceValue, typeof(string));
                Assert.IsNotNull(targetValue);
            }
        }

        [Test]
        public void TestHtmlColor()
        {
            var converter = ValueConverterProvider.Instance.GetConverter(typeof(string), typeof(Color));
            if (converter == null)
            {
                // converter not exist
                return;
            }

            {
                var hexColor = "#FF0000FF";
                var color = (Color)converter.Convert(hexColor, typeof(Color));
                Assert.AreEqual(Color.red, color);

                var newHexColor = (string)converter.ConvertBack(color, typeof(string));
                Assert.AreEqual(hexColor, newHexColor);
            }

            {
                var hexColor = "red";
                var color = (Color)converter.Convert(hexColor, typeof(Color));
                Assert.AreEqual(Color.red, color);

                var newHexColor = (string)converter.ConvertBack(color, typeof(string));
                Assert.AreEqual("#FF0000FF", newHexColor);
            }
        }

        [Test]
        public void TestImplicitOperator()
        {
            // Color <=> Color32
            {
                var converter = ValueConverterProvider.Instance.MatchConverter(typeof(Color), typeof(Color32));

                var color = Color.yellow;
                var color32 = (Color32)converter.Convert(color, typeof(Color32));
                Assert.AreEqual(color, (Color)color32);

                var newColor = (Color)converter.ConvertBack(color32, typeof(Color));
                Assert.AreEqual(color, newColor);
            }

            // vector3 <=> vector2
            {
                var converter = ValueConverterProvider.Instance.MatchConverter(typeof(Vector3), typeof(Vector2));

                var vector3 = Vector3.one;
                var vector2 = (Vector2)converter.Convert(vector3, typeof(Vector2));
                Assert.AreEqual(Vector2.one, vector2);

                var newVector3 = (Vector3)converter.ConvertBack(vector2, typeof(Vector3));
                Assert.AreEqual(new Vector3(1f, 1f, 0f), newVector3);
            }

            // null value
            {
                var converter = ValueConverterProvider.Instance.MatchConverter(typeof(Vector3), typeof(Vector2));

                var vector2 = (Vector2)converter.Convert(null, typeof(Vector2));
                Assert.AreEqual(Vector2.zero, vector2);

                var vector3 = (Vector3)converter.ConvertBack(null, typeof(Vector3));
                Assert.AreEqual(Vector3.zero, vector3);
            }
        }

        [Test]
        public void TestNullable()
        {
            var defaultConverter = ValueConverterProvider.Instance.DefaultConverter;

            {
                Color? a = null;
                var b = defaultConverter.Convert(a, typeof(Color));
                Assert.IsNull(b);
            }

            {
                Color? a = Color.red;
                var b = defaultConverter.Convert(a, typeof(Color));
                Assert.AreEqual(a, b);
            }
        }
    }
}

#endif