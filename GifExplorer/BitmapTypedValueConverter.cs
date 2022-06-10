using System;
using System.Text;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Data;

namespace GifExplorer
{
    static class BitmapTypedValueStringFormatter
    {
        public static string FormatValue(this BitmapTypedValue value)
        {
            switch (value.Type)
            {
                case PropertyType.UInt16:
                    return $"{(ushort)value.Value}";
                case PropertyType.Boolean:
                    return (bool)value.Value ? "True" : "False";
                case PropertyType.UInt8:
                    return $"{(byte)value.Value}";
                case PropertyType.String:
                    return (string)value.Value;
                case PropertyType.UInt8Array:
                    return $"{PrintArray((byte[])value.Value)}";
                default:
                    return $"Unknown type ({value.Type})";
            }
        }
        private static string PrintArray<T>(T[] array)
        {
            var builder = new StringBuilder();
            builder.Append("[ ");
            builder.AppendJoin(", ", array);
            builder.Append(" ]");
            return builder.ToString();
        }
    }


    class BitmapTypedValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var typedValue = (BitmapTypedValue)value;
            return typedValue.FormatValue();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

    }
}
