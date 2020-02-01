#if PEPPERMINT_UNIT_TEST

using NUnit.Framework;

namespace Peppermint.DataBinding.UnitTest
{
    [TestFixture]
    public class BindingModeTests
    {
        public class BindableModel : BindableObject
        {
            private string stringValue;
            private int intValue;

            public string StringValue
            {
                get { return stringValue; }
                set { SetProperty(ref stringValue, value, "StringValue"); }
            }

            public int IntValue
            {
                get { return intValue; }
                set { SetProperty(ref intValue, value, "IntValue"); }
            }
        }

        public class Model
        {
            public string StringValue { get; set; }
            public int IntValue { get; set; }
        }

        [Test]
        public void TestOneWay()
        {
            var source = new BindableModel();
            var target = new Model();

            var bindingA = new Binding("StringValue", target, "StringValue", Binding.BindingMode.OneWay);
            var bindingB = new Binding("IntValue", target, "IntValue", Binding.BindingMode.OneWay);

            // update source
            source.IntValue = 1;
            source.StringValue = "A";

            // bind
            bindingA.Bind(source);
            bindingB.Bind(source);

            Assert.IsTrue(target.IntValue == 1);
            Assert.IsTrue(target.StringValue == "A");

            // update source
            source.IntValue = 2;
            source.StringValue = "B";

            Assert.IsTrue(target.IntValue == 2);
            Assert.IsTrue(target.StringValue == "B");

            // unbind
            bindingA.Unbind();
            bindingB.Unbind();

            // update source
            source.IntValue = 3;
            source.StringValue = "C";

            Assert.IsTrue(target.IntValue == 2);
            Assert.IsTrue(target.StringValue == "B");
        }

        [Test]
        public void TestTwoWay()
        {
            var source = new BindableModel();
            var target = new BindableModel();

            var bindingA = new Binding("StringValue", target, "StringValue", Binding.BindingMode.TwoWay);
            var bindingB = new Binding("IntValue", target, "IntValue", Binding.BindingMode.TwoWay);

            // update source
            source.IntValue = 1;
            source.StringValue = "A";

            // bind
            bindingA.Bind(source);
            bindingB.Bind(source);

            Assert.IsTrue(target.IntValue == 1);
            Assert.IsTrue(target.StringValue == "A");

            // update source
            source.IntValue = 2;
            source.StringValue = "B";

            Assert.IsTrue(target.IntValue == 2);
            Assert.IsTrue(target.StringValue == "B");

            // update target
            target.IntValue = 3;
            target.StringValue = "C";

            Assert.IsTrue(source.IntValue == 3);
            Assert.IsTrue(source.StringValue == "C");

            // unbind
            bindingA.Unbind();
            bindingB.Unbind();

            Assert.IsTrue(source.IntValue == 3);
            Assert.IsTrue(source.StringValue == "C");
            Assert.IsTrue(target.IntValue == 3);
            Assert.IsTrue(target.StringValue == "C");
        }

        [Test]
        public void TestOneWayToSource()
        {
            var source = new Model();
            var target = new BindableModel();

            var bindingA = new Binding("StringValue", target, "StringValue", Binding.BindingMode.OneWayToSource);
            var bindingB = new Binding("IntValue", target, "IntValue", Binding.BindingMode.OneWayToSource);

            // update target
            target.IntValue = 1;
            target.StringValue = "A";

            // bind
            bindingA.Bind(source);
            bindingB.Bind(source);

            Assert.IsTrue(source.IntValue == 1);
            Assert.IsTrue(source.StringValue == "A");

            // update target
            target.IntValue = 2;
            target.StringValue = "B";

            Assert.IsTrue(source.IntValue == 2);
            Assert.IsTrue(source.StringValue == "B");

            // unbind
            bindingA.Unbind();
            bindingB.Unbind();

            Assert.IsTrue(source.IntValue == 2);
            Assert.IsTrue(source.StringValue == "B");

            // update target
            target.IntValue = 1;
            target.StringValue = "A";

            Assert.IsTrue(source.IntValue == 2);
            Assert.IsTrue(source.StringValue == "B");
        }

        [Test]
        public void TestOneWayOneTime()
        {
            var source = new Model();
            var target = new Model();

            var bindingA = new Binding("StringValue", target, "StringValue", Binding.BindingMode.OneWay);
            var bindingB = new Binding("IntValue", target, "IntValue", Binding.BindingMode.OneWay);

            // update source
            source.IntValue = 1;
            source.StringValue = "A";

            // bind
            bindingA.Bind(source);
            bindingB.Bind(source);

            Assert.IsTrue(target.IntValue == 1);
            Assert.IsTrue(target.StringValue == "A");

            // update source
            source.IntValue = 2;
            source.StringValue = "B";

            Assert.IsTrue(target.IntValue == 1);
            Assert.IsTrue(target.StringValue == "A");

            // unbind
            bindingA.Unbind();
            bindingB.Unbind();

            Assert.IsTrue(target.IntValue == 1);
            Assert.IsTrue(target.StringValue == "A");
        }

        [Test]
        public void TestTwoWayOneTime()
        {
            var source = new Model();
            var target = new Model();

            var bindingA = new Binding("StringValue", target, "StringValue", Binding.BindingMode.TwoWay);
            var bindingB = new Binding("IntValue", target, "IntValue", Binding.BindingMode.TwoWay);

            // update source
            source.IntValue = 1;
            source.StringValue = "A";

            // bind
            bindingA.Bind(source);
            bindingB.Bind(source);

            Assert.IsTrue(target.IntValue == 1);
            Assert.IsTrue(target.StringValue == "A");

            // update source
            source.IntValue = 2;
            source.StringValue = "B";

            Assert.IsTrue(target.IntValue == 1);
            Assert.IsTrue(target.StringValue == "A");

            // update target
            target.IntValue = 3;
            target.StringValue = "C";

            Assert.IsTrue(source.IntValue == 2);
            Assert.IsTrue(source.StringValue == "B");

            // unbind
            bindingA.Unbind();
            bindingB.Unbind();

            Assert.IsTrue(source.IntValue == 2);
            Assert.IsTrue(source.StringValue == "B");
            Assert.IsTrue(target.IntValue == 3);
            Assert.IsTrue(target.StringValue == "C");
        }

        [Test]
        public void TestOneWayToSourceOneTime()
        {
            var source = new Model();
            var target = new Model();

            var bindingA = new Binding("StringValue", target, "StringValue", Binding.BindingMode.OneWayToSource);
            var bindingB = new Binding("IntValue", target, "IntValue", Binding.BindingMode.OneWayToSource);

            // update target
            target.IntValue = 1;
            target.StringValue = "A";

            // bind
            bindingA.Bind(source);
            bindingB.Bind(source);

            Assert.IsTrue(source.IntValue == 1);
            Assert.IsTrue(source.StringValue == "A");

            // update target
            target.IntValue = 2;
            target.StringValue = "B";

            Assert.IsTrue(source.IntValue == 1);
            Assert.IsTrue(source.StringValue == "A");

            // unbind
            bindingA.Unbind();
            bindingB.Unbind();

            Assert.IsTrue(source.IntValue == 1);
            Assert.IsTrue(source.StringValue == "A");
        }

    }
}

#endif