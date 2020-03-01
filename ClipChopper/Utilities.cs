using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ClipChopper
{
    class TimeSpanToSecondsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case TimeSpan span:
                    return span.TotalSeconds;
                case Duration duration:
                    return duration.HasTimeSpan ? duration.TimeSpan.TotalSeconds : 0d;
                default:
                    return 0d;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double == false) return 0d;
            var result = TimeSpan.FromTicks(System.Convert.ToInt64(TimeSpan.TicksPerSecond * (double)value));

            // Do the conversion from visibility to bool
            if (targetType == typeof(TimeSpan)) return result;
            return targetType == typeof(Duration) ?
                new Duration(result) : Activator.CreateInstance(targetType);
        }
    }

    class TimeSpanFormatter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TimeSpan? p;

            switch (value)
            {
                case TimeSpan position:
                    p = position;
                    break;
                case Duration duration when duration.HasTimeSpan:
                    p = duration.TimeSpan;
                    break;
                default:
                    return string.Empty;
            }

            if (p.Value == TimeSpan.MinValue)
                return "N/A";

            return $"{(int)p.Value.TotalHours:00}:{p.Value.Minutes:00}:{p.Value.Seconds:00}.{p.Value.Milliseconds:000}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
