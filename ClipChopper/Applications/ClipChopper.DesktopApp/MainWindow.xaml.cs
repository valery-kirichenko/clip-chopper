using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ookii.Dialogs.Wpf;
using System.Threading.Tasks;

namespace ClipChopper.DesktopApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public sealed partial class MainWindow
    {
        private string? _selectedDirectory;
        private string? _loadedMedia;
        private FragmentSelection? _fragment;
        private int _selectedAudioStream;
        public ObservableCollection<AudioTrack> AudioTracks { get; } = new ObservableCollection<AudioTrack>() {
            new AudioTrack
            {
                Name = "No audio",
                StreamIndex = 0
            } };

        public MainWindow()
        {
            InitializeComponent();
            Media.MediaOpened += Media_MediaOpened;
            Media.MediaChanging += Media_MediaChanging;
        }

        async private void Media_MediaOpened(object? sender, Unosquare.FFME.Common.MediaOpenedEventArgs e)
        {
            AudioTracks.Clear();
            AudioTracks.Add(new AudioTrack
            {
                Name = "Loading...",
                StreamIndex = 0
            });
            AudioTrackSlider.SelectedIndex = 0;
            AudioTrackSlider.IsEnabled = false;
            await Task.Run(() => LoadTags());

            Debug.WriteLine(e.Info.Duration);
            Save.IsEnabled = true;
            _fragment = new FragmentSelection(e.Info.Duration);
            PositionSlider.SelectionStart = _fragment.Start.TotalSeconds;
            PositionSlider.SelectionEnd = _fragment.Stop.TotalSeconds;
            _fragment.PropertyChanged += Fragment_PropertyChanged;
        }

        private void Fragment_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_fragment is null)
            {
                throw new InvalidOperationException("Fragment value is not initialized.");
            }

            if (StringComparer.OrdinalIgnoreCase.Equals(e.PropertyName, "Start"))
            {
                PositionSlider.SelectionStart = _fragment.Start.TotalSeconds;
            }
            else
            {
                PositionSlider.SelectionEnd = _fragment.Stop.TotalSeconds;
            }
        }

        private async void Play_Click(object sender, RoutedEventArgs e)
        {
            if (Media.IsPlaying)
            {
                await Media.Pause();
            }
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
            {
                Media.StepForward();
            }
        }

        private void SelectDirectory_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                _selectedDirectory = dialog.SelectedPath;

                // TODO: implement extensions filter, move this to new method.
                IReadOnlyList<string> files = Directory.GetFiles(_selectedDirectory, "*.*")
                    .Where(s => s.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) ||
                                s.EndsWith(".mkv", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                DirectoryList.ItemsSource = Enumerable.Range(0, files.Count)
                    .Select(i => new DirectoryItem(files[i]))
                    .ToList();
            }
        }

        private void RefreshDirectory_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedDirectory is null) return;

            // TODO: implement extensions filter, move this to new method.
            List<string> files = Directory.GetFiles(_selectedDirectory, "*.*")
                .Where(s => s.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) ||
                            s.EndsWith(".mkv", StringComparison.OrdinalIgnoreCase))
                .ToList();

            DirectoryList.ItemsSource =
                Enumerable.Range(0, files.Count).Select(i => new DirectoryItem(files[i])).ToList();
            if (_loadedMedia != null)
            {
                DirectoryList.SelectedIndex = files.IndexOf(_loadedMedia);
            }
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (_fragment is null) return;

            _fragment.Start = Media.Position;
            Debug.WriteLine(Media.Position);
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            if (_fragment is null) return;

            _fragment.Stop = Media.Position;
        }

        private void DirectoryList_Selected(object sender, SelectionChangedEventArgs args)
        {
            if (DirectoryList.SelectedItem is null ||
                ((DirectoryItem) DirectoryList.SelectedItem).Path == _loadedMedia)
            {
                // Do nothing if selection disappeared or selected file was already loaded.
                return;
            }

            var selectedFile = (DirectoryItem) DirectoryList.SelectedItem;
            if (!File.Exists(selectedFile.Path))
            {
                ShowMessage($"Could not load non-existent file {selectedFile.Path}");
                return;
            }

            Media.Open(new Uri(selectedFile.Path));
            _loadedMedia = selectedFile.Path;
        }

        private void Save_Click(object sender, RoutedEventArgs eventArgs)
        {
            if (_fragment is null) return;

            if (_loadedMedia is null)
            {
                throw new InvalidOperationException("Loaded media value is not initialized.");
            }

            Debug.WriteLine(Path.GetExtension(_loadedMedia).Substring(1, 3));

            var dialog = new Ookii.Dialogs.Wpf.VistaSaveFileDialog()
            {
                AddExtension = true,
                Filter = "MP4 Files (*.mp4)|*.mp4|MKV Files (*.mkv)|*.mkv",
                DefaultExt = "mp4",
                Title = "Save a clip",
                OverwritePrompt = true,
                FileName = "Trimmed " + Path.GetFileName(_loadedMedia)
            };

            if (!dialog.ShowDialog().GetValueOrDefault()) return;

            Console.WriteLine(dialog.FileName);
            var inputFile = _loadedMedia;
            var outputFile = dialog.FileName;

            var ffmpegPath = Path.Combine(Unosquare.FFME.Library.FFmpegDirectory, "ffmpeg.exe");
            var startKeyframe = KeyframeProber.FindClosestKeyframeTime(inputFile, _fragment.Start);

            string args = $"-y -ss {startKeyframe} -i \"{inputFile}\" -map_metadata 0 " +
                          $"-to \"{_fragment.Stop - startKeyframe}\" -c:v copy -c:a copy " +
                          $"-map 0 \"{outputFile}\"";

            var startInfo = new ProcessStartInfo()
            {
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                FileName = ffmpegPath,
                Arguments = args
            };

            using (var ffmpeg = Process.Start(startInfo))
            {
                Debug.Assert(ffmpeg != null, nameof(ffmpeg) + " != null");
                ffmpeg.OutputDataReceived += (s, e) => { Debug.WriteLine(e.Data); };
                ffmpeg.WaitForExit();
            }

            ShowMessage("Done");
            RefreshDirectory_Click(sender, eventArgs);
        }

        private void Volume_Change(object sender, RoutedPropertyChangedEventArgs<double> eventArgs)
        {
            Media.Volume = (Math.Exp(eventArgs.NewValue) - 1) / (Math.E - 1);
        }

        private void ShowMessage(string message)
        {
            if (!TaskDialog.OSSupportsTaskDialogs)
            {
                MessageBox.Show(message);
                return;
            }

            var dialog = new TaskDialog
            {
                WindowTitle = "Clip Chopper",
                MainInstruction = message,
                MainIcon = TaskDialogIcon.Information
            };
            dialog.Buttons.Add(new TaskDialogButton(ButtonType.Ok));
            dialog.ShowDialog(this);
        }

        async private void LoadTags()
        {
            var etOptions = new NExifTool.ExifToolOptions()
            {
                ExifToolPath = AppDomain.CurrentDomain.BaseDirectory + @"\exiftool.exe"
            };
            var et = new NExifTool.ExifTool(etOptions);
            List<NExifTool.Tag> tags = (await et.GetTagsAsync(_loadedMedia)).ToList();
            Dispatcher.Invoke((Action)delegate
            {
                // If you switch between files very quickly, this makes it less likely to crash
                if (!Media.IsLoaded || Media.MediaInfo == null)
                {
                    return;
                }
                var audioStreams = Media.MediaInfo.Streams
                    .Where(kvp => kvp.Value.CodecType == FFmpeg.AutoGen.AVMediaType.AVMEDIA_TYPE_AUDIO)
                    .Select(kvp => kvp.Value);

                AudioTracks.Clear();
                if (!audioStreams.Any())
                {
                    AudioTracks.Add(new AudioTrack
                    {
                        Name = "No audio",
                        StreamIndex = 0
                    });
                    AudioTrackSlider.SelectedIndex = 0;
                    return;
                }
                foreach (var stream in audioStreams)
                {
                    Debug.WriteLine(stream.StreamIndex);
                    AudioTracks.Add(new AudioTrack
                    {
                        Name = $"Audio #{stream.StreamIndex} - " + tags
                            .Where(tag => tag.Name.Equals($"Track{stream.StreamId}Name")).Select(tag => tag.Value)
                            .DefaultIfEmpty("Untitled").First(),
                        StreamIndex = stream.StreamIndex
                    });
                }
                AudioTrackSlider.IsEnabled = true;
                AudioTrackSlider.SelectedIndex = 0;
            });
        }

        private void Media_MediaChanging(object? sender, Unosquare.FFME.Common.MediaOpeningEventArgs e)
        {

            var audioStream = Media.MediaInfo.Streams
                .Where(kvp => kvp.Value.CodecType == FFmpeg.AutoGen.AVMediaType.AVMEDIA_TYPE_AUDIO)
                .Where(kvp => kvp.Value.StreamIndex == _selectedAudioStream)
                .Select(kvp => kvp.Value);
            if (audioStream.Any())
            {
                e.Options.AudioStream = audioStream.First();
            }

        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AudioTrackSlider.SelectedItem == null) return;
            _selectedAudioStream = ((AudioTrack)AudioTrackSlider.SelectedItem).StreamIndex;
            Media.ChangeMedia();
        }
    }
}