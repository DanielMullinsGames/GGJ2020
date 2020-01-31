using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using Kilt;
using Kilt.EasyRopes2D;
using Kilt.Extensions;

namespace KiltEditor.EasyRopes2D
{
    public class RopeCreatorWindow : EditorWindow
    {
        static RopeCreatorWindowInformation m_windowInformation = new RopeCreatorWindowInformation();

        #region Unity Functions

        static void Init()
        {
            RopeCreatorWindow v_window = EditorWindow.GetWindow(typeof(RopeCreatorWindow)) as RopeCreatorWindow;
            if (v_window != null)
                v_window.Refresh(true);
        }

        protected virtual void OnEnable()
        {
            Refresh(true);
        }

        protected virtual void Refresh(bool p_force = true)
        {
            if (p_force || m_windowInformation.Ropes != null)
            {
                GameObject v_parentTransform = GameObject.Find("RopesContainer");
                if (v_parentTransform != null)
                    m_windowInformation.RopeParent = v_parentTransform.transform;
                m_windowInformation.Ropes = new List<Rope2D>(RopeEditorInternalUtils.GetAssetsOfType<Rope2D>(".prefab"));
                m_windowInformation.SelectedRopeIndex = 0;
            }
        }

        protected virtual void OnGUI()
        {
            if (m_windowInformation == null)
                m_windowInformation = new RopeCreatorWindowInformation();

            EditorGUILayout.BeginHorizontal();

            bool v_refreshClicked = GUILayout.Button("Refresh Informations");

            EditorGUILayout.EndHorizontal();

            if (v_refreshClicked)
                Refresh();

            EditorGUILayout.BeginHorizontal();

            m_windowInformation.RopeParent = (Transform)EditorGUILayout.ObjectField("Rope Parent", m_windowInformation.RopeParent, typeof(Transform), true);

            EditorGUILayout.EndHorizontal();

            if (m_windowInformation.Ropes != null)
            {
                EditorGUILayout.BeginHorizontal();

                GUILayout.Label("Ropes");
                m_windowInformation.SelectedRopeIndex = EditorGUILayout.Popup(m_windowInformation.SelectedRopeIndex, m_windowInformation.Ropes.GetStringList().ToArray());
                Rope2D v_rope = m_windowInformation.Ropes != null && m_windowInformation.SelectedRopeIndex >= 0 && m_windowInformation.Ropes.Count > m_windowInformation.SelectedRopeIndex ? m_windowInformation.Ropes[m_windowInformation.SelectedRopeIndex] : null;
                if (m_windowInformation.Rope != v_rope)
                {
                    m_windowInformation.Rope = v_rope;
                    if (v_rope != null)
                    {
                        m_windowInformation.RopeDepth = v_rope.RopeDepth;
                        m_windowInformation.CustomPrefabNode = v_rope.CustomNodePrefab;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            if (m_windowInformation.Rope != null)
            {
                EditorGUILayout.BeginHorizontal();

                m_windowInformation.ObjectA = (GameObject)EditorGUILayout.ObjectField("Object A", m_windowInformation.ObjectA, typeof(GameObject), true);

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();

                m_windowInformation.ObjectB = (GameObject)EditorGUILayout.ObjectField("Object B", m_windowInformation.ObjectB, typeof(GameObject), true);

                EditorGUILayout.EndHorizontal();

                //Sorting Layer
                string[] v_sortingLayers = GetSortingLayerNames();
                int v_sortingIndex = EditorGUILayout.Popup("Rope Sorting Layer", GetObjectIndexInArrayOrStrings(v_sortingLayers, m_windowInformation.RopeSortingLayerName), v_sortingLayers);
                m_windowInformation.RopeSortingLayerName = v_sortingLayers.Length > v_sortingIndex && v_sortingIndex >= 0 ? v_sortingLayers[v_sortingIndex] : "Default";

                m_windowInformation.RopeDepth = EditorGUILayout.IntField("Rope Z Order", m_windowInformation.RopeDepth);
                m_windowInformation.CustomPrefabNode = EditorGUILayout.ObjectField("Custom Node Prefab", m_windowInformation.CustomPrefabNode, typeof(GameObject), false) as GameObject;

                if (m_windowInformation.ObjectA != null && m_windowInformation.ObjectB != null)
                {
                    EditorGUILayout.BeginHorizontal();

                    GUILayout.Label("Automatic Calculate Amount of Nodes");

                    m_windowInformation.AutomaticCalculateNodes = EditorGUILayout.BeginToggleGroup("", m_windowInformation.AutomaticCalculateNodes);
                    EditorGUILayout.EndToggleGroup();

                    EditorGUILayout.EndHorizontal();

                    if (!m_windowInformation.AutomaticCalculateNodes)
                    {
                        EditorGUILayout.BeginHorizontal();

                        GUILayout.Label("Amount of Nodes");
                        m_windowInformation.AmountOfNodes = EditorGUILayout.IntField(m_windowInformation.AmountOfNodes);

                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.BeginHorizontal();

                    GUILayout.Label("Use Prefab Rope Scale");

                    m_windowInformation.UsePrefabRopeScale = EditorGUILayout.BeginToggleGroup("", m_windowInformation.UsePrefabRopeScale);
                    EditorGUILayout.EndToggleGroup();

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();

                    bool v_buttonClicked = GUILayout.Button("Create Rope");

                    EditorGUILayout.EndHorizontal();

                    if (v_buttonClicked)
                        CreateRopeClicked();
                }
            }
            if (GUI.changed)
                EditorUtility.SetDirty(this);
        }

        #endregion

        #region Helper Functions

        protected virtual void CreateRopeClicked()
        {
            CreateRope();
        }

        protected virtual Rope2D CreateRope()
        {
            Rope2D v_returnedRope = null;
            if (m_windowInformation.Rope != null)
            {
                GameObject v_ropeInSceneObject = UnityEditor.PrefabUtility.InstantiatePrefab(m_windowInformation.Rope.gameObject) as GameObject;
                v_ropeInSceneObject.transform.parent = m_windowInformation.RopeParent;
                if (m_windowInformation.UsePrefabRopeScale)
                    v_ropeInSceneObject.transform.localScale = m_windowInformation.Rope.transform.localScale;
                v_ropeInSceneObject.name = m_windowInformation.Rope.name + "[(" + m_windowInformation.ObjectA.name.Replace("(Selected)", "") + ") to (" + m_windowInformation.ObjectB.name.Replace("(Selected)", "") + ")]";

                Rope2D v_ropeInScene = v_ropeInSceneObject.GetComponent<Rope2D>();
                v_ropeInScene.ObjectA = m_windowInformation.ObjectA;
                v_ropeInScene.ObjectB = m_windowInformation.ObjectB;
                v_ropeInScene.RopeDepth = m_windowInformation.RopeDepth;
                v_ropeInScene.RopeSortingLayerName = m_windowInformation.RopeSortingLayerName;
                v_ropeInScene.AutoCalculateAmountOfNodes = m_windowInformation.AutomaticCalculateNodes;
                v_ropeInScene.AmountOfNodes = m_windowInformation.AmountOfNodes;
                v_ropeInScene.CustomNodePrefab = m_windowInformation.CustomPrefabNode;

                v_returnedRope = v_ropeInScene;
                EditorUtility.SetDirty(v_ropeInSceneObject);
            }
            return v_returnedRope;
        }

        #endregion

        #region Other Helper Functions

        public int GetObjectIndexInArrayOrStrings(string[] p_array, string p_textObject)
        {
            int v_index = -1;
            if (p_array != null)
            {
                v_index = 0;
                foreach (string v_string in p_array)
                {
                    if (string.Equals(v_string, p_textObject))
                    {
                        break;
                    }
                    else
                        v_index++;
                }
            }
            return v_index;
        }

        public string[] GetSortingLayerNames()
        {
            try
            {
                System.Type v_internalEditorUtilityType = typeof(UnityEditorInternal.InternalEditorUtility);
                PropertyInfo v_sortingLayersProperty = v_internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
                return (string[])v_sortingLayersProperty.GetValue(null, new object[0]);
            }
            catch { }
            List<string> v_list = new List<string>();
            v_list.Add("Default");
            return v_list.ToArray();
        }

        public int[] GetSortingLayerUniqueIDs()
        {
            try
            {
                System.Type v_internalEditorUtilityType = typeof(UnityEditorInternal.InternalEditorUtility);
                PropertyInfo v_sortingLayersProperty = v_internalEditorUtilityType.GetProperty("sortingLayerUniqueIDs", BindingFlags.Static | BindingFlags.NonPublic);
                return (int[])v_sortingLayersProperty.GetValue(null, new object[0]);
            }
            catch { }

            List<int> v_list = new List<int>();
            v_list.Add(0);
            return v_list.ToArray();
        }

        #endregion
    }

    public class RopeCreatorWindowInformation
    {
        #region Private Variables

        [SerializeField]
        private Transform m_ropeParent = null;
        [SerializeField]
        private Rope2D m_rope;
        [SerializeField]
        private GameObject m_objectA;
        [SerializeField]
        private GameObject m_objectB;
        [SerializeField]
        private bool m_automaticCalculateNodes = true;
        [SerializeField]
        private int m_amountOfNodes = 10;
        [SerializeField]
        private bool m_usePrefabRopeScale = true;
        [SerializeField]
        private int m_ropeDepth = 3;
        [SerializeField]
        private string m_ropeSortingLayerName = "Default";
        [SerializeField]
        private GameObject m_customPrefabNode = null;

        [SerializeField]
        List<Rope2D> m_ropes = null;
        [SerializeField]
        int m_selectedRopeIndex = 0;

        #endregion

        #region Public Properties

        public Transform RopeParent { get { return m_ropeParent; } set { m_ropeParent = value; } }
        public Rope2D Rope { get { return m_rope; } set { m_rope = value; } }
        public GameObject ObjectA { get { return m_objectA; } set { m_objectA = value; } }
        public GameObject ObjectB { get { return m_objectB; } set { m_objectB = value; } }
        public bool AutomaticCalculateNodes { get { return m_automaticCalculateNodes; } set { m_automaticCalculateNodes = value; } }
        public int AmountOfNodes { get { return m_amountOfNodes; } set { m_amountOfNodes = value; } }
        public bool UsePrefabRopeScale { get { return m_usePrefabRopeScale; } set { m_usePrefabRopeScale = value; } }
        public int RopeDepth { get { return m_ropeDepth; } set { m_ropeDepth = value; } }
        public string RopeSortingLayerName { get { return m_ropeSortingLayerName; } set { m_ropeSortingLayerName = value; } }
        public GameObject CustomPrefabNode { get { return m_customPrefabNode; } set { m_customPrefabNode = value; } }

        public List<Rope2D> Ropes { get { return m_ropes; } set { m_ropes = value; } }
        public int SelectedRopeIndex { get { return m_selectedRopeIndex; } set { m_selectedRopeIndex = value; } }


        #endregion
    }
}
