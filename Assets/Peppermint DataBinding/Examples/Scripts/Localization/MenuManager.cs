using System;
using System.Collections.Generic;
using UnityEngine;

namespace Peppermint.DataBinding.Example
{
    /// <summary>
    /// A simple menu manager which uses stack to manage panels.
    /// </summary>
    public class MenuManager : MonoBehaviour
    {
        [Serializable]
        public class Menu
        {
            public string name;
            public GameObject[] panels;
        }

        public Menu[] menus;
        public string startMenu;

        private Stack<Menu> menuStack;
        private Dictionary<string, Menu> menuDictionary;

        #region Singleton

        private static MenuManager instance;

        public static MenuManager Instance
        {
            get
            {
                if (!instance)
                {
                    instance = FindObjectOfType(typeof(MenuManager)) as MenuManager;

                    if (!instance)
                    {
                        Debug.Log("Can not find MenuManager instance");
                    }
                }

                return instance;
            }
        }

        #endregion

        void Awake()
        {
            instance = this;

            // initialize
            menuDictionary = new Dictionary<string, Menu>();
            foreach (var item in menus)
            {
                menuDictionary.Add(item.name, item);
            }
            menuStack = new Stack<Menu>();

            PushMenu(startMenu);
        }

        void OnDisable()
        {
            instance = null;
        }

        private Menu GetMenu(string name)
        {
            Menu value;
            menuDictionary.TryGetValue(name, out value);
            return value;
        }

        private void EnableMenu(Menu menu, bool value)
        {
            foreach (var item in menu.panels)
            {
                item.SetActive(value);
            }
        }

        public void PushMenu(string menuName)
        {
            var menu = GetMenu(menuName);
            if (menu == null)
            {
                Debug.LogError(string.Format("Invalid menu {0}", menuName), gameObject);
                return;
            }

            if (menuStack.Count > 0)
            {
                // hide current
                var top = menuStack.Peek();
                EnableMenu(top, false);
            }

            // push menu
            menuStack.Push(menu);

            // show current
            EnableMenu(menu, true);
        }

        public void PopMenu()
        {
            if (menuStack.Count == 0)
            {
                return;
            }

            // pop menu
            var top = menuStack.Pop();

            // hide current
            EnableMenu(top, false);

            // get new menu
            if (menuStack.Count > 0)
            {
                var current = menuStack.Peek();

                // show current
                EnableMenu(current, true);
            }
        }
    }
}
