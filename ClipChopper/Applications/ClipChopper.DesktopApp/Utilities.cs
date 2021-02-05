using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ClipChopper
{
    internal sealed class TimeSpanToSecondsConverter : IValueConverter
    {
        public TimeSpanToSecondsConverter()
        {
        }

        #region IValueConverter Implementation

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                TimeSpan span => span.TotalSeconds,

                Duration duration => duration.HasTimeSpan ? duration.TimeSpan.TotalSeconds : 0d,

                _ => 0d
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            if (!(value is double)) return 0d;

            var result = TimeSpan.FromTicks(
                System.Convert.ToInt64(TimeSpan.TicksPerSecond * (double) value)
            );

            // Do the conversion from visibility to bool
            if (targetType == typeof(TimeSpan)) return result;

            // TODO: wrap conversion logic to new class/method.
            object convertedBack = targetType == typeof(Duration)
                ? new Duration(result)
                : Activator.CreateInstance(targetType)
                    ?? throw new InvalidOperationException($"Activator cannot create instance of type {nameof(Duration)}.");

            return convertedBack;
        }

        #endregion
    }

    internal sealed class TimeSpanFormatter : IValueConverter
    {
        public TimeSpanFormatter()
        {
        }

        #region IValueConverter Implementation

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

            if (p.Value == TimeSpan.MinValue) return "N/A";

            return $"{(int)p.Value.TotalHours:00}:{p.Value.Minutes:00}:{p.Value.Seconds:00}.{p.Value.Milliseconds:000}";
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
