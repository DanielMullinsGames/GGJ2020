using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Peppermint.DataBinding.Editor
{
    /// <summary>
    /// SpriteSet Builder is an editor tool which can build the SpriteSet from specified directory.
    /// </summary>
    public class SpriteSetBuilderEditorWindow : EditorWindow
    {
        private SpriteSetBuilderSettings settings;
        private List<SpriteSetBuilderSettings.Item> pendingList;
        private Vector2 scrollPosition;

        private const int LabelWidth = 160;

        [MenuItem("Tools/Data Binding/SpriteSet Builder")]
        static void Init()
        {
            var window = EditorWindow.GetWindow<SpriteSetBuilderEditorWindow>("Builder");
            window.Show();
        }

        void OnEnable()
        {
            Setup();
        }

        void OnGUI()
        {
            if (settings == null)
            {
                return;
            }

            EditorGUILayout.LabelField("SpriteSet Builder");

            EditorGUI.BeginChangeCheck();
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Output Directory", GUILayout.Width(LabelWidth));
                    settings.outputDirectory = EditorGUILayout.TextField(settings.outputDirectory);
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Commands", GUILayout.Width(LabelWidth));
                    if (GUILayout.Button("Add Item", GUILayout.Width(200)))
                    {
                        AddItem();
                    }
                    if (GUILayout.Button("Build All", GUILayout.Width(200)))
                    {
                        BuildAll();
                    }
                }

                // draw item list
                DrawList();
            }
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(settings);
            }
        }

        private void DrawList()
        {
            EditorGUILayout.LabelField("Item List", GUILayout.Width(LabelWidth));

            using (var view = new EditorGUILayout.ScrollViewScope(scrollPosition))
            {
                scrollPosition = view.scrollPosition;

                EditorGUI.indentLevel++;

                int index = 0;
                foreach (var item in settings.itemList)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        // show index
                        EditorGUILayout.LabelField(index.ToString(), GUILayout.Width(30));

                        EditorGUILayout.LabelField("Sprite Directory", GUILayout.Width(120));
                        var newDirectory = EditorGUILayout.TextField(item.directory);
                        if (newDirectory != item.directory)
                        {
                            // format path
                            item.directory = EditorHelper.FormatPath(newDirectory);
                        }

                        EditorGUILayout.LabelField("Recursive", GUILayout.Width(80));
                        item.recursive = EditorGUILayout.Toggle(item.recursive, GUILayout.Width(40));

                        string info = null;
                        if (item.spriteSet != null)
                        {
                            info = string.Format("Sprite Count {0}", item.spriteSet.spriteList.Count);
                        }

                        EditorGUILayout.LabelField(info, GUILayout.Width(120));

                        if (GUILayout.Button("Build", GUILayout.Width(100)))
                        {
                            Build(item);
                        }

                        using (new EditorGUI.DisabledGroupScope(item.spriteSet == null))
                        {
                            if (GUILayout.Button("Select", GUILayout.Width(100)))
                            {
                                SelectSpriteSet(item);
                            }
                        }

                        if (GUILayout.Button("Remove Item", GUILayout.Width(100)))
                        {
                            // mark for delete
                            pendingList.Add(item);
                        }

                    }

                    index++;
                }

                EditorGUI.indentLevel--;
            }

            // remove pending
            if (pendingList.Count > 0)
            {
                foreach (var item in pendingList)
                {
                    settings.itemList.Remove(item);
                }

                pendingList.Clear();
            }
        }

        void Setup()
        {
            pendingList = new List<SpriteSetBuilderSettings.Item>();

            LoadSettings();
        }

        private void LoadSettings()
        {
            // get root directory
            var rootDirectory = EditorHelper.GetFrameworkRootDirectory(this);
            if (rootDirectory == null)
            {
                Debug.LogWarning("Invalid directory structure");
                return;
            }

            var path = string.Format("{0}/Settings/{1}.asset", rootDirectory, typeof(SpriteSetBuilderSettings).Name);

            // load settings
            settings = AssetDatabase.LoadAssetAtPath<SpriteSetBuilderSettings>(path);

            if (settings == null)
            {
                // create new settings
                settings = ScriptableObject.CreateInstance<SpriteSetBuilderSettings>();

                // set default output directory
                settings.outputDirectory = "Assets/GUI/SpriteSet";

                // save it
                EditorHelper.PrepareOutputPath(path);
                AssetDatabase.CreateAsset(settings, path);
                AssetDatabase.SaveAssets();
            }
        }

        private void AddItem()
        {
            var newItem = new SpriteSetBuilderSettings.Item();
            settings.itemList.Add(newItem);
        }

        private void Build(SpriteSetBuilderSettings.Item item)
        {
            item.spriteSet = null;

            if (!AssetDatabase.IsValidFolder(item.directory))
            {
                Debug.LogWarningFormat("Invalid sprite directory {0}", item.directory);
                return;
            }

            // write
            WriteSpriteSet(item);
        }

        private void BuildAll()
        {
            foreach (var item in settings.itemList)
            {
                Build(item);
            }
        }

        private void SelectSpriteSet(SpriteSetBuilderSettings.Item item)
        {
            if (item.spriteSet != null)
            {
                Selection.activeObject = item.spriteSet;
            }
        }

        private List<Sprite> GetSpriteList(string directory, bool recursive)
        {
            // get texture path list
            var pathList = EditorHelper.GetFileList(directory, "t:texture2D", recursive);

            var spriteList = new List<Sprite>();
            foreach (var path in pathList)
            {
                // load sprite
                var sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(path).Select(x => x as Sprite).Where(x => x != null).ToArray();

                foreach (var sprite in sprites)
                {
                    // add to list
                    spriteList.Add(sprite);
                }
            }

            return spriteList;
        }

        private void WriteSpriteSet(SpriteSetBuilderSettings.Item item)
        {
            // get sprite list
            var spriteList = GetSpriteList(item.directory, item.recursive);

            // load it
            string path = string.Format("{0}/{1}.asset", settings.outputDirectory, item.Name);
            var spriteSet = AssetDatabase.LoadAssetAtPath<SpriteSet>(path);
            if (spriteSet == null)
            {
                // create new
                spriteSet = ScriptableObject.CreateInstance<SpriteSet>();

                // save it
                EditorHelper.PrepareOutputPath(path);
                AssetDatabase.CreateAsset(spriteSet, path);
            }

            // update list
            spriteSet.spriteList = spriteList;

            // mark dirty
            EditorUtility.SetDirty(spriteSet);

            // keep reference
            item.spriteSet = spriteSet;

            Debug.LogFormat("Write SpriteTable, path={0}", path);
        }

    }
}