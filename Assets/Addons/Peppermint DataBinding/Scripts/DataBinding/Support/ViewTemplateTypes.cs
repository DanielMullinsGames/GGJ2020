using System;
using UnityEngine;

namespace Peppermint.DataBinding
{
    [Serializable]
    public class ViewTemplateConfig
    {
        public string name;
        public GameObject viewTemplate;
    }

    public delegate GameObject SelectViewTemplateDelegate(object item, ViewTemplateConfig[] configs);
}