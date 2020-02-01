using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;


namespace Peppermint.DataBinding.Editor
{
    public static class EditorHelper
    {
        public static string GetTransformPath(Transform from, Transform to = null)
        {
            Transform current = from;
            string path = current.name;

            while (current.parent != to)
            {
                current = current.parent;
                path = string.Format("{0}/{1}", current.name, path);
            }

            return path;
        }

        public static string FormatPath(string path)
        {
            var result = path.Replace('\\', '/');
            return result;
        }

        public static void PrepareOutputPath(string path)
        {
            string outputDirectory = Path.GetDirectoryName(path);

            // check if directory exist
            if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
            {
                // create it
                Directory.CreateDirectory(outputDirectory);
            }
        }

        public static void CheckoutFile(string assetPath)
        {
            bool providerEnabled = Provider.enabled && Provider.isActive;

            if (!providerEnabled)
            {
                // no version control
                return;
            }

            if (!File.Exists(assetPath))
            {
                // file not exist
                return;
            }

            if (Provider.GetAssetByPath(assetPath) == null)
            {
                // file is not under version control
                return;
            }

            // checkout file
            var checkoutTask = Provider.Checkout(assetPath, CheckoutMode.Both);
            checkoutTask.Wait();
        }

        public static string GetProjectPath()
        {
            // get project path
            int len = "/Assets".Length;
            var projectPath = Application.dataPath.Substring(0, Application.dataPath.Length - len);
            return projectPath;
        }

        public static void RecompileScripts()
        {
            string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
            foreach (string assetPath in allAssetPaths)
            {
                MonoScript script = AssetDatabase.LoadAssetAtPath(assetPath, typeof(MonoScript)) as MonoScript;
                if (script != null)
                {
                    AssetDatabase.ImportAsset(assetPath);
                    break;
                }
            }
        }

        public static List<string> GetFileList(string path, string filter, bool recursive)
        {
            // create asset list
            List<string> list = new List<string>();

            if (!AssetDatabase.IsValidFolder(path))
            {
                return list;
            }

            // get all assets
            var guids = AssetDatabase.FindAssets(filter, new string[] { path });
            List<string> uniqueGuids = new List<string>();
            foreach (var guid in guids)
            {
                if (!uniqueGuids.Contains(guid))
                {
                    uniqueGuids.Add(guid);
                }
            }

            foreach (var guid in uniqueGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!File.Exists(assetPath))
                {
                    // skip directory
                    continue;
                }

                if (!recursive && Path.GetDirectoryName(assetPath) != path)
                {
                    continue;
                }

                list.Add(assetPath);
            }

            return list;
        }

        public static string GetFullPath(string path)
        {
            var fullPath = EditorHelper.FormatPath(Path.GetFullPath(path));
            return fullPath;
        }

        public static string GetFrameworkRootDirectory(ScriptableObject scriptableObject, int depth = 5)
        {
            var script = MonoScript.FromScriptableObject(scriptableObject);
            var scriptPath = AssetDatabase.GetAssetPath(script);
            var rootDirectory = EditorHelper.GetParentDirectory(scriptPath, depth);
            return rootDirectory;
        }

        private static string GetParentDirectory(string path, int depth)
        {
            int startIndex = -1;
            int count = 0;
            for (int i = path.Length - 1; i >= 0; i--)
            {
                if (path[i] == '/')
                {
                    count++;
                    if (count == depth)
                    {
                        startIndex = i;
                        break;
                    }
                }
            }

            if (startIndex == -1)
            {
                return null;
            }

            var result = path.Substring(0, startIndex);
            return result;
        }
    }
}
