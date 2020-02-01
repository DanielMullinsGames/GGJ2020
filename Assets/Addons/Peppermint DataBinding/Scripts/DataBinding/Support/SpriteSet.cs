using System.Collections.Generic;
using UnityEngine;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// A collection of sprites.
    /// </summary>
    [CreateAssetMenu(fileName = "NewSpriteSet", menuName = "Peppermint/Sprite Set")]
    public class SpriteSet : ScriptableObject
    {
        public List<Sprite> spriteList;

        private Dictionary<string, Sprite> spriteDictionary;

        public Dictionary<string, Sprite> SpriteDictionary
        {
            get { return spriteDictionary; }
        }

        void OnEnable()
        {
            if (spriteList == null)
            {
                return;
            }

            // create sprite map
            spriteDictionary = new Dictionary<string, Sprite>();
            foreach (var item in spriteList)
            {
                if (spriteDictionary.ContainsKey(item.name))
                {
                    Debug.LogErrorFormat("Sprite name {0} already exist.", item.name);
                }
                else
                {
                    spriteDictionary.Add(item.name, item);
                }
            }
        }

        public Sprite GetSprite(string spriteName)
        {
            Sprite sprite;
            spriteDictionary.TryGetValue(spriteName, out sprite);
            return sprite;
        }
    }
}

