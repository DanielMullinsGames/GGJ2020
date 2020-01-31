using UnityEditor;
using UnityEngine;

namespace Peppermint.DataBinding.Editor
{
    public class CodeBuilderEditorWindow : EditorWindow
    {
        private enum Mode
        {
            Property,
            ImplicitConverter,
            Aot,
        }

        private Mode currentMode;
        private GUIContent[] modeToggles;
        private GUIStyle lineStyle;
        private GUIStyle toggleMixedStyle;
        private Vector2 scrollPosition;
        private Vector2 propertyScrollPosition;
        private Vector2 implicitOperatorScrollPosition;
        private bool groupToggle;

        private PropertyCodeBuilder propertyCodeBuilder;
        private AotCodeBuilder aotCodeBuilder;
        private ImplicitConverterCodeBuilder implicitConverterCodeBuilder;

        private const int LabelWidth = 160;

        [MenuItem("Tools/Data Binding/Code Builder")]
        static void Init()
        {
            var window = EditorWindow.GetWindow<CodeBuilderEditorWindow>("Code Builder");
            window.Show();
        }

        void OnEnable()
        {
            Setup();
        }

        void OnDisable()
        {
            propertyCodeBuilder = null;
            aotCodeBuilder = null;
        }

        void Setup()
        {
            // get root directory
            var rootDirectory = EditorHelper.GetFrameworkRootDirectory(this);
            if (rootDirectory == null)
            {
                Debug.LogWarning("Invalid directory structure");
                return;
            }

            // create builder
            aotCodeBuilder = new AotCodeBuilder(rootDirectory);
            aotCodeBuilder.LoadSettings();

            propertyCodeBuilder = new PropertyCodeBuilder(rootDirectory);
            propertyCodeBuilder.LoadSettings();

            implicitConverterCodeBuilder = new ImplicitConverterCodeBuilder(rootDirectory);
            implicitConverterCodeBuilder.LoadSettings();
        }

        void OnGUI()
        {
            if (modeToggles == null)
            {
                modeToggles = new GUIContent[]
                {
                    new GUIContent("Bindable Property"),
                    new GUIContent("Implicit Converter"),
                    new GUIContent("AOT"),
                };
            }

            if (lineStyle == null)
            {
                lineStyle = new GUIStyle(GUI.skin.box);
                lineStyle.stretchWidth = true;
                lineStyle.fixedHeight = 2;
            }

            if (toggleMixedStyle == null)
            {
                toggleMixedStyle = new GUIStyle("ToggleMixed");
            }
            
            EditorGUILayout.Space();
            currentMode = (Mode)GUILayout.Toolbar((int)currentMode, modeToggles, "LargeButton");
            EditorGUILayout.Space();

            using (var view = new EditorGUILayout.ScrollViewScope(scrollPosition))
            {
                scrollPosition = view.scrollPosition;

                switch (currentMode)
                {
                    case Mode.Property:
                        DrawPropertyCodeBuilder();
                        break;

                    case Mode.Aot:
                        DrawAotCodeBuilder();
                        break;

                    case Mode.ImplicitConverter:
                        DrawImplicitConverterCodeBuilder();
                        break;
                }
            }
        }


        #region ImplicitConverter

        private void DrawImplicitConverterCodeBuilder()
        {
            EditorGUI.BeginChangeCheck();
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Output Directory", GUILayout.Width(LabelWidth));
                    implicitConverterCodeBuilder.settings.outputDirectory = EditorGUILayout.TextField(implicitConverterCodeBuilder.settings.outputDirectory);
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Code Template", GUILayout.Width(LabelWidth));
                    implicitConverterCodeBuilder.settings.templatePath = EditorGUILayout.TextField(implicitConverterCodeBuilder.settings.templatePath);
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(implicitConverterCodeBuilder.settings);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Commands", GUILayout.Width(LabelWidth));

                if (GUILayout.Button("Load Type"))
                {
                    implicitConverterCodeBuilder.LoadTypeList();
                }
                if (GUILayout.Button("Generate Code"))
                {
                    implicitConverterCodeBuilder.GenerateCode();
                }
                if (GUILayout.Button("Open Output Folder"))
                {
                    OpenOutputFolder(implicitConverterCodeBuilder.settings.outputDirectory, implicitConverterCodeBuilder.lastOutputPath);
                }
            }

            DrawImplicitOperatorTypeList();
        }

        private void DrawImplicitOperatorTypeList()
        {
            if (implicitConverterCodeBuilder.buildArgs == null)
            {
                return;
            }

            EditorGUILayout.Space();

            // draw line separator
            EditorGUILayout.LabelField("", lineStyle);

            using (var view = new EditorGUILayout.ScrollViewScope(implicitOperatorScrollPosition))
            {
                implicitOperatorScrollPosition = view.scrollPosition;

                // draw list header
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Index", GUILayout.Width(LabelWidth));

                    // member name
                    EditorGUILayout.LabelField("Type");
                }

                int index = 0;
                foreach (var item in implicitConverterCodeBuilder.buildArgs.itemList)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(index.ToString(), GUILayout.Width(LabelWidth));

                        // member name
                        EditorGUILayout.LabelField(item.type.FullName);
                    }

