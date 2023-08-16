using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ClipChopper.WinUIApp
{
    // TODO: use Prism.WPF BindableBase.
    public sealed class FragmentSelection : INotifyPropertyChanged
    {
        private long _start;
        private long _stop;
        private long _duration;

        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public FragmentSelection(long duration)
        {
            Duration = duration;
            Start = 0;
            Stop = duration;
        }

        public long Start
        {
            get => _start;

            set
            {
                if (value != _start)
                {
                    _start = value;

                    if (Stop <= Start)
                    {
                        Stop = Duration;
                    }
                    NotifyPropertyChanged();
                }
            }
        }

        public long Stop
        {
            get => _stop;

            set
            {
                if (value != _stop)
                {
                    _stop = value;

                    if (Start >= value)
                    {
                        Start = 0;
                    }
                    NotifyPropertyChanged();
                }
            }
        }

        public long Duration
        {
            get => _duration;
            set
            {
                _duration = value;
                if (Stop > value)
                {
                    Stop = value;
                }
                NotifyPropertyChanged();
            }
        }
    }
}
