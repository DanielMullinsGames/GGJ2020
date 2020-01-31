using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Peppermint.DataBinding.Editor
{
    public class GraphEditorWindow : EditorWindow
    {
        public class TreeNode
        {
            public TreeNode parent;
            public List<TreeNode> children = new List<TreeNode>();
            public List<NodeGUI> nodeList = new List<NodeGUI>();

            public void AddChild(TreeNode child)
            {
                child.parent = this;
                children.Add(child);
            }

            public void AddNode(NodeGUI node)
            {
                nodeList.Add(node);
            }
        }

        public enum NodeTypeFilter
        {
            DataContext,
            DataContextRegister,
            Binder,
            All,
        }

        private List<NodeGUI> nodeList = new List<NodeGUI>();
        private List<ConnectionGUI> connectionList = new List<ConnectionGUI>();
        private Dictionary<NodeGUI, List<ConnectionGUI>> nodeToConnectionMap = new Dictionary<NodeGUI, List<ConnectionGUI>>();

        private GraphBackground background = new GraphBackground();

        private Rect graphRegion;
        private Vector2 scrollRectMax;
        private Vector2 scrollPos;

        private int dataContextCount;
        private int dataContextRegisterCount;
        private int binderCount;
        private bool enableExtraInfo = true;
        private bool flatDepth;

        private string searchText;
        private NodeTypeFilter searchType;
        private bool searchResultDirty;
        private int currentNodeIndex;
        private List<NodeGUI> searchResultList = new List<NodeGUI>();

        [MenuItem("Tools/Data Binding/Data Binding Graph")]
        static void Init()
        {
            var window = EditorWindow.GetWindow<GraphEditorWindow>("DataBinding");
            window.Show();
        }

        void OnEnable()
        {
            ResetView();
            NodeGUI.NodeEventHandler = OnNodeEvent;
        }

        void OnDisable()
        {
            NodeGUI.NodeEventHandler = null;
        }

        void OnGUI()
        {
            DrawToolbar();

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawGraph();
            }

            HandleEvents();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                var toolbarHeight = GUILayout.Height(GraphEditorSettings.ToolbarHeight);
                if (GUILayout.Button("Create Graph", EditorStyles.toolbarButton, GUILayout.Width(100), toolbarHeight))
                {
                    CreateGraph();
                }
                if (GUILayout.Button("Check Error", EditorStyles.toolbarButton, GUILayout.Width(100), toolbarHeight))
                {
                    CheckError();
                }
                EditorGUILayout.LabelField("Find what", GUILayout.Width(70), toolbarHeight);

                EditorGUI.BeginChangeCheck();
                {
                    searchText = EditorGUILayout.TextField(string.Empty, searchText, new GUIStyle("ToolbarSeachTextField"), GUILayout.Width(160), toolbarHeight);
                    if (GUILayout.Button("Close", "ToolbarSeachCancelButtonEmpty"))
                    {
                        // reset text
                        searchText = null;
                    }
                    EditorGUILayout.LabelField("NodeType Filter", GUILayout.Width(100), toolbarHeight);
                    searchType = (NodeTypeFilter)EditorGUILayout.EnumPopup(searchType, EditorStyles.toolbarPopup, GUILayout.Width(140), toolbarHeight);
                }
                if (EditorGUI.EndChangeCheck())
                {
                    searchResultDirty = true;
                }
                if (GUILayout.Button("Find Next", EditorStyles.toolbarButton, GUILayout.Width(100), toolbarHeight))
                {
                    Find();
                }

                enableExtraInfo = GUILayout.Toggle(enableExtraInfo, "Enable Extra Info", EditorStyles.toolbarButton, GUILayout.Width(150), toolbarHeight);
                flatDepth = GUILayout.Toggle(flatDepth, "Flat Depth", EditorStyles.toolbarButton, GUILayout.Width(120), toolbarHeight);
                GraphEditorSettings.enableDrag = GUILayout.Toggle(GraphEditorSettings.enableDrag, "Enable Drag", EditorStyles.toolbarButton, GUILayout.Width(120), toolbarHeight);

                GUILayout.FlexibleSpace();

                EditorGUILayout.LabelField(string.Format("Binder: {0}", binderCount), GUILayout.Width(100), toolbarHeight);
                EditorGUILayout.LabelField(string.Format("DataContext: {0}", dataContextCount), GUILayout.Width(120), toolbarHeight);
                EditorGUILayout.LabelField(string.Format("DataContextRegister: {0}", dataContextRegisterCount), GUILayout.Width(180), toolbarHeight);
            }
        }

        private void DrawGraph()
        {
            // draw background
            background.Draw(graphRegion, scrollPos);

            using (var scrollScope = new EditorGUILayout.ScrollViewScope(scrollPos))
            {
                scrollPos = scrollScope.scrollPosition;

                // draw connections
                connectionList.ForEach(x => x.Draw());

                // draw nodes
                BeginWindows();
                nodeList.ForEach(x => x.Draw());
                EndWindows();

                // handle GUI events
                HandleGraphGUIEvents();

                // set rect for scroll
                if (nodeList.Count > 0)
                {
                    GUILayoutUtility.GetRect(new GUIContent(string.Empty), GUIStyle.none, GUILayout.Width(scrollRectMax.x), GUILayout.Height(scrollRectMax.y));
                }
            }

            if (Event.current.type == EventType.Repaint)
            {
                var newRect = GUILayoutUtility.GetLastRect();
                if (newRect != graphRegion)
                {
                    graphRegion = newRect;
                    Repaint();
                }
            }
        }

        private void HandleEvents()
        {

        }


        private void HandleGraphGUIEvents()
        {
            var e = Event.current;

            switch (e.type)
            {
                case EventType.MouseUp:
                    {
                        if (e.button == 0)
                        {
                            SelectNode(null);
                        }
                    }
                    break;

                case EventType.MouseDrag:
                    {
                        if (e.button == 2)
                        {
                            // middle
                            var newPos = scrollPos - e.delta;
                            scrollPos.x = Mathf.Max(newPos.x, 0f);
                            scrollPos.y = Mathf.Max(newPos.y, 0f);
                            Repaint();
                        }
                    }
                    break;

                default:
                    break;
            }
        }

        private void OnNodeEvent(NodeEvent ev)
        {
            if (ev.eventType == NodeEvent.EventType.Touched)
            {
                SelectNode(ev.eventSourceNode);
            }
        }

        private void UpdateScrollRect()
        {
            if (nodeList.Count == 0)
            {
                return;
            }

            var right = nodeList.OrderByDescending(x => x.Right).Select(x => x.Right).First() + GraphEditorSettings.WindowSpan;
            var bottom = nodeList.OrderByDescending(x => x.Bottom).Select(x => x.Bottom).First() + GraphEditorSettings.WindowSpan;

            scrollRectMax = new Vector2(right, bottom);
        }

        private void SetupView(TreeNode root)
        {
            int index = 0;
            float x = GraphEditorSettings.TreeViewStartX;
            float y = GraphEditorSettings.TreeViewStartY;

            SetupLayout(root, 0, x, ref y, ref index);

            SetupConnection(root);
        }

        private void SetupLayout(TreeNode treeNode, int depth, float startX, ref float y, ref int index)
        {
            float step = GraphEditorSettings.NodeBaseWidth + GraphEditorSettings.TreeViewHorizontalMargin;

            foreach (var item in treeNode.nodeList)
            {
                var x = step * depth + startX;
                item.Position = new Vector2(x, y);

                // calculate next
                y += (item.nodeRect.height + GraphEditorSettings.TreeViewVerticalMargin);

                // add to graph
                AddNode(item);
                index++;
            }

            int newDepth = 0;

            if (flatDepth)
            {
                bool hasAnyComponent = treeNode.nodeList.Count > 0;

                if (hasAnyComponent)
                {
                    newDepth = depth + 1;
                }
                else
                {
                    newDepth = depth;
                }
            }
            else
            {
                newDepth = depth + 1;
            }

            // iterate children
            foreach (var item in treeNode.children)
            {
                SetupLayout(item, newDepth, startX, ref y, ref index);
            }
        }

        private void SetupConnection(TreeNode treeNode)
        {
            foreach (var item in treeNode.nodeList)
            {
                if (item.NodeType == NodeType.Binder)
                {
                    var connection = CreateConnection();

                    connection.startNode = FindNodeUp(treeNode, NodeType.DataContext);
                    connection.endNode = item;

                    AddConnection(connection, true);
                }

                if (item.NodeType == NodeType.DataContextRegister)
                {
                    var connection = CreateConnection();

                    connection.startNode = item;
                    connection.endNode = treeNode.nodeList.Find(x => x.NodeType == NodeType.DataContext);

                    AddConnection(connection, false);
                }
            }

            // iterate children
            foreach (var item in treeNode.children)
            {
                SetupConnection(item);
            }
        }

        private void ResetView()
        {
            // clear current
            nodeList.Clear();
            connectionList.Clear();
            nodeToConnectionMap.Clear();
            searchResultList.Clear();

            searchText = null;
            searchType = NodeTypeFilter.All;
            searchResultDirty = true;
            currentNodeIndex = 0;

            binderCount = 0;
            dataContextCount = 0;
            dataContextRegisterCount = 0;
        }

        public void CheckError()
        {
            var selectedTransform = Selection.activeTransform;

            if (selectedTransform == null)
            {
                Debug.LogError("No transform is selected");
                return;
            }
            else
            {
                var transformPath = GetGameObjectPath(selectedTransform);
                Debug.LogFormat("Check DataBinding for {0}", transformPath);
            }

            // get all binders
            var binderList = selectedTransform.GetComponentsInChildren<MonoBehaviour>(true).Where(x => x.GetType().IsBinder());

            var usedList = new List<IDataContext>();

            // iterate binder list
            foreach (var item in binderList)
            {
                // get data context
                var dc = BindingUtility.FindDataContext(item.transform);
                if (dc == null)
                {
                    continue;
                }

                if (!usedList.Contains(dc))
                {
                    usedList.Add(dc);
                }
            }

            // get all data context
            var dataContextList = selectedTransform.GetComponentsInChildren<IDataContext>(true);

            foreach (var item in dataContextList)
            {
                if (!usedList.Contains(item))
                {
                    var go = item as MonoBehaviour;
                    string path = GetGameObjectPath(go.transform);
                    string info = string.Format("Find unused DataContext, path={0}", path);
                    Debug.LogWarning(info, go);
                }
            }
        }

        public void CreateGraph()
        {
            var selectedTransform = Selection.activeTransform;

            if (selectedTransform == null)
            {
                Debug.LogError("No transform is selected");
                return;
            }
            else
            {
                var transformPath = GetGameObjectPath(selectedTransform);
                Debug.Log(string.Format("Create binding graph for {0}", transformPath), selectedTransform);
            }

            ResetView();

            // build tree from selected
            var root = new TreeNode();
            BuildTree(selectedTransform, root);

            SetDataContextInfo(selectedTransform, root);

            // get count
            dataContextCount = selectedTransform.GetComponentsInChildren<IDataContext>(true).Count();
            dataContextRegisterCount = selectedTransform.GetComponentsInChildren<DataContextRegister>(true).Count();
            binderCount = selectedTransform.GetComponentsInChildren<MonoBehaviour>(true).Where(x => x.GetType().IsBinder()).Count();

            // setup view
            SetupView(root);

            UpdateScrollRect();
        }

        private void DoSearch()
        {
            // do new search
            searchResultList.Clear();

            foreach (var item in nodeList)
            {
                if (searchType != NodeTypeFilter.All)
                {
                    // filter node type
                    if ((int)item.NodeType != (int)searchType)
                    {
                        continue;
                    }
                }

                if (item.ExtraInfo == null)
                {
                    continue;
                }

                if (item.ExtraInfo.IndexOf(searchText, StringComparison.InvariantCultureIgnoreCase) >= 0)
                {
                    // add to result list
                    searchResultList.Add(item);
                }
            }

            searchResultDirty = false;
            currentNodeIndex = 0;

            if (searchResultList.Count == 0)
            {
                Debug.LogFormat("Can't find \"{0}\"", searchText);
            }
            else
            {
                Debug.LogFormat("Find {0} nodes", searchResultList.Count);
            }
        }

        private void Find()
        {
            if (string.IsNullOrEmpty(searchText))
            {
                return;
            }

            if (searchResultDirty)
            {
                DoSearch();
            }

            if (searchResultList.Count == 0)
            {
                // find nothing
                return;
            }

            // get node from list
            NodeGUI currentNode = searchResultList[currentNodeIndex];

            // wrap around
            currentNodeIndex = (currentNodeIndex + 1) % searchResultList.Count;

            Debug.LogFormat("Node {0}, transform path {1}", currentNode.name, EditorHelper.GetTransformPath(currentNode.target.transform));

            FocusNode(currentNode);
            SelectNode(currentNode);
        }

        private void BuildTree(Transform transform, TreeNode treeNode)
        {
            // DataContextRegister
            {
                var list = transform.GetComponents<DataContextRegister>();
                foreach (var item in list)
                {
                    var node = CreateNode(NodeType.DataContextRegister);
                    node.name = item.GetType().Name;
                    node.ExtraInfo = item.requiredSource;
                    node.target = item;

                    treeNode.AddNode(node);
                }
            }

            // DataContext
            {
                var list = transform.GetComponents<MonoBehaviour>().Where(x => x is IDataContext);
                foreach (var item in list)
                {
                    var node = CreateNode(NodeType.DataContext);
                    node.name = item.GetType().Name;
                    node.target = item;

                    treeNode.AddNode(node);
                }
            }

            // Binder
            {
                var list = transform.GetComponents<MonoBehaviour>().Where(x => x.GetType().IsBinder());
                foreach (var item in list)
                {
                    var node = CreateNode(NodeType.Binder);
                    node.name = item.GetType().Name;
                    if (enableExtraInfo)
                    {
                        var sourceList = GetBindingSourceList(item);
                        node.ExtraInfo = CollectSourceInfo(sourceList);
                    }
                    node.target = item;

                    treeNode.AddNode(node);
                }
            }

            if (transform.childCount == 0)
            {
                return;
            }

            // iterate children
            foreach (Transform item in transform)
            {
                // create new tree node
                var newNode = new TreeNode();
                treeNode.AddChild(newNode);

                BuildTree(item, newNode);
            }
        }

        private void SetDataContextInfo(Transform root, TreeNode treeNode)
        {
            if (!enableExtraInfo)
            {
                return;
            }

            // get all binders
            var binderList = root.GetComponentsInChildren<MonoBehaviour>(true).Where(x => x.GetType().IsBinder());

            var dictionary = new Dictionary<IDataContext, List<string>>();

            // iterate binder list
            foreach (var item in binderList)
            {
                // get data context
                var dc = BindingUtility.FindDataContext(item.transform);
                if (dc == null)
                {
                    continue;
                }

                if (!dictionary.ContainsKey(dc))
                {
                    // add new entry
                    dictionary.Add(dc, new List<string>());
                }

                var list = dictionary[dc];
                var sourceList = GetBindingSourceList(item);

                foreach (var source in sourceList)
                {
                    if (list.Contains(source))
                    {
                        continue;
                    }

                    list.Add(source);
                }
            }

            // get all data context node
            var nodeList = new List<NodeGUI>();
            GetNodeList(treeNode, NodeType.DataContext, nodeList);

            foreach (var item in nodeList)
            {
                var dc = item.target as IDataContext;

                if (!dictionary.ContainsKey(dc))
                {
                    string path = GetGameObjectPath(item.target.transform);
                    string info = string.Format("Find unused DataContext, path={0}", path);
                    Debug.LogWarning(info, item.target);
                    continue;
                }

                var sourceList = dictionary[dc];

                // set extra info
                var sourceInfo = CollectSourceInfo(sourceList);
                item.ExtraInfo = sourceInfo;
            }
        }

        private void GetNodeList(TreeNode treeNode, NodeType nodeType, List<NodeGUI> output)
        {
            foreach (var item in treeNode.nodeList)
            {
                if (item.NodeType == nodeType)
                {
                    // add to list
                    output.Add(item);
                }
            }

            foreach (var item in treeNode.children)
            {
                GetNodeList(item, nodeType, output);
            }
        }

        private string GetGameObjectPath(Transform transform)
        {
            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }
            return path;
        }

        private int GenerateWindowID()
        {
            int id = -1;

            foreach (var item in nodeList)
            {
                if (item.windowID > id)
                {
                    id = item.windowID;
                }
            }

            return id + 1;
        }

        private void FocusNode(NodeGUI node)
        {
            var offset = node.nodeRect.center - graphRegion.center;

            var newScrollPos = new Vector2();
            newScrollPos.x = Mathf.Clamp(offset.x, 0f, scrollRectMax.x);
            newScrollPos.y = Mathf.Clamp(offset.y, 0f, scrollRectMax.y);

            scrollPos = newScrollPos;

            Repaint();
        }

        public NodeGUI CreateNode(NodeType nodeType)
        {
            var node = new NodeGUI();

            // set default value
            node.UpdateRect();
            node.NodeType = nodeType;

            return node;
        }

        public void SelectNode(NodeGUI node)
        {
            foreach (var item in nodeList)
            {
                item.SetInactive();
            }

            if (node != null)
            {
                node.SetActive();

                // select associated connection
                if (nodeToConnectionMap.ContainsKey(node))
                {
                    var list = nodeToConnectionMap[node];
                    SelectConnections(list);
                }
            }
            else
            {
                // set target
                Selection.activeObject = null;

                SelectConnection(null);
            }

            Repaint();
        }

        public void SelectConnection(ConnectionGUI connection)
        {
            foreach (var item in connectionList)
            {
                item.SetInactive();
            }

            if (connection != null)
            {
                connection.SetActive();
            }
        }

        public void SelectConnections(IEnumerable<ConnectionGUI> connections)
        {
            foreach (var item in connectionList)
            {
                item.SetInactive();
            }

            foreach (var item in connections)
            {
                item.SetActive();
            }
        }

        private List<string> GetBindingSourceList(MonoBehaviour monoObject)
        {
            var list = new List<string>();

            var type = monoObject.GetType();

            // get all public fields end with 'path'
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance).Where(x => x.FieldType == typeof(string) && x.Name.EndsWith("path", StringComparison.InvariantCultureIgnoreCase));

            foreach (var field in fields)
            {
                var value = field.GetValue(monoObject) as string;
                if (value != null)
                {
                    list.Add(value);
                }
            }

            // get binder attribute
            var binderAttribute = BindingUtility.GetAttribute<BinderAttribute>(type, false);

            if (!string.IsNullOrEmpty(binderAttribute.SourcePathListMethod))
            {
                var method = type.GetMethod(binderAttribute.SourcePathListMethod, BindingFlags.Public | BindingFlags.Instance);

                if (method != null)
                {
                    // call method to get path list
                    var sourcePathList = method.Invoke(monoObject, null) as IEnumerable<string>;
                    list.AddRange(sourcePathList);
                }
            }

            var resultList = new List<string>();

            // remove empty
            foreach (var item in list)
            {
                if (string.IsNullOrEmpty(item))
                {
                    continue;
                }

                resultList.Add(item);
            }

            return resultList;
        }

        private string CollectSourceInfo(List<string> sourceList)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var item in sourceList)
            {
                if (sb.Length != 0)
                {
                    sb.AppendLine();
                }

                sb.AppendFormat("-{0}", item);
            }

            return sb.ToString();
        }

        public ConnectionGUI CreateConnection()
        {
            var connection = new ConnectionGUI();

            return connection;
        }

        public void AddNode(NodeGUI node)
        {
            node.windowID = GenerateWindowID();
            nodeList.Add(node);
        }

        public void AddConnection(ConnectionGUI connection, bool twoWay)
        {
            AddAssociation(connection.startNode, connection);

            if (twoWay)
            {
                AddAssociation(connection.endNode, connection);
            }

            connectionList.Add(connection);
        }

        private void AddAssociation(NodeGUI node, ConnectionGUI connection)
        {
            if (node == null)
            {
                return;
            }

            if (!nodeToConnectionMap.ContainsKey(node))
            {
                // add it
                nodeToConnectionMap.Add(node, new List<ConnectionGUI>());
            }

            nodeToConnectionMap[node].Add(connection);
        }

        private NodeGUI FindNodeUp(TreeNode current, NodeType nodeType)
        {
            NodeGUI result = null;

            while (current != null)
            {
                result = current.nodeList.Find(x => x.NodeType == nodeType);
                if (result != null)
                {
                    break;
                }

                // go up
                current = current.parent;
            }

            return result;
        }

    }
}