                    index++;
                }
            }
        }

        #endregion

        #region Property

        private void DrawPropertyCodeBuilder()
        {
            EditorGUI.BeginChangeCheck();
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Output Directory", GUILayout.Width(LabelWidth));
                    propertyCodeBuilder.settings.outputDirectory = EditorGUILayout.TextField(propertyCodeBuilder.settings.outputDirectory);
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Code Template", GUILayout.Width(LabelWidth));
                    propertyCodeBuilder.settings.templatePath = EditorGUILayout.TextField(propertyCodeBuilder.settings.templatePath);
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(propertyCodeBuilder.settings);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Class Type Name", GUILayout.Width(LabelWidth));
                propertyCodeBuilder.typeName = EditorGUILayout.TextField(propertyCodeBuilder.typeName);
            }

            EditorGUI.BeginChangeCheck();
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Include Public", GUILayout.Width(LabelWidth));
                    propertyCodeBuilder.includePublic = EditorGUILayout.Toggle(propertyCodeBuilder.includePublic, GUILayout.Width(20));
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Include Property", GUILayout.Width(LabelWidth));
                    propertyCodeBuilder.includeProperty = EditorGUILayout.Toggle(propertyCodeBuilder.includeProperty, GUILayout.Width(20));
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                propertyCodeBuilder.UpdateMemberList();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Commands", GUILayout.Width(LabelWidth));

                if (GUILayout.Button("Load Class"))
                {
                    propertyCodeBuilder.LoadClass();
                }
                if (GUILayout.Button("Generate Code Snippet"))
                {
                    propertyCodeBuilder.GenerateCodeSnippet();
                }
                if (GUILayout.Button("Generate Code"))
                {
                    propertyCodeBuilder.GenerateCode();
                }
                if (GUILayout.Button("Open Output Folder"))
                {
                    OpenOutputFolder(propertyCodeBuilder.settings.outputDirectory, propertyCodeBuilder.lastOutputPath);
                }
            }

            // draw member list
            DrawPropertyMemberList();
        }

        private void DrawPropertyMemberList()
        {
            if (propertyCodeBuilder.buildArgs == null)
            {
                return;
            }

            // get toggle style
            var toggleStyle = EditorStyles.toggle;

            int n = propertyCodeBuilder.buildArgs.memberList.Count;
            if (n > 0)
            {
                int result = 0;
                foreach (var item in propertyCodeBuilder.buildArgs.memberList)
                {
                    int v = item.enabled ? 1 : 0;
                    result += v;
                }

                if (result == 0)
                {
                    groupToggle = false;
                }
                else if (result == n)
                {
                    groupToggle = true;
                }
                else
                {
                    // use mixed style
                    toggleStyle = toggleMixedStyle;
                }
            }
            
            EditorGUILayout.Space();

            // draw line separator
            EditorGUILayout.LabelField("", lineStyle);

            using (var view = new EditorGUILayout.ScrollViewScope(propertyScrollPosition))
            {
                propertyScrollPosition = view.scrollPosition;

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Class Information", GUILayout.Width(LabelWidth));
                    EditorGUILayout.LabelField(propertyCodeBuilder.classInfo);
                }
                
                EditorGUILayout.LabelField("Class Member List");

                // draw list header
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Index", GUILayout.Width(LabelWidth));

                    // member name
                    EditorGUILayout.LabelField("Member Name", GUILayout.Width(300));

                    // output name
                    EditorGUILayout.LabelField("Generated Property Name", GUILayout.Width(300));

                    GUILayout.FlexibleSpace();

                    // draw group toggle
                    var flag = EditorGUILayout.Toggle(groupToggle, toggleStyle, GUILayout.Width(30));
                    if (flag != groupToggle)
                    {
                        // set flag to all items
                        propertyCodeBuilder.buildArgs.memberList.ForEach(x => x.enabled = flag);

                        groupToggle = flag;
                    }
                }

                int index = 0;
                foreach (var item in propertyCodeBuilder.buildArgs.memberList)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(index.ToString(), GUILayout.Width(LabelWidth));

                        // member name
                        EditorGUILayout.LabelField(item.name, GUILayout.Width(300));

                        // output name
                        item.propertyName = EditorGUILayout.TextField(item.propertyName, GUILayout.Width(300));

                        GUILayout.FlexibleSpace();

                        // enabled
                        item.enabled = EditorGUILayout.Toggle(item.enabled, GUILayout.Width(30));
                    }

                    index++;
                }
            }
        }

        #endregion

        #region AOT

        private void DrawAotCodeBuilder()
        {
            EditorGUI.BeginChangeCheck();
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Output Directory", GUILayout.Width(LabelWidth));
                    aotCodeBuilder.settings.outputDirectory = EditorGUILayout.TextField(aotCodeBuilder.settings.outputDirectory);
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Code Template", GUILayout.Width(LabelWidth));
                    aotCodeBuilder.settings.templatePath = EditorGUILayout.TextField(aotCodeBuilder.settings.templatePath);
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Class Type Name", GUILayout.Width(LabelWidth));
                    aotCodeBuilder.settings.className = EditorGUILayout.TextField(aotCodeBuilder.settings.className);
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(aotCodeBuilder.settings);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Commands", GUILayout.Width(LabelWidth));

                if (GUILayout.Button("Generate Code"))
                {
                    aotCodeBuilder.GenerateCode();
                }
                if (GUILayout.Button("Open Output Folder"))
                {
                    OpenOutputFolder(aotCodeBuilder.settings.outputDirectory, aotCodeBuilder.lastOutputPath);
                }
            }
        }

        #endregion

        private void OpenOutputFolder(string outputDirectory, string lastOutputPath)
        {
            var projectPath = EditorHelper.GetProjectPath();

            // set output directory
            var fullPath = string.Format("{0}/{1}", projectPath, outputDirectory);
            if (!string.IsNullOrEmpty(lastOutputPath))
            {
                // use last output path
                fullPath = string.Format("{0}/{1}", projectPath, lastOutputPath);
            }

            EditorUtility.RevealInFinder(fullPath);
        }

    }
}