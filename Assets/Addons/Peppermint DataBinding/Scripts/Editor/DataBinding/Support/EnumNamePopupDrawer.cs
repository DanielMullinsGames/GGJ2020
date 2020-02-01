using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Peppermint.DataBinding.Editor
{
    [CustomPropertyDrawer(typeof(EnumNamePopupAttribute))]
    public class EnumNamePopupDrawer : PropertyDrawer
    {
        private string cachedTypeName;
        private string[] enumNames;

        private static List<Type> cachedTypeList;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            CacheEnumType();

            // get attribute
            var targetAttribute = attribute as EnumNamePopupAttribute;

            // get enum type name
            var targetProperty = property.serializedObject.FindProperty(targetAttribute.EnumTypeName);
            var typeName = targetProperty.stringValue;

            if (cachedTypeName != typeName)
            {
                var type = GetEnumByName(typeName);

                if (type != null)
                {
                    // get enum names
                    enumNames = Enum.GetNames(type);
                }
                else
                {
                    enumNames = null;
                }

                // cache type name
                cachedTypeName = typeName;
            }

            EditorGUI.BeginProperty(position, label, property);

            if (enumNames != null)
            {
                // draw popup
                int oldIndex = Array.IndexOf(enumNames, property.stringValue);

                EditorGUI.BeginChangeCheck();
                int newIndex = EditorGUI.Popup(position, label.text, oldIndex, enumNames);
                if (EditorGUI.EndChangeCheck())
                {
                    property.stringValue = enumNames[newIndex];
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

        private Type GetEnumByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            // find enum
            var result = cachedTypeList.FirstOrDefault(x =>
            {
                if (x.Name == name || x.FullName == name)
                {
                    // match name or full name
                    return true;
                }

                if (!string.IsNullOrEmpty(x.Namespace))
                {
                    // trim namespace
                    var partialName = x.FullName.Substring(x.Namespace.Length + 1);

                    if (partialName == name)
                    {
                        // match
                        return true;
                    }
                }

                return false;
            });

            return result;
        }

        private static void CacheEnumType()
        {
            if (cachedTypeList != null)
            {
                return;
            }

            var types = BindingEditorUtility.GetAllTypeList();
            cachedTypeList = types.Where(x => x.IsEnum).ToList();
        }
    }
}
