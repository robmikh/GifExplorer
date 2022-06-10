using System;
using System.Text;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Data;

namespace GifExplorer
{
    class BitmapTypedValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var typedValue = (BitmapTypedValue)value;
            switch (typedValue.Type)
            {
                case PropertyType.UInt16:
                    return $"{(ushort)typedValue.Value}";
                case PropertyType.Boolean:
                    return (bool)typedValue.Value ? "True" : "False";
                case PropertyType.UInt8:
                    return $"{(byte)typedValue.Value}";
                case PropertyType.String:
                    return (string)typedValue.Value;
                case PropertyType.UInt8Array:
                    return $"{PrintArray((byte[])typedValue.Value)}";
                default:
                    return $"Unknown type ({typedValue.Type})";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        static string PrintArray<T>(T[] array)
        {
            var builder = new StringBuilder();
            builder.Append("[ ");
            builder.AppendJoin(", ", array);
            builder.Append(" ]");
            return builder.ToString();
        }

    }
}
