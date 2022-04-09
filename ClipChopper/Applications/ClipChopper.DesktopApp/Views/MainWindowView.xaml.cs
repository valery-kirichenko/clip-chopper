using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shell;
using Acolyte.Common;
using ClipChopper.Domain;
using ClipChopper.Domain.Errors;
using ClipChopper.Logging;
using FFmpeg.AutoGen;
using Newtonsoft.Json;
using NExifTool;
using Ookii.Dialogs.Wpf;
using Unosquare.FFME;
using Unosquare.FFME.Common;

namespace ClipChopper.DesktopApp.Views
{
    /// <summary>
    /// Interaction logic for MainWindowView.xaml
    /// </summary>
    public sealed partial class MainWindowView
    {
        /// <summary>
        /// Logger instance for current class.
        /// </summary>
        private static readonly ILogger _logger = LoggerFactory.CreateLoggerFor<MainWindowView>();

        private string? _selectedDirectory;
        private string? _loadedMedia;
        private FragmentSelection? _fragment;
        private int _selectedAudioStream;

        public ObservableCollection<AudioTrack> AudioTracks { get; } = new(new List<AudioTrack>
        {
            new AudioTrack
            {
                Name = "No Audio",
                StreamIndex = 0
            }
        });


        public MainWindowView()
        {
            InitializeComponent();
            Media.MediaOpened += Media_MediaOpened;
            Media.MediaChanging += Media_MediaChanging;
        }
        
        private async void Media_MediaOpened(object? sender, MediaOpenedEventArgs e)
        {
            try
            {
                await Media_MediaOpened_Internal(sender, e);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to open media.");
                TaskDialogHelper.ShowErrorTaskDialog(this, ex);
            }
        }

        private async Task Media_MediaOpened_Internal(object? sender, MediaOpenedEventArgs e)
        {
            var loadTags = LoadTags();
            loadTags.FireAndForgetSafeAsync(new DisplayTaskDialogErrorHandler(this));
            // Task will be completed here.
            var tags = await loadTags;
            
            AudioTracks.Clear();
            if (Media.MediaInfo is null)
            {
                return;
            }
            var audioStreams = Media.MediaInfo.Streams
                .Where(kvp => kvp.Value.CodecType == AVMediaType.AVMEDIA_TYPE_AUDIO)
                .Select(kvp => kvp.Value);
            
            if (!audioStreams.Any())
            {
                AudioTracks.Add(new AudioTrack
                {
                    Name = "No Audio",
                    StreamIndex = 0
                });
                AudioTrackSlider.IsEnabled = false;
            }
            else
            {
                foreach (var stream in audioStreams)
                {
                    AudioTracks.Add(new AudioTrack
                    {
                        Name = $"Audio #{stream.StreamIndex} - " + tags
                            .Where(tag => tag.Name.Equals($"Track{stream.StreamId}Name"))
                            .Select(tag => tag.Value)
                            .DefaultIfEmpty("Untitled")
                            .First(),
                        StreamIndex = stream.StreamIndex
                    });
                }
                AudioTrackSlider.IsEnabled = true;
            }

            AudioTrackSlider.SelectedIndex = 0;

            _logger.Info($"Media info has been opened. Duration: [{e.Info.Duration}].");
            Save.IsEnabled = true;
            _fragment = new FragmentSelection(e.Info.Duration);
            PositionSlider.SelectionStart = _fragment.Start.TotalSeconds;
            PositionSlider.SelectionEnd = _fragment.Stop.TotalSeconds;
            _fragment.PropertyChanged += Fragment_PropertyChanged;
        }

        private void Fragment_PropertyChanged(object? sender, PropertyChangedEventArgs e)
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

