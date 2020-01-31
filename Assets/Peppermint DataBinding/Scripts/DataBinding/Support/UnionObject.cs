using System;
using UnityEngine;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// Union object can hold different value types.
    /// </summary>
    [Serializable]
    public class UnionObject
    {
        public enum ValueType
        {
            None,
            Integer,
            Boolean,
            Float,
            String,
            Color,
            Vector2,
            Vector3,
            Object,
        }

        public ValueType valueType;
        public int intValue;
        public bool boolValue;
        public float floatValue;
        public string stringValue;
        public Color colorValue;
        public Vector2 vector2Value;
        public Vector3 vector3Value;
        public UnityEngine.Object objectValue;

        public object GetBoxedValue()
        {
            switch (valueType)
            {
                case ValueType.Integer:
                    return intValue;
                    
                case ValueType.Boolean:
                    return boolValue;

                case ValueType.Float:
                    return floatValue;

                case ValueType.String:
                    return stringValue;

                case ValueType.Color:
                    return colorValue;

                case ValueType.Vector2:
                    return vector2Value;

                case ValueType.Vector3:
                    return vector3Value;

                case ValueType.Object:
                    return objectValue;

                default:
                    Debug.LogErrorFormat("Unsupported value type {0}", valueType);
                    return null;
            }
        }
    }
}
