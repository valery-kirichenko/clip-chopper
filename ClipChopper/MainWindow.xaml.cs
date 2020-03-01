using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Diagnostics;

namespace ClipChopper
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string selectedDirectory;
        private string loadedMedia;
        private FragmentSelection fragment;


        public MainWindow()
        {
            InitializeComponent();
            Media.MediaOpened += Media_MediaOpened;
        }

        private void Media_MediaOpened(object sender, Unosquare.FFME.Common.MediaOpenedEventArgs e)
        {
            Console.WriteLine(e.Info.Duration);
            save.IsEnabled = true;
            fragment = new FragmentSelection(e.Info.Duration);
            PositionSlider.SelectionStart = fragment.Start.TotalSeconds;
            PositionSlider.SelectionEnd = fragment.Stop.TotalSeconds;
            fragment.PropertyChanged += Fragment_PropertyChanged;
        }

        private void Fragment_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Start") PositionSlider.SelectionStart = fragment.Start.TotalSeconds;
            else PositionSlider.SelectionEnd = fragment.Stop.TotalSeconds;
        }

        private async void Play_Click(object sender, RoutedEventArgs e)
        {

            if (Media.IsPlaying) await Media.Pause();
            else
            {
                if (Media.HasMediaEnded)
                {
                    await Media.Seek(TimeSpan.Zero);
                }
                await Media.Play();
            }
        }

        private void Pframe_Click(object sender, RoutedEventArgs e)
        {
            Media.StepBackward();
        }

        private void Nframe_Click(object sender, RoutedEventArgs e)
        {
            // Don't make a step if current frame is the last one
            // fixes an issue when StepForward actually moves to a previous key frame
            if (Media.Position + Media.PositionStep < Media.PlaybackEndTime)
                Media.StepForward();
        }

        private void SelectDirectory_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog() == true)
            {
                selectedDirectory = dialog.SelectedPath;
                string[] files = Directory.GetFiles(selectedDirectory, "*.*")
            .Where(s => s.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) || s.EndsWith(".mkv", StringComparison.OrdinalIgnoreCase)).ToArray();

                DirectoryList.ItemsSource = Enumerable.Range(0, files.Length).Select(i => new DirectoryItem(files[i])).ToList();
            }
        }

        private void RefreshDirectory_Click(object sender, RoutedEventArgs e)
        {
            if (selectedDirectory == null) return;
            string[] files = Directory.GetFiles(selectedDirectory, "*.*")
            .Where(s => s.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) || s.EndsWith(".mkv", StringComparison.OrdinalIgnoreCase)).ToArray();

            DirectoryList.ItemsSource = Enumerable.Range(0, files.Length).Select(i => new DirectoryItem(files[i])).ToList();
            DirectoryList.SelectedIndex = Array.IndexOf(files, loadedMedia);
        }

        private void start_Click(object sender, RoutedEventArgs e)
        {
            if (fragment == null) return;
            fragment.Start = Media.Position;
            Console.WriteLine(Media.Position);
        }

        private void stop_Click(object sender, RoutedEventArgs e)
        {
            if (fragment == null) return;
            fragment.Stop = Media.Position;
        }

        private void DirectoryList_Selected(object sender, SelectionChangedEventArgs args)
        {
            if (DirectoryList.SelectedItem == null || ((DirectoryItem) DirectoryList.SelectedItem).Path == loadedMedia)
            {
                // Do nothing if selection disappeared or selected file is already loaded
                return;
            }
            var selectedFile = (DirectoryItem) DirectoryList.SelectedItem;
            if (!File.Exists(selectedFile.Path))
            {
                MessageBox.Show($"Could not load non-existant file {selectedFile.Path}");
                return;
            }
            Media.Open(new Uri(selectedFile.Path));
            loadedMedia = selectedFile.Path;
            
        }

        private void save_Click(object sender, RoutedEventArgs eventArgs)
        {
            if (fragment == null) return;
            Console.WriteLine(Path.GetExtension(loadedMedia).Substring(1, 3));
            var dialog = new Ookii.Dialogs.Wpf.VistaSaveFileDialog()
            {
                AddExtension = true,
                Filter = "MP4 Files (*.mp4)|*.mp4|MKV Files (*.mkv)|*.mkv",
                DefaultExt = "mp4",
                Title = "Save a clip",
                OverwritePrompt = true,
                FileName = "Trimmed " + Path.GetFileName(loadedMedia)
            };
            
            if (dialog.ShowDialog() == true)
            {
                Console.WriteLine(dialog.FileName);
                var inputFile = loadedMedia;
                var outputFile = dialog.FileName;

                var ffmpegPath = Unosquare.FFME.Library.FFmpegDirectory + @"\ffmpeg.exe";
                var startKeyframe = KeyframeProber.FindClosestKeyframeTime(inputFile, fragment.Start);

                var startInfo = new ProcessStartInfo()
                {
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    FileName = ffmpegPath,
                    Arguments = String.Format("-ss {0} -i \"{1}\" -map_metadata 0 -to \"{2}\" -c:v copy -c:a copy -map 0 \"{3}\"",
                    startKeyframe, inputFile, fragment.Stop - startKeyframe, outputFile)
                };

                using (var ffmpeg = Process.Start(startInfo))
                {
                    ffmpeg.OutputDataReceived += new DataReceivedEventHandler((s, e) =>
                    {
                        Console.WriteLine(e.Data);
                    });
                    ffmpeg.WaitForExit();
                }
                MessageBox.Show("Done");
            }
        }
    }

    public class DirectoryItem
    {
        public string Name { get; set; }
        public string Path { get; set; }

        public DirectoryItem(string name, string path)
        {
            this.Name = name;
            this.Path = path;
        }

        public DirectoryItem(string path)
        {
            this.Name = System.IO.Path.GetFileName(path);
            this.Path = path;
        }
    }

    public class FragmentSelection : INotifyPropertyChanged
    {
        private TimeSpan start;
        private TimeSpan stop;
        private TimeSpan duration;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public FragmentSelection(TimeSpan duration)
        {
            this.Duration = duration;
            this.Start = TimeSpan.Zero;
            this.Stop = duration;
        }

        public TimeSpan Start
        {
            get
            {
                return this.start;
            }

            set
            {
                if (value != this.start)
                {
                    this.start = value;
                    NotifyPropertyChanged("Start");

                    if (this.Stop <= this.Start)
                    {
                        this.Stop = this.Duration;
                    }
                }
            }
        }

        public TimeSpan Stop
        {
            get
            {
                return this.stop;
            }

            set
            {
                if (value != this.stop)
                {
                    this.stop = value;
                    NotifyPropertyChanged("Stop");

                    if (this.Start >= value)
                    {
                        this.Start = TimeSpan.Zero;
                    }
                }
            }
        }

        public TimeSpan Duration
        {
            get
            {
                return this.duration;
            }
            set
            {
                this.duration = value;
                if (this.Stop > value)
                {
                    this.Stop = value;
                }
            }
        }
    }
}