        private async void Play_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                await Play_Click_Internal(sender, e);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to play media.");
                TaskDialogHelper.ShowErrorTaskDialog(this, ex);
            }
        }

        private async Task Play_Click_Internal(object? sender, RoutedEventArgs e)
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

        private void Pframe_Click(object? sender, RoutedEventArgs e)
        {
            if (Media.Position - Media.PositionStep > Media.PlaybackStartTime)
            {
                Media.StepBackward();
            }
        }

        private void Nframe_Click(object? sender, RoutedEventArgs e)
        {
            // Don't make a step if current frame is the last one
            // fixes an issue when StepForward actually moves to a previous key frame
            if (Media.Position + Media.PositionStep < Media.PlaybackEndTime)
            {
                Media.StepForward();
            }
        }

        private void SelectDirectory_Click(object? sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog();
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                _selectedDirectory = dialog.SelectedPath;
                LoadDirectory();
            }
        }

        private void RefreshDirectory_Click(object? sender, RoutedEventArgs e)
        {
            LoadDirectory();
        }

        private void LoadDirectory()
        {
            if (_selectedDirectory is null) return;

            if (!Directory.Exists(_selectedDirectory))
            {
                _selectedDirectory = null;
                ShowMessage("Directory no longer exists.");
                return;
            }

            // TODO: implement extensions filter, move this to new method.
            var files = Directory.GetFiles(_selectedDirectory, "*.*")
                .Where(s => s.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) ||
                            s.EndsWith(".mkv", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            DirectoryList.ItemsSource = Enumerable.Range(0, files.Length)
                .Select(i => new DirectoryItem(files[i]))
                .ToList();

            if (_loadedMedia != null)
            {
                DirectoryList.SelectedIndex = Array.IndexOf(files, _loadedMedia);
            }
        }

        private void Start_Click(object? sender, RoutedEventArgs e)
        {
            if (_fragment is null) return;

            _fragment.Start = Media.Position;
            _logger.Info($"Start position of the clip has been selected. Position: [{Media.Position}].");
        }

        private void Stop_Click(object? sender, RoutedEventArgs e)
        {
            if (_fragment is null) return;

            _fragment.Stop = Media.Position;
        }

        private void DirectoryList_Selected(object? sender, SelectionChangedEventArgs args)
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
                LoadDirectory();
                return;
            }

            Media.Open(new Uri(selectedFile.Path));
            _loadedMedia = selectedFile.Path;
        }

        private async void Save_Click(object? sender, RoutedEventArgs eventArgs)
        {
            try
            {
                await Save_Click_Inernal(sender, eventArgs);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to save media.");
                TaskDialogHelper.ShowErrorTaskDialog(this, ex);
            }
        }

        private async Task Save_Click_Inernal(object? sender, RoutedEventArgs eventArgs)
        {
            if (_fragment is null) return;

            if (_loadedMedia is null)
            {
                throw new InvalidOperationException("Loaded media value is not initialized.");
            }

            _logger.Info("Saving clip.");

            var dialog = new VistaSaveFileDialog
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

            var ffmpegPath = Path.Combine(Library.FFmpegDirectory, "ffmpeg.exe");
            Status.Text = "Looking for keyframes...";
            TaskbarProgress.ProgressState = TaskbarItemProgressState.Normal;
            var progress = new Progress<int>(value =>
            {
                Status.Text = $"Looking for keyframes... {value}%";
                TaskbarProgress.ProgressValue = value / 100.0d;
                Console.WriteLine(TaskbarProgress.ProgressValue);
            });
            var startKeyframe = await Task.Run(() => KeyframeProber.FindClosestKeyframeTime(inputFile, _fragment.Start, progress));

            string args = $"-y -ss {startKeyframe} -i \"{inputFile}\" -map_metadata 0 " +
                          $"-to \"{_fragment.Stop - startKeyframe}\" -c:v copy -c:a copy " +
                          $"-map 0 \"{outputFile}\"";

            var startInfo = new ProcessStartInfo
            {
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                FileName = ffmpegPath,
                Arguments = args
            };

            Status.Text = "Trimming...";
            TaskbarProgress.ProgressState = TaskbarItemProgressState.Indeterminate;
            await Task.Run(() =>
            {
                using var ffmpeg = Process.Start(startInfo);
                Debug.Assert(ffmpeg != null, nameof(ffmpeg) + " != null");
                ffmpeg.OutputDataReceived += (s, e) => _logger.Info(e.Data.ToStringNullSafe());
                ffmpeg.WaitForExit();
            });
            Status.Text = "Done";
            TaskbarProgress.ProgressState = TaskbarItemProgressState.None;
            RefreshDirectory_Click(sender, eventArgs);
            await Task.Delay(2000);
            Status.Text = string.Empty;
        }

        private void Volume_Change(object? sender, RoutedPropertyChangedEventArgs<double> eventArgs)
        {
            Media.Volume = (Math.Exp(eventArgs.NewValue) - 1) / (Math.E - 1);
        }

        private void ShowMessage(string message)
        {
            TaskDialogHelper.ShowInfoTaskDialog(this, message);
        }
        
        private async Task<List<Tag>> LoadTags()
        {
            var etOptions = new ExifToolOptions
            {
                ExifToolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "exiftool.exe")
            };
            var et = new ExifTool(etOptions);

            _logger.Info($"Loading tags with ExifTool. Path: [{etOptions.ExifToolPath}].");
            _logger.Info(_loadedMedia.ToStringNullSafe());

            try
            {
                var result = await et.GetTagsAsync(_loadedMedia);
                return result.ToList();
            }
            catch (JsonReaderException ex)
            {
                _logger.Error(ex, "Failed to read tags.");
                return new List<Tag>();
            }
        }

        private void Media_MediaChanging(object? sender, MediaOpeningEventArgs e)
        {

            var audioStream = Media.MediaInfo.Streams
                .Where(kvp => kvp.Value.CodecType == AVMediaType.AVMEDIA_TYPE_AUDIO)
                .Where(kvp => kvp.Value.StreamIndex == _selectedAudioStream)
                .Select(kvp => kvp.Value);

            e.Options.AudioStream = audioStream.First();

        }

        private void ComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (AudioTrackSlider.SelectedItem == null) return;
            _selectedAudioStream = ((AudioTrack)AudioTrackSlider.SelectedItem).StreamIndex;
            Media.ChangeMedia();
        }

        private void MainWindow_OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string path = ((string[]) e.Data.GetData(DataFormats.FileDrop))[0];
                if (Directory.Exists(path))
                {
                    _selectedDirectory = path;
                }
                else if (File.Exists(path))
                {
                    // TODO: duplicated code -> to refactor.
                    _selectedDirectory = Path.GetDirectoryName(path);
                    if (path.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) ||
                        path.EndsWith(".mkv", StringComparison.OrdinalIgnoreCase))
                    {
                        Media.Open(new Uri(path));
                        _loadedMedia = path;
                    }
                }
                LoadDirectory();
            }
        }

        private void SkipBack_Click(object sender, RoutedEventArgs e)
        {
            Media.Position = Media.Position.Subtract(TimeSpan.FromSeconds(5));
        }

        private void SkipForward_Click(object sender, RoutedEventArgs e)
        {
            Media.Position = Media.Position.Add(TimeSpan.FromSeconds(5));
        }
    }
}
