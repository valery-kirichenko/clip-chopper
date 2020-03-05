using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ClipChopper
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Unosquare.FFME.Library.FFmpegDirectory = AppDomain.CurrentDomain.BaseDirectory + @"\ffmpeg";
        }
    }
}
