using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Stand_upTimer
{
    public class DateFormatterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string formatString = parameter as string;
            if (!string.IsNullOrEmpty(formatString))
            {
                var timeSpan = (TimeSpan)value;
                TimeSpan fromSeconds = TimeSpan.FromSeconds(Math.Floor(timeSpan.TotalSeconds));
                return fromSeconds.ToString(formatString);
            }

            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
