using System.Collections.Generic;
using UnityEngine;

#if UNITY_5_4_OR_NEWER
using UnityEngine.SceneManagement;
#endif

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace Peppermint.DataBinding.Example
{
    /// <summary>
    /// A simple menu controller for data binding examples.
    /// </summary>
    public class MenuController : BindableMonoBehaviour
    {
        public class SceneItem : BindableObject
        {
            private string sceneName;
            private bool isSelected;
            private ICommand loadCommand;

            #region Bindable Properties

            public string SceneName
            {
                get { return sceneName; }
                set { SetProperty(ref sceneName, value, "SceneName"); }
            }

            public bool IsSelected
            {
                get { return isSelected; }
                set { SetProperty(ref isSelected, value, "IsSelected"); }
            }

            public ICommand LoadCommand
            {
                get { return loadCommand; }
                set { SetProperty(ref loadCommand, value, "LoadCommand"); }
            }

            #endregion
        }

        public string[] sceneNames;

        private bool isMenuVisible;
        private string currentSceneName;
        private ICommand openMenuCommand;
        private ICommand closeMenuCommand;
        private ObservableList<SceneItem> sceneList;

        #region Bindable Properties

        public bool IsMenuVisible
        {
            get { return isMenuVisible; }
            set { SetProperty(ref isMenuVisible, value, "IsMenuVisible"); }
        }

        public string CurrentSceneName
        {
            get { return currentSceneName; }
            set { SetProperty(ref currentSceneName, value, "CurrentSceneName"); }
        }

        public ICommand OpenMenuCommand
        {
            get { return openMenuCommand; }
            set { SetProperty(ref openMenuCommand, value, "OpenMenuCommand"); }
        }

        public ICommand CloseMenuCommand
        {
            get { return closeMenuCommand; }
            set { SetProperty(ref closeMenuCommand, value, "CloseMenuCommand"); }
        }

        public ObservableList<SceneItem> SceneList
        {
            get { return sceneList; }
            set { SetProperty(ref sceneList, value, "SceneList"); }
        }

        #endregion

        void Start()
        {

#if !UNITY_EDITOR
            Application.targetFrameRate = 60;
#endif
            // show menu
            OpenMenu();

            // create scene list
            sceneList = new ObservableList<SceneItem>();
            foreach (var sceneName in sceneNames)
            {
                var item = new SceneItem();
                item.SceneName = sceneName;
                item.LoadCommand = new DelegateCommand(() => LoadScene(item));

                // add to list
                sceneList.Add(item);
            }

            // create commands
            openMenuCommand = new DelegateCommand(OpenMenu);
            closeMenuCommand = new DelegateCommand(CloseMenu);

#if UNITY_5_4_OR_NEWER
            SceneManager.sceneLoaded += OnSceneLoaded;
#endif

            UpdateSceneName();

            BindingManager.Instance.AddSource(this, typeof(MenuController).Name);

            // do not destroy controller
            GameObject.DontDestroyOnLoad(gameObject);
        }

        void OnDestroy()
        {
#if UNITY_5_4_OR_NEWER
            SceneManager.sceneLoaded -= OnSceneLoaded;
#endif

            BindingManager.Instance.RemoveSource(this);
        }

#if UNITY_5_4_OR_NEWER
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            UpdateSceneName();
        }
#else
        void OnLevelWasLoaded(int level)
        {
            UpdateSceneName();
        }
#endif

        public void LoadScene(SceneItem sceneItem)
        {
            // hide menu
            IsMenuVisible = false;

            // set selected
            foreach (var item in sceneList)
            {
                if (item == sceneItem)
                {
                    item.IsSelected = true;
                }
                else
                {
                    item.IsSelected = false;
                }
            }

            if (sceneItem.SceneName == "Start" || sceneItem.SceneName == "NGUI Start")
            {
                // destroy current menu controller
                Destroy(gameObject);
            }

            // load scene
#if UNITY_5_3_OR_NEWER
            SceneManager.LoadScene(sceneItem.SceneName);
#else
            Application.LoadLevel(sceneItem.SceneName);
#endif
        }

        private void UpdateSceneName()
        {
#if UNITY_5_3_OR_NEWER
            CurrentSceneName = SceneManager.GetActiveScene().name;
#else
            CurrentSceneName = Application.loadedLevelName;
#endif
        }

        public void OpenMenu()
        {
            IsMenuVisible = true;
        }

        public void CloseMenu()
        {
            IsMenuVisible = false;
        }

#if UNITY_EDITOR

        void Reset()
        {
            var nameList = new List<string>();
            int sceneIndex = 0;

            // get scene names from build settings
            foreach (var item in EditorBuildSettings.scenes)
            {
                if (!item.enabled)
                {
                    continue;
                }

                if (sceneIndex == 0)
                {
                    // skip first scene
                    sceneIndex++;
                    continue;
                }

                var sceneName = Path.GetFileNameWithoutExtension(item.path);
                nameList.Add(sceneName);

                sceneIndex++;
            }

            sceneNames = nameList.ToArray();
        }

#endif
    }
}
