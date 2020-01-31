using System;
using System.Collections.Generic;
using UnityEngine;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// CanvasData is the blueprint for recreating the view.
    /// </summary>
    [CreateAssetMenu(fileName = "NewCanvasData", menuName = "Peppermint/Canvas Data")]
    public class CanvasData : ScriptableObject
    {
        public enum LayoutType
        {
            Horizontal,
            Vertical,
        }

        /// <summary>
        /// Node only contains the essential information, such as the prefab reference, 2D position
        /// and rotation, etc. If the view contains component derived from ISerializableCanvasNode, it
        /// will also serialize it to metadata.
        /// </summary>
        [Serializable]
        public class Node
        {
            public int index;
            public GameObject prefab;
            public Vector2 anchoredPosition;
            public Vector2 sizeDelta;
            public float rotation;
            public string metadata;

            public override string ToString()
            {
                return string.Format("Node_{0}", index);
            }
        }

        /// <summary>
        /// The canvas is split into sections, and each section contains an index list for visible
        /// nodes. The section is a simplified PVS (Potential Visibility Set).
        /// </summary>
        [Serializable]
        public class Section
        {
            public int index;
            public List<int> nodeIndexList;
            internal List<Node> nodeList;

            public Section()
            {
                nodeIndexList = new List<int>();
                nodeList = new List<Node>();
            }

            public override string ToString()
            {
                return string.Format("Section_{0}", index);
            }
        }

        public float width;
        public float height;
        public LayoutType layout;

        public float sectionWidth;
        public float sectionHeight;

        public List<Node> nodeList;
        public List<Section> sectionList;

        void OnEnable()
        {
            if (nodeList == null || sectionList == null)
            {
                return;
            }

            // create node list from index list
            foreach (var section in sectionList)
            {
                section.nodeList.Clear();

                // create node list
                foreach (var index in section.nodeIndexList)
                {
                    var node = nodeList[index];
                    section.nodeList.Add(node);
                }
            }
        }
    }
}
