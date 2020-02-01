using System;
using UnityEditor;
using UnityEngine;

namespace Peppermint.DataBinding.Editor
{
    [CustomPropertyDrawer(typeof(StyleSetter))]
    public class StyleSetterDrawer : PropertyDrawer
    {
        private const float LineSpace = 2f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // get target
            var targetProp = property.serializedObject.FindProperty("target");
            var targetObject = targetProp.objectReferenceValue;

            int lineNumber = 2;
            if (targetObject == null)
            {
                // add extra line
                lineNumber += 1;
            }

            var height = EditorGUIUtility.singleLineHeight * lineNumber + LineSpace * (lineNumber - 1);
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // default string field
            EditorGUI.BeginProperty(position, label, property);

            // find properties
            var targetPathProp = property.FindPropertyRelative("targetPath");
            var valueProp = property.FindPropertyRelative("value");
            var unionValueTypeProp = valueProp.FindPropertyRelative("valueType");

            // get target
            var targetProp = property.serializedObject.FindProperty("target");
            var targetObject = targetProp.objectReferenceValue;

            // rect for single line
            var lineRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            EditorGUI.PropertyField(lineRect, targetPathProp);

            Type targetPropertyType = null;
            if (targetObject == null)
            {
                lineRect.y += (EditorGUIUtility.singleLineHeight + LineSpace);

                // manual set value type
                EditorGUI.PropertyField(lineRect, unionValueTypeProp);
            }
            else
            {
                // auto set value type
                var targetType = targetObject.GetType();
                var pi = targetType.GetProperty(targetPathProp.stringValue);

                var valueType = UnionObject.ValueType.None;
                if (pi != null)
                {
                    // get property type
                    targetPropertyType = pi.PropertyType;
                    valueType = GetValueType(targetPropertyType);
                }

                var valueTypeProp = valueProp.FindPropertyRelative("valueType");
                if (valueTypeProp.intValue != (int)valueType)
                {
                    // update value type
                    valueProp.FindPropertyRelative("valueType").intValue = (int)valueType;
                }
            }

            // move to next line
            lineRect.y += (EditorGUIUtility.singleLineHeight + LineSpace);

            var newValueType = (UnionObject.ValueType)unionValueTypeProp.intValue;
            var path = GetUnionObjectValuePath(newValueType);
            if (path != null)
            {
                var prop = valueProp.FindPropertyRelative(path);

                if (targetPropertyType == null)
                {
                    // use default
                    EditorGUI.PropertyField(lineRect, prop);
                }
                else
                {
                    if (targetPropertyType.IsEnum)
                    {
                        // show enum popup
                        var enumValue = Enum.ToObject(targetPropertyType, prop.intValue);

                        EditorGUI.BeginChangeCheck();
                        enumValue = EditorGUI.EnumPopup(lineRect, "Enum Value", (Enum)enumValue);
                        if (EditorGUI.EndChangeCheck())
                        {
                            prop.intValue = (int)enumValue;
                        }
                    }
                    else if (newValueType == UnionObject.ValueType.Object)
                    {
                        // reset object value if type not match
                        if (prop.objectReferenceValue != null && prop.objectReferenceValue.GetType() != targetPropertyType)
                        {
                            var objectValueType = prop.objectReferenceValue.GetType();
                            var msg = string.Format("Object value type {0} not matched target property type {1}, reset object value.", objectValueType, targetPropertyType);
                            Debug.LogWarning(msg, property.serializedObject.targetObject);

                            // reset to null
                            prop.objectReferenceValue = null;
                        }

                        // show object field with specified type
                        EditorGUI.BeginChangeCheck();
                        var newValue = EditorGUI.ObjectField(lineRect, "Object Value", prop.objectReferenceValue, targetPropertyType, true);
                        if (EditorGUI.EndChangeCheck())
                        {
                            prop.objectReferenceValue = newValue;
                        }
                    }
                    else
                    {
                        // use default
                        EditorGUI.PropertyField(lineRect, prop);
                    }
                }
            }
            else
            {
                EditorGUI.LabelField(lineRect, "Value", "Unsupported type!");
            }

            EditorGUI.EndProperty();
        }

        private string GetUnionObjectValuePath(UnionObject.ValueType valueType)
        {
            string path = null;
            switch (valueType)
            {
                case UnionObject.ValueType.Integer:
                    path = "intValue";
                    break;
                case UnionObject.ValueType.Boolean:
                    path = "boolValue";
                    break;
                case UnionObject.ValueType.Float:
                    path = "floatValue";
                    break;
                case UnionObject.ValueType.String:
                    path = "stringValue";
                    break;
                case UnionObject.ValueType.Color:
                    path = "colorValue";
                    break;
                case UnionObject.ValueType.Vector2:
                    path = "vector2Value";
                    break;
                case UnionObject.ValueType.Vector3:
                    path = "vector3Value";
                    break;
                case UnionObject.ValueType.Object:
                    path = "objectValue";
                    break;
                case UnionObject.ValueType.None:
                    path = null;
                    break;
                default:
                    Debug.LogErrorFormat("Unsupported value type {0}", valueType);
                    break;
            }

            return path;
        }

        private UnionObject.ValueType GetValueType(Type type)
        {
            if (type == typeof(Int32))
            {
                return UnionObject.ValueType.Integer;
            }
            else if (type == typeof(Boolean))
            {
                return UnionObject.ValueType.Boolean;
            }
            else if (type == typeof(Single))
            {
                return UnionObject.ValueType.Float;
            }
            else if (type == typeof(Color))
            {
                return UnionObject.ValueType.Color;
            }
            else if (type == typeof(String))
            {
                return UnionObject.ValueType.String;
            }
            else if (type == typeof(Vector2))
            {
                return UnionObject.ValueType.Vector2;
            }
            else if (type == typeof(Vector3))
            {
                return UnionObject.ValueType.Vector3;
            }
            else if (type.IsEnum)
            {
                return UnionObject.ValueType.Integer;
            }
            else if (type.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                return UnionObject.ValueType.Object;
            }
            else
            {
                return UnionObject.ValueType.None;
            }
        }

    }
}
