using System;
using UnityEngine;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// SpriteNameConverter is a value converter, which convert string to sprite.
    ///
    /// When the Convert method is called, it search the sprite name in the specified SpriteSet. If
    /// the given sprite is found, it just return the sprite, otherwise it will return some dummy
    /// sprite.
    ///
    /// If the value is null, a null sprite is return. If the string value is null or empty, a empty
    /// sprite will return. If the given sprite is not found, a missing sprite will return. You can
    /// specify a pink sprite to indicate this kind of error.
    /// </summary>
    public class SpriteNameConverter : MonoBehaviour, IValueConverter
    {
        public string converterName = "SpriteNameConverter";

        public SpriteSet spriteSet;
        public Sprite missingSprite;
        public Sprite nullSprite;
        public Sprite emptySprite;

        void Awake()
        {
            ValueConverterProvider.Instance.AddNamedConverter(converterName, this);
        }

        void OnDestroy()
        {
            ValueConverterProvider.Instance.RemoveNamedConverter(converterName, this);
        }

        private Sprite GetSprite(string spriteName)
        {
            return spriteSet.GetSprite(spriteName);
        }

        #region IValueConverter Members

        public object Convert(object value, Type targetType)
        {
            if (value == null)
            {
                return nullSprite;
            }

            string spriteName = value.ToString();
            if (string.IsNullOrEmpty(spriteName))
            {
                return emptySprite;
            }

            Sprite sprite = GetSprite(spriteName);
            if (sprite == null)
            {
                // use missing sprite
                return missingSprite;
            }

            return sprite;
        }

        public object ConvertBack(object value, Type targetType)
        {
            if (value == null)
            {
                return null;
            }
            else
            {
                Sprite sprite = (Sprite)value;
                return sprite.name;
            }
        }

        #endregion
    }
}