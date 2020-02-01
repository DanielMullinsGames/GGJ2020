#if PEPPERMINT_UNIT_TEST

using NUnit.Framework;

namespace Peppermint.DataBinding.UnitTest
{
    [TestFixture]
    public class NestedPropertyTests
    {
        public class A
        {
            public B NestedB { get; set; }

            public A()
            {
                NestedB = new B();
            }
        }

        public class B
        {
            public C NestedC { get; set; }

            public B()
            {
                NestedC = new C();
            }
        }

        public class C : BindableObject
        {
            private int intValue;

            public int IntValue
            {
                get { return intValue; }
                set { SetProperty(ref intValue, value, "IntValue"); }
            }
        }


        [Test]
        public void TestNestedSourceOneWay()
        {
            var source = new A();
            var target = new C();

            var binding = new Binding("NestedB.NestedC.IntValue", target, "IntValue");
            binding.Bind(source);

            source.NestedB.NestedC.IntValue = 5;
            Assert.AreEqual(5, target.IntValue);
        }

        [Test]
        public void TestNestedTargetOneWay()
        {
            var source = new C();
            var target = new A();

            var binding = new Binding("IntValue", target, "NestedB.NestedC.IntValue");
            binding.Bind(source);

            source.IntValue = 5;
            Assert.AreEqual(5, target.NestedB.NestedC.IntValue);
        }

        [Test]
        public void TestNestedSourceOneWayToSource()
        {
            var source = new A();
            var target = new C();

            var binding = new Binding("NestedB.NestedC.IntValue", target, "IntValue", Binding.BindingMode.OneWayToSource, Binding.ConversionMode.Automatic, null);
            binding.Bind(source);

            // update target value
            target.IntValue = 5;
            Assert.AreEqual(5, source.NestedB.NestedC.IntValue);
        }

        [Test]
        public void TestNestedTargetOneWayToSource()
        {
            var source = new C();
            var target = new A();

            var binding = new Binding("IntValue", target, "NestedB.NestedC.IntValue", Binding.BindingMode.OneWayToSource, Binding.ConversionMode.Automatic, null);
            binding.Bind(source);

            // update target value
            target.NestedB.NestedC.IntValue = 5;
            Assert.AreEqual(5, source.IntValue);
        }

        [Test]
        public void TestNestedSourceTwoWay()
        {
            var source = new A();
            var target = new C();

            var binding = new Binding("NestedB.NestedC.IntValue", target, "IntValue", Binding.BindingMode.TwoWay, Binding.ConversionMode.Automatic, null);
            binding.Bind(source);

            // update source
            source.NestedB.NestedC.IntValue = 5;
            Assert.AreEqual(5, target.IntValue);

            // update target
            target.IntValue = 6;
            Assert.AreEqual(6, source.NestedB.NestedC.IntValue);
        }

        [Test]
        public void TestNestedTargetTwoWay()
        {
            var source = new C();
            var target = new A();

            var binding = new Binding("IntValue", target, "NestedB.NestedC.IntValue", Binding.BindingMode.TwoWay, Binding.ConversionMode.Automatic, null);
            binding.Bind(source);

            // update source
            source.IntValue = 5;
            Assert.AreEqual(5, target.NestedB.NestedC.IntValue);

            // update target
            target.NestedB.NestedC.IntValue = 6;
            Assert.AreEqual(6, source.IntValue);
        }

        [Test]
        public void TestNestedSourceAndNestedTargetTwoWay()
        {
            var source = new A();
            var target = new A();

            var binding = new Binding("NestedB.NestedC.IntValue", target, "NestedB.NestedC.IntValue", Binding.BindingMode.TwoWay, Binding.ConversionMode.Automatic, null);
            binding.Bind(source);

            // update source
            source.NestedB.NestedC.IntValue = 5;
            Assert.AreEqual(5, target.NestedB.NestedC.IntValue);

            // update target
            target.NestedB.NestedC.IntValue = 6;
            Assert.AreEqual(6, source.NestedB.NestedC.IntValue);
        }
    }
}

#endif