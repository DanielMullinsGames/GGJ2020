using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Peppermint.DataBinding.Editor
{
    [CustomEditor(typeof(CanvasScanner))]
    public class CanvasScannerEditor : UnityEditor.Editor
    {
        public class DataContainer
        {
            public Dictionary<CanvasData.Node, Rect> nodeRectDictionary;
            public Vector3[] worldCorners;
            public Vector2[] localCorners;

            public DataContainer()
            {
                nodeRectDictionary = new Dictionary<CanvasData.Node, Rect>();
                worldCorners = new Vector3[4];
                localCorners = new Vector2[4];
            }
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Scan"))
            {
                Scan();
            }
        }

        private void Scan()
        {
            var scanner = (CanvasScanner)target;

            if (scanner.data == null)
            {
                Debug.LogError("CanvasData is null");
                return;
            }

            if (!VerifyTransform(scanner.content))
            {
                Debug.LogError("Invalid content transform");
                return;
            }

            // create containers
            var dataContainer = new DataContainer();

            if (scanner.rectContainer != null)
            {
                // destroy all debug objects.
                var pendingList = new List<GameObject>();
                foreach (Transform child in scanner.rectContainer)
                {
                    pendingList.Add(child.gameObject);
                }

                foreach (var item in pendingList)
                {
                    GameObject.DestroyImmediate(item);
                }
            }

            if (scanner.generateDebugRect)
            {
                // copy RectTransform value from content to rectContainer
                scanner.rectContainer.anchorMax = scanner.content.anchorMax;
                scanner.rectContainer.anchorMin = scanner.content.anchorMin;
                scanner.rectContainer.anchoredPosition = scanner.content.anchoredPosition;
                scanner.rectContainer.sizeDelta = scanner.content.sizeDelta;
            }

            // setup basic info
            scanner.data.layout = scanner.layout;
            scanner.data.width = scanner.content.sizeDelta.x;
            scanner.data.height = scanner.content.sizeDelta.y;
            scanner.data.sectionWidth = scanner.sectionWidth;
            scanner.data.sectionHeight = scanner.sectionHeight;

            BuildNodes(dataContainer);

            BuildSections(dataContainer);

            // mark dirty
            EditorUtility.SetDirty(scanner.data);

            Debug.LogFormat("Scan finished. nodeCount={0}, sectionCount={1}", scanner.data.nodeList.Count, scanner.data.sectionList.Count);
        }

        private void BuildNodes(DataContainer dataContainer)
        {
            var scanner = (CanvasScanner)target;

            // get all items from content node
            var list = GetPrefabInstanceList();

            var nodeList = new List<CanvasData.Node>();

            int index = 0;
            foreach (var item in list)
            {
                var nodeRT = item.transform as RectTransform;

                if (!VerifyTransform(nodeRT))
                {
                    continue;
                }

                var node = new CanvasData.Node();

                // get prefab parent
                node.prefab = PrefabUtility.GetPrefabParent(item) as GameObject;

                // get position and size
                node.anchoredPosition = GetAnchoredPosition(nodeRT, scanner.content);
                node.sizeDelta = nodeRT.sizeDelta;
                node.index = index++;

                // set rotation
                node.rotation = nodeRT.rotation.eulerAngles.z;

                // get metadata
                var metadata = nodeRT.GetComponent<ISerializableCanvasNode>();
                if (metadata != null)
                {
                    // serialize metadata to string
                    node.metadata = metadata.Serialize();
                }

                // add it
                nodeList.Add(node);

                // add node rect
                var rect = GetNodeRect(nodeRT, scanner.content, dataContainer);
                dataContainer.nodeRectDictionary.Add(node, rect);
            }

            if (scanner.generateDebugRect)
            {
                foreach (var node in nodeList)
                {
                    var rect = dataContainer.nodeRectDictionary[node];

                    CreateDebugRect(node, rect);
                }
            }

            // set data
            scanner.data.nodeList = nodeList;
        }

        private void BuildSections(DataContainer dataContainer)
        {
            var scanner = (CanvasScanner)target;

            var sectionList = new List<CanvasData.Section>();

            // calculate section number
            int sectionNumber;
            if (scanner.layout == CanvasData.LayoutType.Horizontal)
            {
                sectionNumber = Mathf.CeilToInt(scanner.data.width / scanner.sectionWidth);
            }
            else
            {
                sectionNumber = Mathf.CeilToInt(scanner.data.height / scanner.sectionHeight);
            }

            var contentRect = scanner.content.rect;

            float x, y;

            // foreach section
            for (int index = 0; index < sectionNumber; index++)
            {
                if (scanner.layout == CanvasData.LayoutType.Horizontal)
                {
                    x = contentRect.x + scanner.sectionWidth * index;
                    y = contentRect.y;
                }
                else
                {
                    x = contentRect.x;
                    y = contentRect.y + scanner.sectionHeight * index;
                }

                // get section bounds
                var sectionRect = new Rect(x, y, scanner.sectionWidth, scanner.sectionHeight);

                // create section
                var section = new CanvasData.Section();
                section.index = index;

                foreach (var item in scanner.data.nodeList)
                {
                    var nodeRect = dataContainer.nodeRectDictionary[item];

                    if (Overlaps(sectionRect, nodeRect))
                    {
                        // add node index
                        section.nodeIndexList.Add(item.index);
                    }
                }

                // add section to list
                sectionList.Add(section);
            }

            // set data
            scanner.data.sectionList = sectionList;
        }

        private bool Overlaps(Rect lhs, Rect rhs)
        {
            return ((((rhs.xMax >= lhs.xMin) && (rhs.xMin <= lhs.xMax)) && (rhs.yMax >= lhs.yMin)) && (rhs.yMin <= lhs.yMax));
        }

        private List<GameObject> GetPrefabInstanceList()
        {
            var scanner = (CanvasScanner)target;

            List<RectTransform> allList = new List<RectTransform>();
            scanner.content.GetComponentsInChildren<RectTransform>(allList);

            List<GameObject> validList = new List<GameObject>();

            foreach (var item in allList)
            {
                var go = item.gameObject;

                var result = PrefabUtility.GetPrefabType(go);
                if (result != PrefabType.PrefabInstance)
                {
                    continue;
                }

                var root = PrefabUtility.FindPrefabRoot(go);

                if (!validList.Contains(root))
                {
                    validList.Add(go);
                }
            }

            return validList;
        }

        private bool VerifyTransform(RectTransform rt)
        {
            if (rt.anchorMin != rt.anchorMax)
            {
                // stretched
                return false;
            }

            if (rt.localScale != Vector3.one)
            {
                // scaled
                return false;
            }

            var rotationAngles = rt.rotation.eulerAngles;
            if (rotationAngles.x != 0f || rotationAngles.y != 0f)
            {
                // rotated
                return false;
            }

            return true;
        }

        private Rect GetNodeRect(RectTransform sourceRT, RectTransform targetRT, DataContainer dataContainer)
        {
            var scanner = (CanvasScanner)target;

            // get all RectTransforms
            var rectTransforms = sourceRT.GetComponentsInChildren<RectTransform>(scanner.includeInactiveRect);

            if (rectTransforms.Length == 0)
            {
                // return empty rect
                return new Rect();
            }

            float xmin = float.MaxValue;
            float xmax = float.MinValue;
            float ymin = float.MaxValue;
            float ymax = float.MinValue;

            foreach (var rt in rectTransforms)
            {
                rt.GetWorldCorners(dataContainer.worldCorners);

                for (int index = 0; index < 4; index++)
                {
                    var point = RectTransformUtility.WorldToScreenPoint(null, dataContainer.worldCorners[index]);
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(targetRT, point, null, out dataContainer.localCorners[index]);
                }

                UpdateMinMax(dataContainer.localCorners, ref xmin, ref xmax, ref ymin, ref ymax);
            }

            // create rect
            var rect = Rect.MinMaxRect(xmin, ymin, xmax, ymax);
            return rect;
        }

        private static void UpdateMinMax(Vector2[] corners, ref float xmin, ref float xmax, ref float ymin, ref float ymax)
        {
            for (int i = 0; i < corners.Length; i++)
            {
                if (corners[i].x < xmin)
                {
                    xmin = corners[i].x;
                }

                if (corners[i].x > xmax)
                {
                    xmax = corners[i].x;
                }

                if (corners[i].y < ymin)
                {
                    ymin = corners[i].y;
                }

                if (corners[i].y > ymax)
                {
                    ymax = corners[i].y;
                }
            }
        }

        private static Vector2 GetAnchoredPosition(RectTransform sourceRT, RectTransform targetRT)
        {
            var worldPosition = sourceRT.position;
            var point = RectTransformUtility.WorldToScreenPoint(null, worldPosition);

            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(targetRT, point, null, out localPos);
            return localPos;
        }

        private void CreateDebugRect(CanvasData.Node node, Rect rect)
        {
            var scanner = (CanvasScanner)target;

            var go = DefaultControls.CreateImage(new DefaultControls.Resources());
            go.transform.SetParent(scanner.rectContainer, false);

            // set name
            go.name = string.Format("NodeRect {0:D}", node.index);
            var rt = go.transform as RectTransform;

            // set position
            rt.anchoredPosition = rect.center;
            rt.sizeDelta = rect.size;

            // set color
            var image = go.GetComponent<Image>();
            image.color = scanner.rectColor;
        }
    }
}