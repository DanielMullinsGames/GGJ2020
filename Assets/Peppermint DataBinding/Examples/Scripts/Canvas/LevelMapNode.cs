using System;
using UnityEngine;

namespace Peppermint.DataBinding.Example
{
    /// <summary>
    /// Component for level map node. You can setup node type and level index, these values will be
    /// used in data binding.
    /// </summary>
    public class LevelMapNode : MonoBehaviour, ISerializableCanvasNode
    {
        public enum NodeType
        {
            Level,
            BossLevel,
        }

        public NodeType nodeType;
        public int levelIndex;

        private static readonly char[] Seperators = new char[] { ',' };

        public string Serialize()
        {
            // serialize to custom string format "nodeType,levelIndex"
            string str = string.Format("{0},{1}", nodeType, levelIndex);
            return str;
        }

        public void Deserialize(string str)
        {
            // deserialize from string
            var tokens = str.Split(Seperators, StringSplitOptions.RemoveEmptyEntries);

            nodeType = (NodeType)Enum.Parse(typeof(NodeType), tokens[0]);
            levelIndex = Int32.Parse(tokens[1]);
        }
    }
}
