using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Peppermint.DataBinding.Editor
{
    [CustomPropertyDrawer(typeof(PropertyNamePopupAttribute))]
    public class PropertyNamePopupDrawer : PropertyDrawer
    {
        private Type cachedType;
        private string[] propertyNames;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // get attribute
            var targetAttribute = attribute as PropertyNamePopupAttribute;

            // get target
            var targetProperty = property.serializedObject.FindProperty(targetAttribute.PropertyName);
            var targetObject = targetProperty.objectReferenceValue;

            if (targetObject != null)
            {
                var type = targetObject.GetType();

                // check if type changed
                if (type != cachedType)
                {
                    propertyNames = GetPropertyNames(type);
                    cachedType = type;
                }
            }
            else
            {
                // clean cached
                cachedType = null;
                propertyNames = null;
            }

            EditorGUI.BeginProperty(position, label, property);

            if (propertyNames != null)
            {
                // draw popup
                int oldIndex = Array.IndexOf(propertyNames, property.stringValue);

                EditorGUI.BeginChangeCheck();
                int newIndex = EditorGUI.Popup(position, label.text, oldIndex, propertyNames);
                if (EditorGUI.EndChangeCheck())
                {
                    property.stringValue = propertyNames[newIndex];
                }
            }
            else
            {
                // default string field
                EditorGUI.BeginChangeCheck();
                var newValue = EditorGUI.TextField(position, label.text, property.stringValue);
                if (EditorGUI.EndChangeCheck())
                {
                    property.stringValue = newValue;
                }
            }

            EditorGUI.EndProperty();
        }

        private string[] GetPropertyNames(Type type)
        {
            var propertyNameList = new List<string>();

            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var item in properties)
            {
                // check property
                if (item.CanWrite && (item.GetSetMethod() != null))
                {
                    propertyNameList.Add(item.Name);
                }
            }

            return propertyNameList.ToArray();
        }
    }
}
