#if PEPPERMINT_UNIT_TEST

using NUnit.Framework;
using UnityEngine;

namespace Peppermint.DataBinding.UnitTest
{
    [TestFixture]
    public class PropertyAccessorTests
    {
        public class Model
        {
            public int IntValue { get; set; }
            public float FloatValue { get; set; }
            public Vector3 Vector3Value { get; set; }
            public string StringValue { get; set; }
            public Binding.BindingMode EnumValue { get; set; }
            public Color? NullableColorValue { get; set; }
        }

        [Test]
        public void TestCache()
        {
            var accessorA = TypeCache.Instance.GetPropertyAccessor(typeof(Model), "IntValue");
            Assert.IsNotNull(accessorA);

            var accessorB = TypeCache.Instance.GetPropertyAccessor(typeof(Model), "FloatValue");
            Assert.IsNotNull(accessorB);

            var accessorC = TypeCache.Instance.GetPropertyAccessor(typeof(Model), "IntValue");
            Assert.IsNotNull(accessorC);

            Assert.IsTrue(accessorA == accessorC);
        }

        [Test]
        public void TestSetNull()
        {
            var model = new Model()
            {
                IntValue = -1,
                FloatValue = 2.0f,
                StringValue = "Test",
                Vector3Value = Vector3.one,
                EnumValue = Binding.BindingMode.TwoWay,
                NullableColorValue = Color.red,
            };

            var accessor = TypeCache.Instance.GetPropertyAccessor(typeof(Model), "IntValue");
            accessor.SetValue(model, null);
            Assert.AreEqual(0, model.IntValue);

            accessor = TypeCache.Instance.GetPropertyAccessor(typeof(Model), "FloatValue");
            accessor.SetValue(model, null);
            Assert.AreEqual(0.0f, model.FloatValue);

            accessor = TypeCache.Instance.GetPropertyAccessor(typeof(Model), "Vector3Value");
            accessor.SetValue(model, null);
            Assert.AreEqual(new Vector3(), model.Vector3Value);

            accessor = TypeCache.Instance.GetPropertyAccessor(typeof(Model), "StringValue");
            accessor.SetValue(model, null);
            Assert.AreEqual(null, model.StringValue);

            accessor = TypeCache.Instance.GetPropertyAccessor(typeof(Model), "EnumValue");
            accessor.SetValue(model, null);
            Assert.AreEqual(Binding.BindingMode.OneWay, model.EnumValue);

            accessor = TypeCache.Instance.GetPropertyAccessor(typeof(Model), "NullableColorValue");
            accessor.SetValue(model, null);
            Assert.AreEqual(null, model.NullableColorValue);
        }
    }
}

#endif