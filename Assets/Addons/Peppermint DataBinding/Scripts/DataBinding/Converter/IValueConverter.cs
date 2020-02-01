using System;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// Value converter interface.
    /// </summary>
    public interface IValueConverter
    {
        // Convert object to specified type
        object Convert(object value, Type targetType);

        object ConvertBack(object value, Type targetType);
    }
}