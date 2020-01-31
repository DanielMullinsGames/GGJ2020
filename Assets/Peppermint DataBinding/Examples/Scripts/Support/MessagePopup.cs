using System.Collections.Generic;
using UnityEngine;

namespace Peppermint.DataBinding.Example
{
    /// <summary>
    /// A simple message popup controller
    /// </summary>
    public class MessagePopup : BindableMonoBehaviour
    {
        public class Item : BindableObject
        {
            public enum State
            {
                FadeIn = 0,
                Show,
                FadeOut,
            }

            private string text;
            private float alpha;

            internal State state;
            internal float timer;

            #region Bindable Properties

            public string Text
            {
                get { return text; }
                set { SetProperty(ref text, value, "Text"); }
            }

            public float Alpha
            {
                get { return alpha; }
                set { SetProperty(ref alpha, value, "Alpha"); }
            }

            #endregion

        }

        public int maxCount = 5;
        public float showDuration = 2.0f;
        public float fadeInDuration = 0.2f;
        public float fadeOutDuration = 0.5f;

        private ObservableList<Item> itemList;
        private List<Item> pendingList;

        #region Bindable Properties

        public ObservableList<Item> ItemList
        {
            get { return itemList; }
            set { SetProperty(ref itemList, value, "ItemList"); }
        }

        #endregion

        #region Singleton

        private static MessagePopup instance;

        public static MessagePopup Instance
        {
            get
            {
                if (!instance)
                {
                    instance = FindObjectOfType(typeof(MessagePopup)) as MessagePopup;

                    if (!instance)
                    {
                        Debug.Log("Can not find MessagePopup instance");
                    }
                }

                return instance;
            }
        }

        #endregion

        void Awake()
        {
            instance = this;
        }

        void OnDisable()
        {
            instance = null;
        }

        void Start()
        {
            // create list
            itemList = new ObservableList<Item>();
            pendingList = new List<Item>();

            BindingManager.Instance.AddSource(this, typeof(MessagePopup).Name);
        }

        void OnDestroy()
        {
            BindingManager.Instance.RemoveSource(this);
        }

        void Update()
        {
            foreach (var item in itemList)
            {
                // update timer
                item.timer += Time.deltaTime;
                
                if (item.state == Item.State.FadeIn)
                {
                    // fadeIn state
                    
                    // set alpha
                    float t = item.timer / fadeInDuration;
                    item.Alpha = Mathf.Lerp(0f, 1f, t);

                    // check end
                    if (item.timer > fadeInDuration)
                    {
                        item.Alpha = 1f;

                        // change state to show
                        item.state = Item.State.Show;
                        item.timer = 0f;
                    }
                }
                else if (item.state == Item.State.Show)
                {
                    // show state

                    // check end
                    if (item.timer > showDuration)
                    {
                        // change state to fadeout
                        item.state = Item.State.FadeOut;
                        item.timer = 0f;
                    }
                }
                else
                {
                    // fadeOut state

                    // set alpha
                    float t = item.timer / fadeOutDuration;
                    item.Alpha = Mathf.Lerp(1f, 0f, t);

                    // check end
                    if (item.timer > fadeOutDuration)
                    {
                        // add to pending list
                        pendingList.Add(item);
                    }
                }
            }

            if (pendingList.Count > 0)
            {
                // remove ended items
                foreach (var item in pendingList)
                {
                    itemList.Remove(item);
                }

                pendingList.Clear();
            }
        }

        public void Show(string text)
        {
            // create new item
            var item = new Item();
            item.Text = text;
            item.Alpha = 0f;

            // check max count
            if (itemList.Count >= maxCount)
            {
                // remove last one
                itemList.RemoveAt(itemList.Count - 1);
            }

            // insert to front
            itemList.Insert(0, item);
        }
    }
}
