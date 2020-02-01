using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace Peppermint.DataBinding.Example
{
    /// <summary>
    /// HtmlColorConverter handles HTML color string and Color conversion. It implements the
    /// IValueConverter interface, and uses ColorUtility to do the conversion.
    ///
    /// Note that this class associates an InitializeValueConverter attribute, this attribute allows
    /// this class to be initialized when the ValueConverterProvider is created. You can create and
    /// add the converter as a general converter in its static constructor.
    /// </summary>
    [InitializeValueConverter]
    public class HtmlColorConverter : IValueConverter
    {
        private readonly Color defaultColor;
        private readonly string defaultHexString;

        static HtmlColorConverter()
        {
            // create converter instance
            var converter = new HtmlColorConverter();

            // add as a string to color converter
            ValueConverterProvider.Instance.AddConverter(typeof(string), typeof(Color), converter);
        }

        private HtmlColorConverter()
        {
            defaultColor = Color.black;
            defaultHexString = "#" + ColorUtility.ToHtmlStringRGBA(defaultColor);
        }

        public object Convert(object value, Type targetType)
        {
            Assert.IsTrue(targetType == typeof(Color));

            // get hex string
            var hexString = value as string;
            if (hexString == null)
            {
                return defaultColor;
            }

            Color color;
            if (ColorUtility.TryParseHtmlString(hexString, out color))
            {
                // parse success
                return color;
            }
            else
            {
                // parse failed
                return defaultColor;
            }
        }

        public object ConvertBack(object value, Type targetType)
        {
            Assert.IsTrue(targetType == typeof(string));

            // check value
            if (value == null || !(value is Color))
            {
                return defaultHexString;
            }

            var color = (Color)value;

            // append #
            var hexString = "#" + ColorUtility.ToHtmlStringRGBA(color);
            return hexString;
        }
    }
}
