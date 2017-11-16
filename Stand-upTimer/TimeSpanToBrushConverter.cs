using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Stand_upTimer
{
    public class TimeSpanToBrushConverter : IValueConverter
    {
        public TimeSpan Limit { get; set; } = TimeSpan.Zero;
        public Brush HighterBrush { get; set; } = new SolidColorBrush(Colors.Black);
        public Brush LowerBrush { get; set; } = new SolidColorBrush(Colors.Red);

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var timeSpan = (TimeSpan)value;
            return timeSpan < Limit ? LowerBrush : HighterBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
