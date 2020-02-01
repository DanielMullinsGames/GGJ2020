using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// CanvasScanner is an component used in Editor. When you click the "Scan" button, it will
    /// generate the CanvasData.
    ///
    /// "layout" is the layout of the "content", which can be Horizontal or Vertical. "sectionWidth"
    /// and "sectionHeight" decide the dimension of generated section. Generally, you can set to the
    /// same size of target view port. "content" is the source to be scanned. "data" is the generated
    /// CanvasData.
    ///
    /// How it works
    ///
    /// First the scanner gets all GameObjects in "content" node, it only accepts if the GameObject
    /// is a prefab instance and has valid RectTransform. The scanner will create a Node for the
    /// valid GameObject, the Node only contains the minimal information of the GameObject, such as
    /// prefab reference, order index, 2D position, etc. Next the scanner will build the section
    /// list. The content is split into sections, and each section contains a visible node list.
    ///
    /// Note that the CanvasPrinter will use the Node to recreate the source GameObject. Because the
    /// node only contains the minimal information of the source GameObject, all values which
    /// override the default values in prefab will be lost. The values of RectTransform are partially
    /// restored. If you want to save extra data, you can create a script which implements
    /// ISerializableCanvasNode and add it to the source GameObject. The CanvasScanner will call the
    /// Serialize method and save it as metadata in Node.
    /// </summary>
    public class CanvasScanner : MonoBehaviour
    {
        public CanvasData.LayoutType layout;
        public float sectionWidth;
        public float sectionHeight;
        public bool includeInactiveRect;
        public RectTransform content;
        public CanvasData data;

        public bool generateDebugRect;
        public Color rectColor = new Color(0f, 1f, 0f, 0.5f);
        public RectTransform rectContainer;
    }
}