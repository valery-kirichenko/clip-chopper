using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ClipChopper.DesktopApp
{
    // TODO: use Prism.WPF BindableBase.
    public sealed class FragmentSelection : INotifyPropertyChanged
    {
        private TimeSpan _start;
        private TimeSpan _stop;
        private TimeSpan _duration;

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
            get => _start;

            set
            {
                if (value != _start)
                {
                    _start = value;
                    NotifyPropertyChanged();

                    if (Stop <= Start)
                    {
                        Stop = Duration;
                    }
                }
            }
        }

        public TimeSpan Stop
        {
            get => _stop;

            set
            {
                if (value != _stop)
                {
                    _stop = value;
                    NotifyPropertyChanged();

                    if (Start >= value)
                    {
                        Start = TimeSpan.Zero;
                    }
                }
            }
        }

        private TimeSpan Duration
        {
            get => _duration;
            set
            {
                _duration = value;
                if (Stop > value)
                {
                    Stop = value;
                }
            }
        }
    }
}
