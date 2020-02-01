using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Peppermint.DataBinding.Example
{
    /// <summary>
    /// A simple level select menu.
    /// </summary>
    public class LevelSelectMenu : BindableMonoBehaviour
    {
        public enum LevelStarNumber
        {
            None,
            One,
            Two,
            Three,
        }

        // metadata of level (readonly)
        public class LevelData
        {
            public int levelID;
            public string name;
        }

        // the current state of level, its value will be changed during gameplay.
        public class LevelState
        {
            public int levelID;
            public int score;
            public bool locked;
            public LevelStarNumber starNumber;
        }

        /// <summary>
        /// LevelItem contains nested LevelData and LevelState. It exposes internal members for data
        /// binding, and also redirect the value modification to internal members.
        ///
        /// Note that we modify "Score" by LevelItem.Score, so its backing field will be changed and
        /// the UI will also get updated. If you modify "score" by LevelItem.LevelState.score, you
        /// need to notify property changed event manually.
        /// </summary>
        public class LevelItem : BindableObject
        {
            // nested data
            private LevelData levelData;
            private LevelState levelState;
            private ICommand selectCommand;

            public LevelData LevelData
            {
                get { return levelData; }
            }

            public LevelState LevelState
            {
                get { return levelState; }
            }

            #region Bindable Properties

            public string Name
            {
                get { return levelData.name; }
            }

            public int LevelID
            {
                get { return levelData.levelID; }
            }

            public LevelStarNumber StarNumber
            {
                get { return levelState.starNumber; }
                set { SetProperty(ref levelState.starNumber, value, "StarNumber"); }
            }

            public int Score
            {
                get { return levelState.score; }
                set { SetProperty(ref levelState.score, value, "Score"); }
            }

            public bool IsLocked
            {
                get { return levelState.locked; }
                set { SetProperty(ref levelState.locked, value, "IsLocked"); }
            }

            public ICommand SelectCommand
            {
                get { return selectCommand; }
                set { SetProperty(ref selectCommand, value, "SelectCommand"); }
            }

            #endregion

            public LevelItem(LevelData levelData, LevelState levelState)
            {
                this.levelData = levelData;
                this.levelState = levelState;
            }
        }

        public class LevelInfo : BindableObject
        {
            private string name;
            private int score;
            private LevelStarNumber starNumber;

            #region Bindable Properties

            public string Name
            {
                get { return name; }
                set { SetProperty(ref name, value, "Name"); }
            }

            public int Score
            {
                get { return score; }
                set { SetProperty(ref score, value, "Score"); }
            }

            public LevelStarNumber StarNumber
            {
                get { return starNumber; }
                set { SetProperty(ref starNumber, value, "StarNumber"); }
            }

            #endregion
        }

        public int levelCount = 25;

        private ObservableList<LevelItem> levelItemList;
        private Dictionary<int, LevelItem> levelItemDictionary;

        private bool showLevelInfo;
        private ICommand closeCommand;
        private ICommand playCommand;
        private LevelItem selectedLevelItem;
        private LevelInfo currentLevelInfo;

        #region Bindable Properties

        public ObservableList<LevelItem> LevelItemList
        {
            get { return levelItemList; }
            set { SetProperty(ref levelItemList, value, "LevelItemList"); }
        }

        public bool ShowLevelInfo
        {
            get { return showLevelInfo; }
            set { SetProperty(ref showLevelInfo, value, "ShowLevelInfo"); }
        }

        public ICommand CloseCommand
        {
            get { return closeCommand; }
            set { SetProperty(ref closeCommand, value, "CloseCommand"); }
        }

        public ICommand PlayCommand
        {
            get { return playCommand; }
            set { SetProperty(ref playCommand, value, "PlayCommand"); }
        }

        public LevelInfo CurrentLevelInfo
        {
            get { return currentLevelInfo; }
        }

        #endregion

        void Awake()
        {
            LoadData();
        }

        void Start()
        {
            // create commands
            closeCommand = new DelegateCommand(CloseLevelInfo);
            playCommand = new DelegateCommand(PlayLevel);

            BindingManager.Instance.AddSource(this, typeof(LevelSelectMenu).Name);
        }

        void OnDestroy()
        {
            BindingManager.Instance.RemoveSource(this);
        }

        private void LoadData()
        {
            // load level data. In this demo, we generate the level data with code. In real game, you
            // should load the level data.
            var levelDataDictionary = new Dictionary<int, LevelData>();
            for (int index = 1; index <= levelCount; index++)
            {
                var item = new LevelData();
                item.levelID = index;
                item.name = string.Format("Level {0:D2}", index);

                levelDataDictionary.Add(item.levelID, item);
            }

            // load level state. In this demo, we generate the state with code. In real game, state
            // should be restored from persistent data. When the state is updated, you should save it
            // to persistent data.
            var levelStateDictionary = new Dictionary<int, LevelState>();
            for (int index = 1; index <= levelCount; index++)
            {
                var item = new LevelState();
                item.levelID = index;

                // set lock state
                item.locked = index > (levelCount / 2);

                if (item.locked)
                {
                    item.starNumber = LevelStarNumber.None;
                }
                else
                {
                    // set star number
                    item.starNumber = (LevelStarNumber)(index % 4);

                    // set score
                    item.score = index * 100;
                }

                // add it
                levelStateDictionary.Add(item.levelID, item);
            }

            // set last unlocked level
            int lastIndex = levelCount / 2;
            var lastUnlockedLevel = levelStateDictionary[lastIndex];
            lastUnlockedLevel.score = 0;
            lastUnlockedLevel.starNumber = LevelStarNumber.None;

            // create level info
            currentLevelInfo = new LevelInfo();

            // create level item list
            levelItemList = new ObservableList<LevelItem>();
            foreach (var kvp in levelStateDictionary)
            {
                var levelID = kvp.Key;
                var levelState = kvp.Value;

                // get level data
                var levelData = levelDataDictionary[levelID];

                // create level item
                var newItem = new LevelItem(levelData, levelState);

                // set command
                newItem.SelectCommand = new DelegateCommand(() => SelectLevel(newItem));

                // add it to list
                levelItemList.Add(newItem);
            }

            // create level item dictionary
            levelItemDictionary = new Dictionary<int, LevelItem>();
            foreach (var item in levelItemList)
            {
                var key = item.LevelData.levelID;

                levelItemDictionary.Add(key, item);
            }
        }

        public void SelectLevel(LevelItem item)
        {
            if (item.IsLocked)
            {
                var lastUnlockedLevel = levelItemList.LastOrDefault(x => !x.IsLocked);

                LogInfo(string.Format("Level {0} is locked, you need to finish level {1} first.", item.LevelID, lastUnlockedLevel.LevelID));
                return;
            }
            else
            {
                Debug.LogFormat("Select level {0}", item.LevelID);
            }

            selectedLevelItem = item;

            // update level info
            currentLevelInfo.Name = selectedLevelItem.Name;
            currentLevelInfo.Score = selectedLevelItem.Score;
            currentLevelInfo.StarNumber = selectedLevelItem.StarNumber;

            // show level info
            ShowLevelInfo = true;
        }

        public void CloseLevelInfo()
        {
            // close level info
            ShowLevelInfo = false;

            // reset selected
            selectedLevelItem = null;
        }

        public void PlayLevel()
        {
            // update level state with random number
            selectedLevelItem.Score = UnityEngine.Random.Range(1000, 9999);
            selectedLevelItem.StarNumber = (LevelStarNumber)UnityEngine.Random.Range(1, 4);

            LogInfo(String.Format("Level {0} finished. score={1}, starNumber={2}", selectedLevelItem.LevelID, selectedLevelItem.Score, selectedLevelItem.StarNumber));

            // unlock next level
            int nextLevelID = selectedLevelItem.LevelData.levelID + 1;
            if (levelItemDictionary.ContainsKey(nextLevelID))
            {
                // get next level
                var nextLevel = levelItemDictionary[nextLevelID];

                if (nextLevel.IsLocked)
                {
                    // unlock level
                    nextLevel.IsLocked = false;

                    LogInfo(String.Format("Unlock level {0}", nextLevel.LevelID));
                }
            }

            // close level info
            ShowLevelInfo = false;

            // reset selected
            selectedLevelItem = null;
        }

        private void LogInfo(string text)
        {
            MessagePopup.Instance.Show(text);
            Debug.Log(text);
        }
    }
}
