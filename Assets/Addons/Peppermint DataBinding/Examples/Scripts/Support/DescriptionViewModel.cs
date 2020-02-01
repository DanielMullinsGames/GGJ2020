using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Peppermint.DataBinding.Example
{
    /// <summary>
    /// Example description view model.
    /// </summary>
    public class DescriptionViewModel : BindableMonoBehaviour
    {
        public class TextSegment
        {
            private string text;

            public string Text
            {
                get { return text; }
                set { text = value; }
            }
        }

        public int minLines = 24;

        #pragma warning disable 0649

        [HideInInspector, SerializeField]
        private string text;

        #pragma warning restore 0649

        private bool showInfo;
        private ICommand openCommand;
        private ICommand closeCommand;
        private ObservableList<TextSegment> itemList;

        private const string ColorEndTag = "</color>";

        #region Bindable Properties

        public bool ShowInfo
        {
            get { return showInfo; }
            set { SetProperty(ref showInfo, value, "ShowInfo"); }
        }

        public ICommand OpenCommand
        {
            get { return openCommand; }
            set { SetProperty(ref openCommand, value, "OpenCommand"); }
        }

        public ICommand CloseCommand
        {
            get { return closeCommand; }
            set { SetProperty(ref closeCommand, value, "CloseCommand"); }
        }

        public ObservableList<TextSegment> ItemList
        {
            get { return itemList; }
            set { SetProperty(ref itemList, value, "ItemList"); }
        }

        #endregion

        void Start()
        {
            // create list
            CreateList();

            // create commands
            openCommand = new DelegateCommand(OpenPanel);
            closeCommand = new DelegateCommand(ClosePanel);

            BindingManager.Instance.AddSource(this, typeof(DescriptionViewModel).Name);
        }

        void OnDestroy()
        {
            BindingManager.Instance.RemoveSource(this);
        }

        public void OpenPanel()
        {
            ShowInfo = true;
        }

        public void ClosePanel()
        {
            ShowInfo = false;
        }

        private void CreateList()
        {
            itemList = new ObservableList<TextSegment>();
            var sb = new StringBuilder();
            List<string> list = new List<string>();
            string lastLine = null;

            using (var reader = new StringReader(text))
            {
                string line = null;

                while ((line = reader.ReadLine()) != null)
                {
                    if (list.Count < minLines)
                    {
                        // add to list
                        list.Add(line);
                        continue;
                    }

                    // check line start with end tag
                    var textLine = line.Trim();
                    if (!textLine.StartsWith(ColorEndTag))
                    {
                        // add to list
                        list.Add(line);
                        continue;
                    }

                    // add end tag to last line
                    list[list.Count - 1] = list[list.Count - 1] + ColorEndTag;

                    // add new segment
                    sb.Length = 0;
                    lastLine = list[list.Count - 1];
                    foreach (var item in list)
                    {
                        if (item != lastLine)
                        {
                            sb.AppendLine(item);
                        }
                        else
                        {
                            sb.Append(item);
                        }
                    }

                    // add segment
                    var segment = new TextSegment();
                    segment.Text = sb.ToString();
                    itemList.Add(segment);

                    // add modified line
                    int index = line.IndexOf(ColorEndTag);
                    var newLine = line.Substring(0, index) + line.Substring(index + ColorEndTag.Length);

                    list.Clear();
                    list.Add(newLine);
                }
            }

            if (list.Count > 0)
            {
                // add last segment
                sb.Length = 0;
                lastLine = list[list.Count - 1];
                foreach (var item in list)
                {
                    if (item != lastLine)
                    {
                        sb.AppendLine(item);
                    }
                    else
                    {
                        sb.Append(item);
                    }
                }

                // add last segment
                var lastSegment = new TextSegment();
                lastSegment.Text = sb.ToString();
                itemList.Add(lastSegment);
            }
        }

#if UNITY_EDITOR

        [ContextMenu("Import Text")]
        public void ImportText()
        {
            // get text from clipboard
            var text = GUIUtility.systemCopyBuffer;

            var serializedObject = new UnityEditor.SerializedObject(this);
            var textProperty = serializedObject.FindProperty("text");
            textProperty.stringValue = text;

            serializedObject.ApplyModifiedProperties();
        }

#endif
    }
}