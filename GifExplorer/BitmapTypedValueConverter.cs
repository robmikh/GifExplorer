using System;
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
                default:
                    return $"Unknown type ({typedValue.Type})";
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
