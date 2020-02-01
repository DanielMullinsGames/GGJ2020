#if PEPPERMINT_UNIT_TEST

using NUnit.Framework;
using System;
using System.Reflection;
using UnityEngine;

namespace Peppermint.DataBinding.UnitTest
{
    [TestFixture]
    public class ValueConversionTests
    {
        public class TestModel : BindableObject
        {
            private Vector2 vector2Value;
            private Vector3 vector3Value;
            private Color colorValue;
            private Color32 color32Value;
            private int? nullableIntValue;
            private Color? nullableColorValue;

            #region Bindable Properties

            public Vector2 Vector2Value
            {
                get { return vector2Value; }
                set { SetProperty(ref vector2Value, value, "Vector2Value"); }
            }

            public Vector3 Vector3Value
            {
                get { return vector3Value; }
                set { SetProperty(ref vector3Value, value, "Vector3Value"); }
            }

            public Color ColorValue
            {
                get { return colorValue; }
                set { SetProperty(ref colorValue, value, "ColorValue"); }
            }

            public Color32 Color32Value
            {
                get { return color32Value; }
                set { SetProperty(ref color32Value, value, "Color32Value"); }
            }

            public Nullable<int> NullableIntValue
            {
                get { return nullableIntValue; }
                set { SetProperty(ref nullableIntValue, value, "NullableIntValue"); }
            }

            public Nullable<Color> NullableColorValue
            {
                get { return nullableColorValue; }
                set { SetProperty(ref nullableColorValue, value, "NullableColorValue"); }
            }

            #endregion

        }

        [Test]
        public void TestImplicitConverter()
        {
            var source = new TestModel();
            var target = new TestModel();

            // Vector2 <=> Vector3
            {
                var binding = new Binding("Vector2Value", target, "Vector3Value");
                binding.Bind(source);
                binding.UpdateSource();

                binding = new Binding("Vector3Value", target, "Vector2Value");
                binding.Bind(source);
                binding.UpdateSource();
            }

            // Color <=> Color32
            {
                var binding = new Binding("ColorValue", target, "Color32Value");
                binding.Bind(source);
                binding.UpdateSource();

                binding = new Binding("Color32Value", target, "ColorValue");
                binding.Bind(source);
                binding.UpdateSource();
            }
        }

        [Test]
        public void TestNullable()
        {
            var source = new TestModel();
            var target = new TestModel();

            // Color? => Color
            {
                // set source value to null
                source.NullableColorValue = null;

                var binding = new Binding("NullableColorValue", target, "ColorValue");
                binding.Bind(source);

                // check converter
                var converter = GetValueConverter(binding);
                Assert.IsTrue(converter == ValueConverterProvider.Instance.DefaultConverter);

                // check target value
                Assert.AreEqual(new Color(), target.ColorValue);

                // force update source
                binding.UpdateSource();
                Assert.AreEqual(new Color(), source.NullableColorValue);

                // set source value
                source.NullableColorValue = Color.red;
                Assert.AreEqual(Color.red, target.ColorValue);

                // force update source
                binding.UpdateSource();
                Assert.AreEqual(Color.red, source.NullableColorValue);
            }

            // Color => Color?
            {
                // set source value
                source.ColorValue = Color.red;

                var binding = new Binding("ColorValue", target, "NullableColorValue");
                binding.Bind(source);

                // check converter
                var converter = GetValueConverter(binding);
                Assert.IsTrue(converter == null);

                // check target value
                Assert.AreEqual(Color.red, target.NullableColorValue);

                // force update source
                binding.UpdateSource();
                Assert.AreEqual(Color.red, source.ColorValue);

                // set target value
                target.NullableColorValue = null;
                binding.UpdateSource();

                Assert.AreEqual(new Color(), source.ColorValue);
            }
        }

        [Test]
        public void TestModeNone()
        {
            var source = new TestModel();
            var target = new TestModel();

            // Vector2 => Vector2
            {
                source.Vector2Value = Vector2.one;

                var binding = new Binding("Vector2Value", target, "Vector2Value", Binding.BindingMode.OneWay, Binding.ConversionMode.None, null);
                binding.Bind(source);

                Assert.AreEqual(Vector2.one, target.Vector2Value);
            }

            // Vector2 => Vector3 will fail
//             {
//                 source.Vector2Value = Vector2.one;
// 
//                 var binding = new Binding("Vector2Value", target, "Vector3Value", Binding.BindingMode.OneWay, Binding.ConversionMode.None, null);
//                 binding.Bind(source);
// 
//                 Assert.AreNotEqual((Vector3)source.Vector2Value, target.Vector3Value);
//             }
        }

        [Test]
        public void TestModeParameter()
        {
            var source = new TestModel();
            var target = new TestModel();

            // Vector2 => Vector3 with converter
            {
                source.Vector2Value = Vector2.one;

                var vc = ValueConverterProvider.Instance.GetConverter(typeof(Vector2), typeof(Vector3));
                var binding = new Binding("Vector2Value", target, "Vector3Value", Binding.BindingMode.OneWay, Binding.ConversionMode.Parameter, vc);
                binding.Bind(source);

                Assert.AreEqual((Vector3)source.Vector2Value, target.Vector3Value);
            }

            // Vector3 => Vector2 with converter
            {
                source.Vector3Value = Vector3.one;

                var vc = ValueConverterProvider.Instance.GetConverter(typeof(Vector3), typeof(Vector2));
                var binding = new Binding("Vector3Value", target, "Vector2Value", Binding.BindingMode.OneWay, Binding.ConversionMode.Parameter, vc);
                binding.Bind(source);

                Assert.AreEqual((Vector2)source.Vector3Value, target.Vector2Value);
            }
        }

        [Test]
        public void TestModeAutomatic()
        {
            var source = new TestModel();
            var target = new TestModel();

            // Vector2 => Vector2
            {
                source.Vector2Value = Vector2.one;

                var binding = new Binding("Vector2Value", target, "Vector2Value", Binding.BindingMode.OneWay, Binding.ConversionMode.Automatic, null);
                binding.Bind(source);

                // check converter
                var converter = GetValueConverter(binding);
                Assert.IsTrue(converter == null);

                Assert.AreEqual(Vector2.one, target.Vector2Value);
            }

            // Vector2 => Vector3
            {
                var vc = ValueConverterProvider.Instance.GetConverter(typeof(Vector2), typeof(Vector3));
                source.Vector2Value = Vector2.one;

                var binding = new Binding("Vector2Value", target, "Vector3Value", Binding.BindingMode.OneWay, Binding.ConversionMode.Automatic, null);
                binding.Bind(source);

                // check converter
                var converter = GetValueConverter(binding);
                Assert.IsTrue(converter == vc);

                Assert.AreEqual((Vector3)source.Vector2Value, target.Vector3Value);
            }

            // Vector3 => Vector2
            {
                var vc = ValueConverterProvider.Instance.GetConverter(typeof(Vector3), typeof(Vector2));
                source.Vector3Value = Vector3.one;

                var binding = new Binding("Vector3Value", target, "Vector2Value", Binding.BindingMode.OneWay, Binding.ConversionMode.Automatic, null);
                binding.Bind(source);

                // check converter
                var converter = GetValueConverter(binding);
                Assert.IsTrue(converter == vc);

                Assert.AreEqual((Vector2)source.Vector3Value, target.Vector2Value);
            }
        }

        private IValueConverter GetValueConverter(Binding binding)
        {
            var converterField = typeof(Binding).GetField("converter", BindingFlags.Instance | BindingFlags.NonPublic);
            return (IValueConverter)converterField.GetValue(binding);
        }

    }
}

#endif