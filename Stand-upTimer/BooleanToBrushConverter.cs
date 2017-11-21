using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Stand_upTimer
{
    public class BooleanToBrushConverter : IValueConverter
    {
        public Brush FalseBrush { get; set; } = new SolidColorBrush(Colors.Black);
        public Brush TrueBrush { get; set; } = new SolidColorBrush(Colors.Red);

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (bool)value  ? TrueBrush : FalseBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
