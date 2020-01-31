using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Peppermint.DataBinding.Editor
{
    public class BindingManagerDebugEditorWindow : EditorWindow
    {
        #region BindingManager Helper

        private class ManagerInstance
        {
            public Dictionary<string, List<IDataContext>> dataContextDictionary;
            public Dictionary<string, object> sourceDictionary;

            public int GetDataContextCount()
            {
                int totalCount = 0;

                foreach (var item in dataContextDictionary)
                {
                    totalCount += item.Value.Count();
                }

                return totalCount;
            }

            public int GetBindingCount()
            {
                int totalCount = 0;

                foreach (var item in dataContextDictionary)
                {
                    totalCount += GetBindingCountInGroup(item.Key);
                }

                return totalCount;
            }

            public int GetBindingCountInGroup(string name)
            {
                List<IDataContext> list;
                if (!dataContextDictionary.TryGetValue(name, out list))
                {
                    return 0;
                }

                int totalCount = 0;

                foreach (var dc in list)
                {
                    totalCount += GetBindingCount(dc);
                }

                return totalCount;
            }

            private int GetBindingCount(IDataContext dataContext)
            {
                int count = 0;

                foreach (var item in dataContext.BindingList)
                {
                    // increase count
                    count++;

                    if (item is CollectionBinding)
                    {
                        var binding = (CollectionBinding)item;

                        count += GetCount(binding.BindingDictionary);
                    }
                    else if (item is ListDynamicBinding)
                    {
                        var binding = (ListDynamicBinding)item;

                        count += GetCount(binding.BindingDictionary);
                    }
                }

                return count;
            }

            private int GetCount(Dictionary<object, GameObject> bindingDictionary)
            {
                if (bindingDictionary.Count == 0)
                {
                    return 0;
                }

                int totalCount = 0;
                foreach (var kvp in bindingDictionary)
                {
                    var dc = kvp.Value.GetComponent<IDataContext>();

                    int internalBindingCount = GetBindingCount(dc);
                    totalCount += internalBindingCount;
                }

                return totalCount;
            }

        }

        #endregion

        private Vector2 sourceScrollPos;
        private Vector2 dataContextScrollPos;
        private bool showSource;
        private bool showDataContext;
        private GUIStyle lineStyle;

        private ManagerInstance managerInstance;

        const int LabelWidth = 200;
        const int SpaceWidth = 20;

        [MenuItem("Tools/Data Binding/BindingManager Debug")]
        static void Init()
        {
            var window = EditorWindow.GetWindow<BindingManagerDebugEditorWindow>("BindingManager Debug");
            window.Show();
        }

        void OnEnable()
        {
            InitManager();
        }

        void OnDisable()
        {
            managerInstance = null;
        }

        void OnInspectorUpdate()
        {
            if (EditorApplication.isPlaying && !EditorApplication.isPaused)
            {
                Repaint();
            }
        }

        private void InitManager()
        {
            // reset current
            managerInstance = null;

            var newInstance = new ManagerInstance();

            var bindingManager = BindingManager.Instance;
            var managerType = typeof(BindingManager);

            // get private field
            var field = managerType.GetField("sourceDictionary", BindingFlags.NonPublic | BindingFlags.Instance);
            newInstance.sourceDictionary = (Dictionary<string, object>)field.GetValue(bindingManager);

            field = managerType.GetField("dataContextDictionary", BindingFlags.NonPublic | BindingFlags.Instance);
            newInstance.dataContextDictionary = (Dictionary<string, List<IDataContext>>)field.GetValue(bindingManager);

            managerInstance = newInstance;
        }

        void OnGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.LabelField("Application is not running.");
                return;
            }

            if (managerInstance == null)
            {
                return;
            }

            if (lineStyle == null)
            {
                lineStyle = new GUIStyle(GUI.skin.box);
                lineStyle.stretchWidth = true;
                lineStyle.fixedHeight = 2;
            }

            EditorGUILayout.LabelField("BindingManager Debug");

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Source Count", GUILayout.Width(LabelWidth));
                EditorGUILayout.LabelField(string.Format("{0}", managerInstance.sourceDictionary.Count));
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("DataContext", GUILayout.Width(LabelWidth));
                EditorGUILayout.LabelField(string.Format("GroupCount={0}, Total DataContext Count={1}, Total IBinding Count={2}",
                    managerInstance.dataContextDictionary.Count, managerInstance.GetDataContextCount(), managerInstance.GetBindingCount()));
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Show Source Dictionary", GUILayout.Width(LabelWidth));
                showSource = EditorGUILayout.Toggle(showSource, GUILayout.Width(20));
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Show DataContext Dictionary", GUILayout.Width(LabelWidth));
                showDataContext = EditorGUILayout.Toggle(showDataContext, GUILayout.Width(20));
            }

            if (showSource)
            {
                DrawSource();
            }

            if (showDataContext)
            {
                DrawDataContext();
            }
        }

        private void DrawSource()
        {
            // draw line separator
            EditorGUILayout.LabelField("", lineStyle);

            EditorGUILayout.LabelField("Source Dictionary", GUILayout.Width(LabelWidth));

            using (var view = new EditorGUILayout.ScrollViewScope(sourceScrollPos))
            {
                sourceScrollPos = view.scrollPosition;

                // draw list header
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Index", GUILayout.Width(40));
                    EditorGUILayout.LabelField("Source Name", GUILayout.Width(200));
                    GUILayout.Space(SpaceWidth);

                    EditorGUILayout.LabelField("Source Type", GUILayout.MinWidth(400));
                    GUILayout.Space(SpaceWidth);

                    EditorGUILayout.LabelField("Source Instance", GUILayout.Width(200));
                }

                int index = 0;
                foreach (var item in managerInstance.sourceDictionary)
                {
                    var source = item.Value;
                    var souceType = source.GetType();

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(index.ToString(), GUILayout.Width(40));

                        // source name
                        EditorGUILayout.LabelField(item.Key, GUILayout.Width(200));
                        GUILayout.Space(SpaceWidth);

                        // source type
                        var sourceTypeInfo = string.Format("{0}", souceType);
                        EditorGUILayout.LabelField(sourceTypeInfo, GUILayout.MinWidth(400));
                        GUILayout.Space(SpaceWidth);

                        // show select button
                        var unitySource = source as UnityEngine.Object;
                        if (unitySource != null)
                        {
                            if (GUILayout.Button(unitySource.name, GUILayout.Width(200)))
                            {
                                SelectObject(unitySource);
                            }
                        }
                        else
                        {
                            var objectName = "(null)";
                            if (source != null)
                            {
                                objectName = string.Format("(Object {0:X8})", source.GetHashCode());
                            }

                            EditorGUILayout.LabelField(objectName, GUILayout.Width(200));
                        }
                    }

                    index++;
                }
            }
        }

        private void DrawDataContext()
        {
            // draw line separator
            EditorGUILayout.LabelField("", lineStyle);

            EditorGUILayout.LabelField("DataContext Dictionary", GUILayout.Width(LabelWidth));

            using (var view = new EditorGUILayout.ScrollViewScope(dataContextScrollPos))
            {
                dataContextScrollPos = view.scrollPosition;

                // draw list header
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Index", GUILayout.Width(40));
                    EditorGUILayout.LabelField("Required Source Name", GUILayout.Width(200));
                    GUILayout.Space(SpaceWidth);

                    EditorGUILayout.LabelField("DataContext Count", GUILayout.Width(140));
                    EditorGUILayout.LabelField("IBinding Count", GUILayout.Width(100));
                    EditorGUILayout.LabelField("IsBound", GUILayout.Width(60));
                    GUILayout.Space(SpaceWidth);

                    EditorGUILayout.LabelField("Source Instance", GUILayout.Width(200));
                    GUILayout.Space(SpaceWidth);

                    EditorGUILayout.LabelField("DataContext Instance List", GUILayout.MinWidth(200));
                }

                int index = 0;
                foreach (var item in managerInstance.dataContextDictionary)
                {
                    var sourceName = item.Key;
                    var list = item.Value;
                    int bindingCount = managerInstance.GetBindingCountInGroup(sourceName);

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(index.ToString(), GUILayout.Width(40));

                        // source name
                        EditorGUILayout.LabelField(item.Key, GUILayout.Width(200));
                        GUILayout.Space(SpaceWidth);

                        // DataContext info
                        object source = null;
                        managerInstance.sourceDictionary.TryGetValue(sourceName, out source);

                        EditorGUILayout.LabelField(string.Format("{0}", list.Count), GUILayout.Width(140));
                        EditorGUILayout.LabelField(string.Format("{0}", bindingCount), GUILayout.Width(100));
                        EditorGUILayout.LabelField(string.Format("{0}", source != null), GUILayout.Width(60));
                        GUILayout.Space(SpaceWidth);

                        // show select button
                        var unitySource = source as UnityEngine.Object;
                        if (unitySource != null)
                        {
                            if (GUILayout.Button(unitySource.name, GUILayout.Width(200)))
                            {
                                SelectObject(unitySource);
                            }
                        }
                        else
                        {
                            var objectName = "(null)";
                            if (source != null)
                            {
                                objectName = string.Format("(Object {0:X8})", source.GetHashCode());
                            }

                            EditorGUILayout.LabelField(objectName, GUILayout.Width(200));
                        }

                        GUILayout.Space(SpaceWidth);

                        // draw all dataContext
                        foreach (var dc in list)
                        {
                            var dcComponent = dc as DataContext;
                            if (dcComponent == null)
                            {
                                continue;
                            }

                            if (GUILayout.Button(dcComponent.name))
                            {
                                SelectObject(dcComponent);
                            }
                        }

                        GUILayout.FlexibleSpace();
                    }

                    GUILayout.Space(2f);

                    index++;
                }
            }
        }

        private void SelectObject(UnityEngine.Object source)
        {
            Selection.activeObject = source;
        }
    }
}
