using System.Collections.Generic;
using UnityEngine;

namespace Peppermint.DataBinding.Example
{
    /// <summary>
    /// This example demonstrates how to do the dynamic binding manually with CanvasPrinter.
    ///
    /// First, we need to create a "level map". It is a 2D UI that contains interactable items (level
    /// widget that displays level information and support clicking) and other non-interactable items
    /// (background/foreground images, particles, etc.).
    ///
    /// If the game contains many levels, the map will be very large and contains tons of objects.
    /// Load or create the entire map at runtime is impractical, because it consumes lots of memory
    /// and CPU time. So we need two helper components: CanvasScaner and CanvasPrinter. The
    /// CanvasScanner works like a scanner, it "scans" the map and generates a simplified blueprint
    /// called CanvasData. The CanvasPrinter works like a printer, it loads generated CanvasData and
    /// "prints" the visible portion of the entire map.
    ///
    /// You can set up all non-interactable items first, and test if the CanvasPrinter works
    /// properly. Next we create two prefabs for level and boss level and add binding components.
    ///
    /// Then add some instances to your "level map". Note that these view instances will be bound to
    /// its associated view model at runtime. But we need a way to indicate which view bind to which
    /// view model. In this demo, we create a simple LevelMapNode class, it contains an index and a
    /// node type, so we can set up the connection in Editor.
    ///
    /// For example, we want the leftmost instance as the first level node, simply set levelIndex to
    /// "0" and set nodeType to "Level". This means this view will be bound to levelNodes[0].
    /// Configure all level instances you added in the "level map", the index must be unique and must
    /// be a valid index.
    ///
    /// In "CanvasPrinterExample" class, we create lists for the view models and populate with test
    /// data. The CanvasPrinter reference is set by the ComponentGetter. When the set accessor is
    /// called, we register the event handlers to CanvasPrinter.
    ///
    /// When a node is added, the OnNodeItemAdded method will be called. First we need to check if
    /// the node contains metadata. If it contains, we get the LevelMapNode component, otherwise just
    /// return. Next, we can get the binding source by indexing levelNodes with levelIndex and assign
    /// it to the Source property of its DataContext.
    ///
    /// When a node is removed, the OnNodeItemRemoved method will be called. If the node contains
    /// metadata, we get the DataContext component from the view and set null to its source property.
    /// </summary>
    public class CanvasPrinterExample : BindableMonoBehaviour
    {
        /// <summary>
        /// ViewModel for level node.
        /// </summary>
        public class LevelNode : BindableObject
        {
            private string name;
            private int starNumber;
            private ICommand selectCommand;

            #region Bindable Properties

            public string Name
            {
                get { return name; }
                set { SetProperty(ref name, value, "Name"); }
            }

            public int StarNumber
            {
                get { return starNumber; }
                set { SetProperty(ref starNumber, value, "StarNumber"); }
            }

            public ICommand SelectCommand
            {
                get { return selectCommand; }
                set { SetProperty(ref selectCommand, value, "SelectCommand"); }
            }

            #endregion
        }

        /// <summary>
        /// ViewModel for boss level node.
        /// </summary>
        public class BossLevelNode : BindableObject
        {
            private string name;
            private int requiredStarNumber;
            private ICommand selectCommand;

            #region Bindable Properties

            public string Name
            {
                get { return name; }
                set { SetProperty(ref name, value, "Name"); }
            }

            public int RequiredStarNumber
            {
                get { return requiredStarNumber; }
                set { SetProperty(ref requiredStarNumber, value, "RequiredStarNumber"); }
            }

            public ICommand SelectCommand
            {
                get { return selectCommand; }
                set { SetProperty(ref selectCommand, value, "SelectCommand"); }
            }

            #endregion
        }

        public int levelCount;
        public int levelPerWorld = 10;
        public int bossLevelCount;
        public bool enableDebug;

        private CanvasPrinter canvasPrinter;
        private List<LevelNode> levelNodes;
        private List<BossLevelNode> bossLevelNodes;

