using System;
using System.Collections;
using UnityEngine;

namespace Peppermint.DataBinding.Example
{
    /// <summary>
    /// This example demonstrates how to use TextBinder.
    ///
    /// TextBinder can bind UI.Text to any System.Object. If the format string is specified, the
    /// given format string is used, otherwise it calls ToString() method to get the string value.
    /// </summary>
    public class TextBinderExample : BindableMonoBehaviour
    {
        public float updateInterval = 1f;

        private string stringValue;
        private float floatValue;
        private double doubleValue;
        private bool boolValue;
        private int intValue;
        private Vector2 vector2Value;
        private SystemLanguage enumValue;
        private DateTime dateTimeValue;
        private int? nullableIntValue;
        private int counter;
        private int maxCount;

        #region Bindable Properties

        public string StringValue
        {
            get { return stringValue; }
            set { SetProperty(ref stringValue, value, "StringValue"); }
        }

        public float FloatValue
        {
            get { return floatValue; }
            set { SetProperty(ref floatValue, value, "FloatValue"); }
        }

        public double DoubleValue
        {
            get { return doubleValue; }
            set { SetProperty(ref doubleValue, value, "DoubleValue"); }
        }

        public bool BoolValue
        {
            get { return boolValue; }
            set { SetProperty(ref boolValue, value, "BoolValue"); }
        }

        public int IntValue
        {
            get { return intValue; }
            set { SetProperty(ref intValue, value, "IntValue"); }
        }

        public Vector2 Vector2Value
        {
            get { return vector2Value; }
            set { SetProperty(ref vector2Value, value, "Vector2Value"); }
        }

        public SystemLanguage EnumValue
        {
            get { return enumValue; }
            set { SetProperty(ref enumValue, value, "EnumValue"); }
        }

        public DateTime DateTimeValue
        {
            get { return dateTimeValue; }
            set { SetProperty(ref dateTimeValue, value, "DateTimeValue"); }
        }

        public Nullable<int> NullableIntValue
        {
            get { return nullableIntValue; }
            set { SetProperty(ref nullableIntValue, value, "NullableIntValue"); }
        }

        public int Counter
        {
            get { return counter; }
            set { SetProperty(ref counter, value, "Counter"); }
        }

        public int MaxCount
        {
            get { return maxCount; }
            set { SetProperty(ref maxCount, value, "MaxCount"); }
        }

        #endregion

        void Start()
        {
            InitValue();

            // start coroutine to update date time
            StartCoroutine(UpdateAsync());

            BindingManager.Instance.AddSource(this, typeof(TextBinderExample).Name);
        }

        void OnDestroy()
        {
            BindingManager.Instance.RemoveSource(this);
        }

        private void InitValue()
        {
            StringValue = "TextBinder Test";
            FloatValue = 3.1415926f;
            DoubleValue = 10761.937554;
            BoolValue = true;
            IntValue = 8395;
            Vector2Value = new Vector2(0.5f, 0.5f);
            DateTimeValue = DateTime.Now;
            EnumValue = SystemLanguage.English;
            Counter = 1;
            MaxCount = 2;
        }

        private void UpdateValue()
        {
            StringValue = ShiftString(StringValue);
            EnumValue = ShiftEnum(EnumValue);

            FloatValue += 0.01f;
            DoubleValue += 0.9f;
            BoolValue = !BoolValue;
            IntValue += 10;
            Vector2Value = UnityEngine.Random.insideUnitCircle;
            DateTimeValue = DateTime.Now;
            Counter++;
            if (Counter > MaxCount)
            {
                Counter = 1;
                MaxCount++;
            }

            if (BoolValue)
            {
                NullableIntValue = IntValue;
            }
            else
            {
                NullableIntValue = null;
            }
        }

        private string ShiftString(string s)
        {
            return string.Format("{0}{1}", s.Substring(1, s.Length - 1), s.Substring(0, 1));
        }

        private SystemLanguage ShiftEnum(SystemLanguage e)
        {
            int value = (int)e;
            value = (value + 1) % (int)SystemLanguage.Unknown;
            return (SystemLanguage)value;
        }

        private IEnumerator UpdateAsync()
        {
            while (true)
            {
                yield return new WaitForSeconds(updateInterval);

                UpdateValue();
            }
        }
    }
}
