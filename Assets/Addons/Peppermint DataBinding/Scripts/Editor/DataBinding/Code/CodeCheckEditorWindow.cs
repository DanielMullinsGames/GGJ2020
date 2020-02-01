using UnityEditor;
using UnityEngine;

namespace Peppermint.DataBinding.Editor
{
    public class CodeCheckEditorWindow : EditorWindow
    {
        [MenuItem("Tools/Data Binding/Code Check")]
        static void Init()
        {
            var window = EditorWindow.GetWindow<CodeCheckEditorWindow>("Code Check");
            window.Show();
        }

        void OnGUI()
        {
            const int LabelWidth = 160;

            EditorGUILayout.LabelField("Data Binding Code Check Tool");

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Assembly Directory", GUILayout.Width(LabelWidth));

                // show script assemblies directory
                EditorGUILayout.SelectableLabel(BindingEditorUtility.ScriptAssembliesDirectory, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Commands", GUILayout.Width(LabelWidth));

                if (GUILayout.Button("Force Recompile", GUILayout.Width(200)))
                {
                    EditorHelper.RecompileScripts();
                }

                if (GUILayout.Button("Verify Property Names", GUILayout.Width(200)))
                {
                    // call code tool
                    CodeTool.VerifyPropertyNames();

                    Debug.Log("Verify property names finished.");
                }
            }
        }
    }
}