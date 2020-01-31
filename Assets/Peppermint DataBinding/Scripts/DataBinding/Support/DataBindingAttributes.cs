using System;
using UnityEngine;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// Allow a class to be initialized when the ValueConverterProvider is created. The static
    /// constructor will be called.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class InitializeValueConverterAttribute : Attribute
    {
        /// <summary>
        /// Specify the calling order
        /// </summary>
        public int Order { get; set; }
    }


    [AttributeUsage(AttributeTargets.Class)]
    public class BinderAttribute : Attribute
    {
        /// <summary>
        /// Used in graph tool to get the source path list. If the method is not specified the graph
        /// tool will get all matched fields as source path. The field must be a public string, and
        /// its name must end with 'path' (case is ignored).
        /// </summary>
        public string SourcePathListMethod { get; set; }
    }

    /// <summary>
    /// Indicate this property is used in data binding, so the AOT code builder will include this
    /// property in code generation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class BindablePropertyAttribute : Attribute
    {

    }

    /// <summary>
    /// Draw a popup selection field that contains all public properties of the specified target.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class PropertyNamePopupAttribute : PropertyAttribute
    {
        /// <summary>
        /// The target property name
        /// </summary>
        public string PropertyName { get; private set; }

        public PropertyNamePopupAttribute(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException("propertyName");
            }

            PropertyName = propertyName;
        }
    }

    /// <summary>
    /// Draw a enum popup of the specified target.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class EnumNamePopupAttribute : PropertyAttribute
    {
        /// <summary>
        /// The target enum type name
        /// </summary>
        public string EnumTypeName { get; private set; }

        public EnumNamePopupAttribute(string enumTypeName)
        {
            if (string.IsNullOrEmpty(enumTypeName))
            {
                throw new ArgumentException("enumTypeName");
            }

            EnumTypeName = enumTypeName;
        }
    }
}