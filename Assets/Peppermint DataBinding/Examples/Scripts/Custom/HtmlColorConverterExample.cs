using System.Collections;
using UnityEngine;

namespace Peppermint.DataBinding.Example
{
    /// <summary>
    /// This example demonstrates how to use custom value converter.
    ///
    /// The view model has two color properties, but the type of the property is string, which is not
    /// supported by the ColorBinder. The ColorBinder binds to color, so we need create a custom
    /// converter to convert the string to color.
    ///
    /// There are two types of converters: general converter and named converter. The general
    /// converters are used in all binding objects with automatic conversion mode. If we add a new
    /// general converter which convert the string to color, the ColorBinder will work without any
    /// modification.
    ///
    /// Notice that the general converter can be used if there is no other general converter which
    /// already added with string to color conversion. If we need two different string to color
    /// converters, we can use named converter. Add both converters with different names, and use the
    /// CustomBinder to bind the color property with specified "Converter Name".
    ///
    /// In this demo, the hexColorA is bound to ColorBinder, it use the general converter to convert
    /// string to color. The hexColorB is bound to a CustomBinder, it use the named converter to
    /// convert string to color.
    ///
    /// For demonstration, we add the same converter as a named converter, so it can be used in the
    /// CustomBinder.
    /// </summary>
    public class HtmlColorConverterExample : BindableMonoBehaviour
    {
        public float updateInterval = 1f;

        private string hexColorA;
        private string hexColorB;

        #region Bindable Properties

        public string HexColorA
        {
            get { return hexColorA; }
            set { SetProperty(ref hexColorA, value, "HexColorA"); }
        }

        public string HexColorB
        {
            get { return hexColorB; }
            set { SetProperty(ref hexColorB, value, "HexColorB"); }
        }

        #endregion

        void Awake()
        {
            // get the HtmlColorConverter instance
            var converter = ValueConverterProvider.Instance.GetConverter(typeof(string), typeof(Color));

            // add it as named converter
            ValueConverterProvider.Instance.AddNamedConverter(typeof(HtmlColorConverter).Name, converter);
        }

        void Start()
        {
            UpdateValue();

            // start coroutine to update color
            StartCoroutine(UpdateAsync());

            BindingManager.Instance.AddSource(this, typeof(HtmlColorConverterExample).Name);
        }

        void OnDestroy()
        {
            BindingManager.Instance.RemoveSource(this);
        }

        private void UpdateValue()
        {
            // generate random hex color
            var newColor = GetRandomHexColor();
            HexColorA = newColor;
            HexColorB = newColor;
        }

        private string GetRandomHexColor()
        {
            int r = UnityEngine.Random.Range(0x00, 0xFF);
            int g = UnityEngine.Random.Range(0x00, 0xFF);
            int b = UnityEngine.Random.Range(0x00, 0xFF);

            var s = "#" + r.ToString("X2") + g.ToString("X2") + b.ToString("X2") + "FF";
            return s;
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
