using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Kilt;
using Kilt.EasyRopes2D;
using Kilt.Extensions;
using Kilt.Collections;

namespace KiltEditor.EasyRopes2D
{
    public class TackRopeCreatorWindow : RopeCreatorWindow
    {
        #region Private Variables

        protected static List<Rope2D> m_currentCreatedRopes = new List<Rope2D>();
        //protected static List<GameObject> m_selectedObjects = new List<GameObject>();
        protected static ArrayDict<int, GameObject> m_selectedObjects = new ArrayDict<int, GameObject>();
        protected static TackRopeCreatorWindowInformation m_windowInformation = new TackRopeCreatorWindowInformation();
        bool _needFillSelected = true;
        bool _needRecreateList = true;
        protected Vector2 _panelScroll = Vector2.zero;

        #endregion

        #region Init

        [MenuItem("KILT/Create/Rope/TackRope")]
        static void Init()
        {
            TackRopeCreatorWindow v_window = EditorWindow.GetWindow(typeof(TackRopeCreatorWindow)) as TackRopeCreatorWindow;
            if (v_window != null)
                v_window.Refresh(true);
        }

        #endregion

        #region Unity Functions

        protected virtual void OnFocus()
        {
            FillSelectedObjects();
        }

        protected virtual void Update()
        {
            if (SceneView.focusedWindow != this)
                FillSelectedObjects();
        }

        protected override void OnGUI()
        {
            _panelScroll = EditorGUILayout.BeginScrollView(_panelScroll);
            if (m_windowInformation == null)
                m_windowInformation = new TackRopeCreatorWindowInformation();

            bool v_refreshClicked = InspectorUtils.DrawButton("Refresh Informations", Color.cyan);

            if (v_refreshClicked)
                Refresh();

            if (_needFillSelected)
                FillSelectedObjects();

            if (_needRecreateList)
                RecreateList(true);

            if (m_windowInformation.Ropes != null)
            {
                m_windowInformation.RopeParent = (Transform)EditorGUILayout.ObjectField("Rope Parent", m_windowInformation.RopeParent, typeof(Transform), true);
                m_windowInformation.SelectedRopeIndex = EditorGUILayout.Popup("Ropes", m_windowInformation.SelectedRopeIndex, m_windowInformation.Ropes.GetStringList().ToArray());
                Rope2D v_rope = m_windowInformation.Ropes != null && m_windowInformation.SelectedRopeIndex >= 0 && m_windowInformation.Ropes.Count > m_windowInformation.SelectedRopeIndex ? m_windowInformation.Ropes[m_windowInformation.SelectedRopeIndex] : null;
                if (m_windowInformation.Rope != v_rope)
                {
                    m_windowInformation.Rope = v_rope as TackRope;
                    if (v_rope != null)
                    {
                        m_windowInformation.RopeDepth = v_rope.RopeDepth;
                        m_windowInformation.RopeSortingLayerName = v_rope.RopeSortingLayerName;
                        m_windowInformation.CustomPrefabNode = v_rope.CustomNodePrefab;
                    }
                }

                bool v_pickSelected = EditorGUILayout.Toggle("Use Selected Objects in Scene", m_windowInformation.PickSelectedObjects);
                if (m_selectedObjects.Count > 2)
                    m_windowInformation.PlugLastWithFirst = EditorGUILayout.Toggle("Plug Last With First", m_windowInformation.PlugLastWithFirst);

                if (m_windowInformation.PickSelectedObjects != v_pickSelected)
                {
                    m_windowInformation.PickSelectedObjects = v_pickSelected;
                    FillSelectedObjects();
                    RecreateList(true);
                }
                DrawList();

                InspectorUtils.DrawTitleText("Rope Properties", new Color(1, 0.1f, 0.7f));

                //Sorting Layer
                string[] v_sortingLayers = GetSortingLayerNames();
                int v_sortingIndex = EditorGUILayout.Popup("Rope Sorting Layer", GetObjectIndexInArrayOrStrings(v_sortingLayers, m_windowInformation.RopeSortingLayerName), v_sortingLayers);
                m_windowInformation.RopeSortingLayerName = v_sortingLayers.Length > v_sortingIndex && v_sortingIndex >= 0 ? v_sortingLayers[v_sortingIndex] : "Default";

                m_windowInformation.RopeDepth = EditorGUILayout.IntField("Rope Z Order", m_windowInformation.RopeDepth);
                m_windowInformation.CustomPrefabNode = EditorGUILayout.ObjectField("Custom Node Prefab", m_windowInformation.CustomPrefabNode, typeof(GameObject), false) as GameObject;
                m_windowInformation.AutomaticCalculateNodes = EditorGUILayout.Toggle("Automatic Calculate Amount of Nodes", m_windowInformation.AutomaticCalculateNodes);
                if (!m_windowInformation.AutomaticCalculateNodes)
                    m_windowInformation.AmountOfNodes = EditorGUILayout.IntField("Amount of Nodes", m_windowInformation.AmountOfNodes);
                m_windowInformation.UsePrefabRopeScale = EditorGUILayout.Toggle("Use Prefab Rope Scale", m_windowInformation.UsePrefabRopeScale);
                InspectorUtils.DrawTitleText("", new Color(1, 0.1f, 0.7f));
                GUILayout.Space(10);

                if (m_selectedObjects.Count >= 2)
                {
                    //Final Buttons
                    bool v_recreateClicked = false;
                    bool v_deletePreviousClicked = false;
                    if (m_currentCreatedRopes.Count > 0)
                    {
                        EditorGUILayout.BeginHorizontal();
                        v_recreateClicked = InspectorUtils.DrawButton("Recreate Previus Ropes", new Color(0.4f, 0.7f, 1f));
                        v_deletePreviousClicked = InspectorUtils.DrawButton("Delete Previus Ropes", new Color(1f, 0.7f, 0.1f));
                        EditorGUILayout.EndHorizontal();
                    }
                    bool v_buttonClicked = InspectorUtils.DrawButton("Create New Rope", Color.green);

                    if (v_recreateClicked)
                        RecreateRopeClicked();
                    if (v_deletePreviousClicked)
                        DeleteRopeClicked();
                    else if (v_buttonClicked)
                        CreateRopeClicked();
                }
            }
            EditorGUILayout.EndScrollView();

            if (GUI.changed)
                EditorUtility.SetDirty(this);
        }

