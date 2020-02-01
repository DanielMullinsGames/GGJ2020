using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Kilt;
using Kilt.EasyRopes2D;
using Kilt.Extensions;

namespace KiltEditor.EasyRopes2D
{

    public class BalloonPluggerWindow : EditorWindow
    {
        static BalloonPluggerWindowInformation m_windowInformation = new BalloonPluggerWindowInformation();

        [MenuItem("KILT/Create/Balloon")]
        static void Init()
        {
            BalloonPluggerWindow v_window = EditorWindow.GetWindow(typeof(BalloonPluggerWindow)) as BalloonPluggerWindow;
            if (v_window != null)
                v_window.Refresh(true);
        }

        protected virtual void Refresh(bool p_force = true)
        {
            if (p_force || (m_windowInformation.Ballons != null || m_windowInformation.Ropes != null))
            {
                m_windowInformation.Ballons = new List<BalloonProperty>(RopeEditorInternalUtils.GetAssetsOfType<BalloonProperty>(".prefab"));
                m_windowInformation.Ropes = new List<TackRope>(RopeEditorInternalUtils.GetAssetsOfType<TackRope>(".prefab"));
                m_windowInformation.SelectedRopeIndex = 0;
                m_windowInformation.SelectedBallonIndex = 0;
            }
        }

        protected virtual void OnGUI()
        {
            if (m_windowInformation == null)
                m_windowInformation = new BalloonPluggerWindowInformation();

            EditorGUILayout.BeginHorizontal();

            bool v_refreshClicked = GUILayout.Button("Refresh Informations");

            EditorGUILayout.EndHorizontal();

            if (v_refreshClicked)
                Refresh();

            if (m_windowInformation.Ballons != null)
            {
                EditorGUILayout.BeginHorizontal();

                GUILayout.Label("Ballon");
                m_windowInformation.SelectedBallonIndex = EditorGUILayout.Popup(m_windowInformation.SelectedBallonIndex, m_windowInformation.Ballons.GetStringList().ToArray());

                EditorGUILayout.EndHorizontal();
            }

            if (m_windowInformation.Ropes != null)
            {
                EditorGUILayout.BeginHorizontal();

                GUILayout.Label("Ropes");
                m_windowInformation.SelectedRopeIndex = EditorGUILayout.Popup(m_windowInformation.SelectedRopeIndex, m_windowInformation.Ropes.GetStringList().ToArray());

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("Plug In Selected Object In Scene");

            m_windowInformation.PlugInSelectedObject = EditorGUILayout.BeginToggleGroup("", m_windowInformation.PlugInSelectedObject);
            EditorGUILayout.EndToggleGroup();

            EditorGUILayout.EndHorizontal();

            if (!m_windowInformation.PlugInSelectedObject)
            {
                EditorGUILayout.BeginHorizontal();

                m_windowInformation.ObjectInSceneToPlug = (GameObject)EditorGUILayout.ObjectField("Object To Plug", m_windowInformation.ObjectInSceneToPlug, typeof(GameObject), true);

                EditorGUILayout.EndHorizontal();
            }
            else
            {
                Transform v_transform = Selection.activeTransform;
                if (v_transform != null)
                    m_windowInformation.ObjectInSceneToPlug = v_transform.gameObject;
            }

            BalloonProperty v_ballonPrefabComponent = m_windowInformation.Ballons != null && m_windowInformation.SelectedBallonIndex >= 0 && m_windowInformation.Ballons.Count > m_windowInformation.SelectedBallonIndex ? m_windowInformation.Ballons[m_windowInformation.SelectedBallonIndex] : null;
            TackRope v_ropePrefabComponent = m_windowInformation.Ropes != null && m_windowInformation.SelectedRopeIndex >= 0 && m_windowInformation.Ropes.Count > m_windowInformation.SelectedRopeIndex ? m_windowInformation.Ropes[m_windowInformation.SelectedRopeIndex] : null;

            if (m_windowInformation.ObjectInSceneToPlug != null && v_ballonPrefabComponent != null && v_ropePrefabComponent != null)
            {
                EditorGUILayout.BeginHorizontal();

                bool v_buttonClicked = GUILayout.Button("Plug Ballon");

                EditorGUILayout.EndHorizontal();

                if (v_buttonClicked)
                    PlugBallon();
            }
        }

        protected virtual void PlugBallon()
        {
            Rope2D.CleanUselessRopes();
            BalloonProperty v_ballonPrefabComponent = m_windowInformation.Ballons != null && m_windowInformation.SelectedBallonIndex >= 0 && m_windowInformation.Ballons.Count > m_windowInformation.SelectedBallonIndex ? m_windowInformation.Ballons[m_windowInformation.SelectedBallonIndex] : null;
            TackRope v_ropePrefabComponent = m_windowInformation.Ropes != null && m_windowInformation.SelectedRopeIndex >= 0 && m_windowInformation.Ropes.Count > m_windowInformation.SelectedRopeIndex ? m_windowInformation.Ropes[m_windowInformation.SelectedRopeIndex] : null;
            if (v_ballonPrefabComponent != null && v_ropePrefabComponent != null && m_windowInformation.ObjectInSceneToPlug)
                BalloonUtils.PlugBallonToObject(m_windowInformation.ObjectInSceneToPlug, v_ballonPrefabComponent, v_ropePrefabComponent, new Vector2(0.4f, 1f), new Vector2(-0.4f, 0.8f), true, true);
        }
    }

    public class BalloonPluggerWindowInformation
    {
        #region Private Variables

        [SerializeField]
        List<BalloonProperty> m_ballons = null;
        [SerializeField]
        List<TackRope> m_ropes = null;
        [SerializeField]
        bool m_plugInSelectedObject = true;
        [SerializeField]
        GameObject m_objectInSceneToPlug = null;
        [SerializeField]
        int m_selectedBallonIndex = 0;
        [SerializeField]
        int m_selectedRopeIndex = 0;

        #endregion

        #region Public Properties

        public List<BalloonProperty> Ballons { get { return m_ballons; } set { m_ballons = value; } }
        public List<TackRope> Ropes { get { return m_ropes; } set { m_ropes = value; } }
        public bool PlugInSelectedObject { get { return m_plugInSelectedObject; } set { m_plugInSelectedObject = value; } }
        public GameObject ObjectInSceneToPlug { get { return m_objectInSceneToPlug; } set { m_objectInSceneToPlug = value; } }
        public int SelectedBallonIndex { get { return m_selectedBallonIndex; } set { m_selectedBallonIndex = value; } }
        public int SelectedRopeIndex { get { return m_selectedRopeIndex; } set { m_selectedRopeIndex = value; } }

        #endregion
    }
}
