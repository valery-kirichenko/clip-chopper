using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace ClipChopper.WinUIApp
{
    internal class SelectionRectangle
    {
        private Point _startPoint;
        private Point _endPoint;
        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public SelectionRectangle(Point startPoint, Point endPoint)
        {
            _startPoint = startPoint;
            _endPoint = endPoint;
        }

        public double Width
        {
            get => Math.Abs(_startPoint.X - _endPoint.X);
        }

        public double Height
        {
            get => Math.Abs(_startPoint.Y - _endPoint.Y);
        }

        public Point StartPoint
        {
            get => _startPoint;
            set
            {
                _startPoint = value;
                NotifyPropertyChanged();
            }
        }

        public Point EndPoint
        {
            get => _endPoint;
            set
            {
                _endPoint = value;
                NotifyPropertyChanged();
            }
        }

        public Point TopLeft
        {
            get => new Point(Math.Min(_startPoint.X, _endPoint.X), Math.Min(_startPoint.Y, _endPoint.Y));
        }
    }
}