        #endregion

        #region Draw Selection List

        protected void RecreateList(bool p_force = false)
        {
            if (_reordList == null || p_force || _needRecreateList)
            {
                _needRecreateList = false;
                if (m_windowInformation != null && !m_windowInformation.PickSelectedObjects)
                    _reordList = new UnityEditorInternal.ReorderableList(new List<KVPair<int, GameObject>>(m_selectedObjects.ToArray()), typeof(KVPair<int, GameObject>), true, true, true, true);
                else
                    _reordList = new UnityEditorInternal.ReorderableList(new List<KVPair<int, GameObject>>(m_selectedObjects.ToArray()), typeof(KVPair<int, GameObject>), true, true, false, false);
                _reordList.onReorderCallback = HandleOnElementReordered;
                _reordList.drawElementCallback = HandleOnDrawElement;
                _reordList.drawHeaderCallback = HandleOnDrawTitle;
                _reordList.onAddCallback = HandleOnAdd;
                _reordList.onRemoveCallback = HandleOnRemove;
            }
        }

        UnityEditorInternal.ReorderableList _reordList = null;
        protected virtual void DrawList()
        {
            RecreateList();
            _reordList.DoLayoutList();
        }

        protected void FillSelectedWithList(IList p_list, bool p_reapplySelection, bool p_acceptNulls = false)
        {
            m_selectedObjects = new ArrayDict<int, GameObject>();
            List<Object> v_selectedObject = new List<Object>();
            if (p_list != null)
            {
                for (int i = 0; i < p_list.Count; i++)
                {
                    KVPair<int, GameObject> v_object = p_list[i] as KVPair<int, GameObject>;
                    if (v_object != null)
                    {
                        if ((v_object.Value != null || p_acceptNulls))
                            v_selectedObject.Add(v_object.Value);
                        m_selectedObjects.Add(v_object);
                    }
                }
                foreach (Rope2D v_rope in m_currentCreatedRopes)
                {
                    if (v_rope != null && v_rope.gameObject != null)
                        v_selectedObject.Add(v_rope.gameObject);
                }
                if (p_reapplySelection)
                    Selection.objects = v_selectedObject.ToArray();
            }
        }

        protected void HandleOnAdd(UnityEditorInternal.ReorderableList p_list)
        {
            List<KVPair<int, GameObject>> v_clonedList = p_list.list is List<KVPair<int, GameObject>> ? (p_list.list as List<KVPair<int, GameObject>>).CloneList() : new List<KVPair<int, GameObject>>();
            v_clonedList.Add(new KVPair<int, GameObject>());
            FillSelectedWithList(v_clonedList, false, true);
            _needRecreateList = true;
        }