        #region Bindable Properties

        public CanvasPrinter CanvasPrinter
        {
            set
            {
                SetCanvasPrinter(value);
            }
        }

        public int TotalLevelCount
        {
            get { return levelCount + bossLevelCount; }
        }

        #endregion

        void Start()
        {
            Init();

            BindingManager.Instance.AddSource(this, typeof(CanvasPrinterExample).Name);
        }

        void OnDestroy()
        {
            BindingManager.Instance.RemoveSource(this);
        }

        private void Init()
        {
            // create level nodes
            levelNodes = new List<LevelNode>();
            for (int i = 0; i < levelCount; i++)
            {
                var node = new LevelNode();

                int worldIndex = i / levelPerWorld;
                int index = i % levelPerWorld;
                node.Name = string.Format("{0}-{1}", (char)('A' + worldIndex), index);

                node.StarNumber = (i % 3) + 1;
                node.SelectCommand = new DelegateCommand(() => SelectLevel(node));

                levelNodes.Add(node);
            }

            // create boss level nodes
            bossLevelNodes = new List<BossLevelNode>();
            for (int i = 0; i < bossLevelCount; i++)
            {
                var node = new BossLevelNode();

                node.Name = string.Format("Boss {0}", (char)('A' + i));
                node.RequiredStarNumber = 10 * (i + 1);
                node.SelectCommand = new DelegateCommand(() => SelectBossLevel(node));

                bossLevelNodes.Add(node);
            }
        }

        private void SetCanvasPrinter(CanvasPrinter value)
        {
            if (canvasPrinter != null)
            {
                // unregister
                canvasPrinter.UnregisterEvents(OnNodeItemAdded, OnNodeItemRemoved);
            }

            // save reference
            canvasPrinter = value;

            if (canvasPrinter != null)
            {
                // register
                canvasPrinter.RegisterEvents(OnNodeItemAdded, OnNodeItemRemoved);
            }
        }

        private void LogInfo(string text)
        {
            if (enableDebug)
            {
                Debug.Log(text);
            }

            MessagePopup.Instance.Show(text);
        }

        private void OnNodeItemAdded(CanvasPrinter.NodeItem item)
        {
            if (string.IsNullOrEmpty(item.node.metadata))
            {
                return;
            }

            if (enableDebug)
            {
                Debug.LogFormat("OnNodeItemAdded. metadata={0}", item.node.metadata);
            }

            // get LevelMapNode
            var levelMapNode = item.transform.GetComponent<LevelMapNode>();

            // get data context from view
            var dc = item.transform.GetComponent<IDataContext>();

            // bind source
            if (levelMapNode.nodeType == LevelMapNode.NodeType.Level)
            {
                // bind to level node
                dc.Source = levelNodes[levelMapNode.levelIndex];
            }
            else if (levelMapNode.nodeType == LevelMapNode.NodeType.BossLevel)
            {
                // bind to boss level node
                dc.Source = bossLevelNodes[levelMapNode.levelIndex];
            }
            else
            {
                Debug.LogErrorFormat("Unhandled node type {0}", levelMapNode.nodeType);
            }
        }

        private void OnNodeItemRemoved(CanvasPrinter.NodeItem item)
        {
            if (string.IsNullOrEmpty(item.node.metadata))
            {
                return;
            }

            if (enableDebug)
            {
                Debug.LogFormat("OnNodeItemRemoved. metadata={0}", item.node.metadata);
            }

            // get data context from view
            var dc = item.transform.GetComponent<IDataContext>();

            // unbind source
            dc.Source = null;
        }

        public void SelectLevel(LevelNode node)
        {
            LogInfo(string.Format("Select Level {0}, startNumber={1}", node.Name, node.StarNumber));
        }

        public void SelectBossLevel(BossLevelNode node)
        {
            LogInfo(string.Format("Select BossLevel {0}, requiredStarNumber={1}", node.Name, node.RequiredStarNumber));
        }
    }
}