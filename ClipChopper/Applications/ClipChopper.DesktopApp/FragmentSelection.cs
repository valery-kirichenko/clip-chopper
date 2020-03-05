using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ClipChopper.DesktopApp
{
    // TODO: use Prism.WPF BindableBase.
    public sealed class FragmentSelection : INotifyPropertyChanged
    {
        private TimeSpan start;
        private TimeSpan stop;
        private TimeSpan duration;

        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public FragmentSelection(TimeSpan duration)
        {
            Duration = duration;
            Start = TimeSpan.Zero;
            Stop = duration;
        }

        public TimeSpan Start
        {
            get => start;

            set
            {
                if (value != start)
                {
                    start = value;
                    NotifyPropertyChanged("Start");

                    if (Stop <= Start)
                    {
                        Stop = Duration;
                    }
                }
            }
        }

        public TimeSpan Stop
        {
            get => stop;

            set
            {
                if (value != stop)
                {
                    stop = value;
                    NotifyPropertyChanged("Stop");

                    if (Start >= value)
                    {
                        Start = TimeSpan.Zero;
                    }
                }
            }
        }

        public TimeSpan Duration
        {
            get => duration;
            set
            {
                duration = value;
                if (Stop > value)
                {
                    Stop = value;
                }
            }
        }
    }
}