        protected void HandleOnRemove(UnityEditorInternal.ReorderableList p_list)
        {
            List<KVPair<int, GameObject>> v_clonedList = p_list.list is List<KVPair<int, GameObject>> ? (p_list.list as List<KVPair<int, GameObject>>).CloneList() : new List<KVPair<int, GameObject>>();
            try
            {
                int v_index = p_list.index >= 0 && p_list.index < v_clonedList.Count ? p_list.index : v_clonedList.Count - 1;
                v_clonedList.RemoveAt(v_index);
            }
            catch { }
            FillSelectedWithList(v_clonedList, false, true);
            _needRecreateList = true;
        }

        protected void HandleOnDrawTitle(Rect p_rect)
        {
            EditorGUI.LabelField(p_rect, "Nodes");
        }

        protected void HandleOnDrawElement(Rect p_rect, int p_index, bool p_isActive, bool p_isFocused)
        {
            Color v_oldColor = GUI.backgroundColor;
            KVPair<int, GameObject> v_currentObject = m_selectedObjects[p_index];
            if (v_currentObject != null)
            {
                Rect v_firstRect = new Rect(p_rect.x, p_rect.y, p_rect.width - 60, p_rect.height);
                Rect v_secondRect = new Rect(p_rect.x + v_firstRect.width, p_rect.y, p_rect.width - v_firstRect.width, p_rect.height);
                GUI.enabled = m_windowInformation != null && !m_windowInformation.PickSelectedObjects;
                if (v_currentObject.Value == null)
                    GUI.backgroundColor = Color.red;
                GameObject v_newGameObject = EditorGUI.ObjectField(v_firstRect, v_currentObject.Value, typeof(GameObject), true) as GameObject;
                GUI.backgroundColor = v_oldColor;
                GUI.enabled = true;
                if (v_newGameObject != v_currentObject.Value)
                {
                    v_currentObject.Value = v_newGameObject;
                    _needFillSelected = true;
                }
                //Draw Tack Selector
                if (v_currentObject != null)
                {
                    TacksInventory v_controller = v_currentObject.Value != null ? v_currentObject.Value.GetNonMarkedComponentInChildren<TacksInventory>() : null;
                    if (v_controller != null)
                        v_controller.FillWithChildrenTacks(true);
                    if (v_controller != null && v_controller.TacksInObject.Count > 0)
                    {
                        GUI.backgroundColor = Color.green;
                        v_currentObject.Key = EditorGUI.Popup(v_secondRect, v_currentObject.Key, v_controller.TacksInObject.GetStringList().ToArray());
                    }
                    else
                    {
                        GUI.enabled = false;
                        //if(v_currentObject.Value == null)
                        GUI.backgroundColor = Color.red;
                        v_currentObject.Key = EditorGUI.Popup(v_secondRect, v_currentObject.Key, new string[0]);
                        GUI.enabled = true;
                    }
                    GUI.backgroundColor = v_oldColor;
                }
            }

        }

        protected void HandleOnElementReordered(UnityEditorInternal.ReorderableList p_list)
        {
            FillSelectedWithList(p_list.list, (m_windowInformation != null && m_windowInformation.PickSelectedObjects));
        }

        #endregion

        #region Helper Functions

        protected override void Refresh(bool p_force = true)
        {
            if (p_force || m_windowInformation.Ropes != null)
            {
                Rope2D.CleanUselessRopes();
                GameObject v_parentTransform = GameObject.Find("RopesContainer");
                if (v_parentTransform != null)
                    m_windowInformation.RopeParent = v_parentTransform.transform;
                List<TackRope> v_tackList = new List<TackRope>(RopeEditorInternalUtils.GetAssetsOfType<TackRope>(".prefab"));
                m_windowInformation.SelectedRopeIndex = 0;
                m_windowInformation.Ropes = new List<Rope2D>();
                foreach (TackRope v_tackRope in v_tackList)
                {
                    if (v_tackRope != null)
                        m_windowInformation.Ropes.Add(v_tackRope);
                }
                if (p_force && m_selectedObjects != null && m_windowInformation.PickSelectedObjects)
                    m_selectedObjects.Clear();
                FillSelectedObjects();
            }
        }

        protected virtual void FillSelectedObjects()
        {
            _needFillSelected = false;
            if (m_currentCreatedRopes == null)
                m_currentCreatedRopes = new List<Rope2D>();
            else
                m_currentCreatedRopes.Clear();

            if (m_selectedObjects == null)
                m_selectedObjects = new ArrayDict<int, GameObject>();

            if (m_windowInformation.PickSelectedObjects)
            {
                List<Transform> v_transforms = new List<Transform>(Selection.transforms);
                for (int i = 0; i < m_selectedObjects.Count; i++)
                {
                    GameObject v_object = m_selectedObjects[i] != null ? m_selectedObjects[i].Value : null;
                    if (v_object != null && !v_transforms.Contains(v_object.transform))
                    {
                        m_selectedObjects[i] = null;
                    }
                }
                m_selectedObjects.RemoveNulls();
                m_selectedObjects.RemovePairsWithNullValues();
                foreach (Transform v_transform in v_transforms)
                {
                    if (v_transform != null)
                    {
                        Rope2D v_rope = v_transform.GetComponent<Rope2D>();
                        if (v_rope == null)
                        {
                            Camera v_camera = v_transform.GetNonMarkedComponentInChildren<Camera>();
                            if (v_camera == null && !m_selectedObjects.ContainsValue(v_transform.gameObject))
                            {
                                m_selectedObjects.Add(0, v_transform.gameObject);
                            }
                        }
                        else
                            m_currentCreatedRopes.AddChecking(v_rope);
                    }
                }
                RecreateList(true);
            }
        }

        protected virtual void DeleteRopeClicked()
        {
            foreach (Rope2D v_rope in m_currentCreatedRopes)
            {
                if (v_rope != null)
                    DestroyUtils.Destroy(v_rope.gameObject);
            }
        }

        protected virtual void RecreateRopeClicked()
        {
            DeleteRopeClicked();
            CreateRopeClicked();
        }

        protected override void CreateRopeClicked()
        {
            if (m_windowInformation != null)
            {
                List<Object> v_objectsSelected = new List<Object>();
                if (m_selectedObjects.Count > 2 && m_windowInformation.PlugLastWithFirst)
                {
                    m_selectedObjects.Add(m_selectedObjects[0]);
                }
                for (int i = 1; i < m_selectedObjects.Count; i++)
                {
                    m_currentCreatedRopes.AddChecking(CreateRope(m_selectedObjects[i - 1].Value, m_selectedObjects[i].Value, m_selectedObjects[i - 1].Key, m_selectedObjects[i].Key));
                }
                Rope2D.CleanUselessRopes();

                //Reset Selection
                foreach (Rope2D v_newRope in m_currentCreatedRopes)
                {
                    if (v_newRope != null)
                        v_objectsSelected.Add(v_newRope.gameObject);
                }
                foreach (KVPair<int, GameObject> v_object in m_selectedObjects)
                {
                    if (v_object != null && v_object.Value != null)
                        v_objectsSelected.AddChecking(v_object.Value);
                }
                Selection.objects = v_objectsSelected.ToArray();
            }
        }

        protected virtual Rope2D CreateRope(GameObject p_objectA, GameObject p_objectB, int p_indexA = 0, int p_indexB = 0)
        {
            TackRope v_returnedRope = null;
            if (m_windowInformation.Rope != null && p_objectA != null && p_objectB != null)
            {
                GameObject v_ropeInSceneObject = PrefabUtility.InstantiatePrefab(m_windowInformation.Rope.gameObject) as GameObject;
                v_ropeInSceneObject.name = m_windowInformation.Rope.name + "[(" + p_objectA.name.Replace("(Selected)", "") + ") to (" + p_objectB.name.Replace("(Selected)", "") + ")]";
                v_ropeInSceneObject.transform.parent = m_windowInformation.RopeParent;
                if (m_windowInformation.UsePrefabRopeScale)
                    v_ropeInSceneObject.transform.localScale = m_windowInformation.Rope.transform.localScale;

                TackRope v_ropeInScene = v_ropeInSceneObject.GetComponent<TackRope>();
                v_ropeInScene.ObjectA = p_objectA;
                v_ropeInScene.ObjectB = p_objectB;
                v_ropeInScene.TackAIndex = p_indexA;
                v_ropeInScene.TackBIndex = p_indexB;
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
    }

    #region Helper Classes

    public class TackRopeCreatorWindowInformation : RopeCreatorWindowInformation
    {
        #region Private Variables

        [SerializeField]
        int m_tackAIndex = 0;
        [SerializeField]
        int m_tackBIndex = 0;
        [SerializeField]
        bool m_pickSelectedObjects = true;
        [SerializeField]
        bool m_plugLastWithFirst = true;

        #endregion

        #region Public Properties

        public new TackRope Rope { get { return (base.Rope as TackRope); } set { base.Rope = value; } }
        public int TackAIndex { get { return m_tackAIndex; } set { m_tackAIndex = value; } }
        public int TackBIndex { get { return m_tackBIndex; } set { m_tackBIndex = value; } }
        public bool PickSelectedObjects { get { return m_pickSelectedObjects; } set { m_pickSelectedObjects = value; } }
        public bool PlugLastWithFirst { get { return m_plugLastWithFirst; } set { m_plugLastWithFirst = value; } }

        #endregion
    }

    #endregion
}
