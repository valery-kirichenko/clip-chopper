using CommunityToolkit.WinUI.Helpers;
using FlyleafLib;
using FlyleafLib.MediaPlayer;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.ViewManagement;

using Color = Windows.UI.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using System.ComponentModel;
using MediaInfo;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.ObjectModel;
using FFMpegCore;
using Windows.ApplicationModel.DataTransfer;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ClipChopper.WinUIApp
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public Player Player { get; set; }
        private const int WAINACTIVE = 0x00;
        private const int WAACTIVE = 0x01;
        private const int WMACTIVATE = 0x0006;
        UISettings settings = new();
        SymbolIcon iconPlay = new SymbolIcon(Symbol.Play);
        SymbolIcon iconPause = new SymbolIcon(Symbol.Pause);
        private FragmentSelection? _fragment;
        public ObservableCollection<Tuple<int, string>> AudioTracks { get; } = new();
        private StorageFolder? _selectedFolder;
        private StorageFile? _selectedFile;

        public MainWindow()
        {
            GlobalFFOptions.Configure(new FFOptions { BinaryFolder = @"C:\Flyleaf\FFmpeg" });
            // Initializes Engine (Specifies FFmpeg libraries path which is required)
            Engine.Start(new EngineConfig()
            {
                #if DEBUG
                LogOutput = ":debug",
                LogLevel = LogLevel.Debug,
                FFmpegLogLevel = FFmpegLogLevel.Warning,
                #endif

                UIRefresh = true, // For Activity Mode usage
                UICurTimePerSecond = false,
                UIRefreshInterval = 8,
                FFmpegPath = @"C:\Flyleaf\FFmpeg",
                FFmpegDevices = false,
            });
            Config config = new Config();
            Color color = ((SolidColorBrush)Application.Current.Resources["ApplicationPageBackgroundThemeBrush"]).Color;
            config.Video.BackgroundColor = (System.Windows.Media.Color)ColorConverter.ConvertFromString(color.ToHex());
            config.Player.SeekAccurate = true;
            config.Player.AutoPlay = false;
            Player = new Player(config);
            Player.Audio.Volume = 100;
            Player.PropertyChanged += Player_PropertyChanged;
            Player.OpenCompleted += Player_OpenCompleted;


            this.InitializeComponent();
            this.ExtendsContentIntoTitleBar = true;
            this.SetTitleBar(AppTitleBar);
            this.Activated += MainWindow_Activated;

            AudioTracks.CollectionChanged += AudioTracks_CollectionChanged;

            settings.ColorValuesChanged += Listener_ThemeChanged;
            RootGrid.DataContext = this;

            GridSplitter.Element = new TextBlock
            {
                FontFamily = new FontFamily("Segoe MDL2 Assets"),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Text = "\xE784",
            };
        }

        private void AudioTracks_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            AudioTrackSelector.Width = Double.NaN;
            double maxWidth = 0;
            TextBlock textBlock = new();
            foreach (var obj in AudioTrackSelector.Items)
            {
                textBlock.Text = obj.ToString();
                textBlock.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                if (textBlock.DesiredSize.Width > maxWidth)
                {
                    maxWidth = textBlock.DesiredSize.Width;
                }
            }
            AudioTrackSelector.Width = maxWidth > 0 ? maxWidth + 32 : Double.NaN;
        }

        private void Player_OpenCompleted(object? sender, OpenCompletedArgs e)
        {
            if (!Player.CanPlay)
            {
                return;
            }
            if (Player.Audio.Streams.Count > 0)
            {
                var media = new MediaInfoWrapper(Player.Playlist.Selected.Url, NullLogger.Instance);
                DispatcherQueue.TryEnqueue(() =>
                {
                    var i = -1;
                    foreach (var stream in Player.Audio.Streams)
                    {
                        i++;
                        var candidates = new[] { media.AudioStreams[i].Name, media.AudioStreams[i].Tags.Title };
                        var name = candidates.Where(candidate => !string.IsNullOrWhiteSpace(candidate)).FirstOrDefault("Audio Track " + (i + 1));
                        if (name.StartsWith("SoundHandle / "))
                        {
                            name = name.Substring(14);
                        }
                        AudioTracks.Add(new Tuple<int, string>(i, name.Trim()));
                    }
                    AudioTrackSelector.SelectedIndex = 0;
                });
            }
            _fragment = new FragmentSelection(Player.Duration);
            _fragment.PropertyChanged += Fragment_PropertyChanged;
            UpdateFragmentRange();

        }

        private void Fragment_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            UpdateFragmentRange();
        }

        private void UpdateFragmentRange()
        {
            if (_fragment == null || !Player.CanPlay)
            {
                return;
            }
            DispatcherQueue.TryEnqueue(() =>
            {
                var width = Math.Min(FragmentRangeBackground.ActualWidth, (_fragment.Stop - _fragment.Start) * FragmentRangeBackground.ActualWidth / _fragment.Duration);
                var marginValue = Math.Min(FragmentRangeBackground.ActualWidth - width, _fragment.Start * FragmentRangeBackground.ActualWidth / _fragment.Duration);
                FragmentRange.Width = width;
                var margin = FragmentRange.Margin;
                margin.Left = marginValue;
                FragmentRange.Margin = margin;
            });
        }

        private void Player_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Player.Status):
                    PlayButton.Content = Player.Status == Status.Paused ? iconPlay : iconPause;
                    break;
            }
        }

        private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            var resource = args.WindowActivationState == WindowActivationState.Deactivated ? "WindowCaptionForegroundDisabled" : "WindowCaptionForeground";
            AppTitleTextBlock.Foreground = (SolidColorBrush)App.Current.Resources[resource];
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, IntPtr lParam);

        private void Listener_ThemeChanged(UISettings sender, object args)
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

            if (hwnd == GetActiveWindow())
            {
                SendMessage(hwnd, WMACTIVATE, WAACTIVE, IntPtr.Zero);
            }
            else
            {
                SendMessage(hwnd, WMACTIVATE, WAINACTIVE, IntPtr.Zero);
            }
            DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.High,
            () =>
            {
                Color color = ((SolidColorBrush)Application.Current.Resources["ApplicationPageBackgroundThemeBrush"]).Color;
                Player.Config.Video.BackgroundColor = (System.Windows.Media.Color)ColorConverter.ConvertFromString(color.ToHex());
            });
        }

        private async void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker openPicker = new();
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            openPicker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
            openPicker.FileTypeFilter.Add("*");

            StorageFolder folder = await openPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                _selectedFolder = folder;
                DirectoryList.ItemsSource = await folder.GetFilesAsync();
            }
        }

        private async void DirectoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count < 1) {
                return;
            }
            _selectedFile = e.AddedItems[0] as StorageFile;
            bool fileExists = (await _selectedFolder.TryGetItemAsync(_selectedFile.Name)) != null;
            if (!fileExists)
            {
                _selectedFile = null;

                ContentDialog dialog = new ContentDialog()
                {
                    Title = "Error",
                    Content = "Selected file does not exist",
                    CloseButtonText = "Ok",
                    XamlRoot = RootGrid.XamlRoot,
                };
                await dialog.ShowAsync();
                RefreshFolder();
                return;
            }
            AudioTracks.Clear();
            Player.OpenAsync(_selectedFile.Path);
        }

        private void StartFragmentButton_Click(object sender, RoutedEventArgs e)
        {
            _fragment.Start = Player.CurTime;
        }

        private void EndFragmentButton_Click(object sender, RoutedEventArgs e)
        {
            _fragment.Stop = Player.CurTime;
        }

        private void FragmentRangeBackground_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateFragmentRange();
        }

        private void AudioTrackSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count < 1) { return; }
            Player.Open(Player.Audio.Streams[(int) AudioTrackSelector.SelectedValue]);
        }

        private void SeekFrameBackwardButton_Click(object sender, RoutedEventArgs e)
        {
            Player.ShowFramePrev();
            Player.ReversePlayback = false; // Workaround for the Flyleaf bug: ShowFramePrev does not restore normal playback direction
        }

        private async void TakeSnapshotButton_Click(object sender, RoutedEventArgs e)
        {
            var savePicker = new FileSavePicker();
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hWnd);
            savePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            savePicker.SuggestedFileName = (DirectoryList.SelectedItem as StorageFile).DisplayName;
            savePicker.FileTypeChoices.Add("PNG image", new List<string>() { ".png" });
            savePicker.FileTypeChoices.Add("JPG image", new List<string>() { ".jpg" });
            savePicker.FileTypeChoices.Add("BMP image", new List<string>() { ".bmp" });
            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                Player.TakeSnapshotToFile(file.Path);
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var savePicker = new FileSavePicker();
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hWnd);
            savePicker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
            savePicker.SuggestedFileName = (DirectoryList.SelectedItem as StorageFile).DisplayName;
            savePicker.FileTypeChoices.Add("MP4 video", new List<string>() { ".mp4" });
            savePicker.FileTypeChoices.Add("MKV video", new List<string>() { ".mkv" });
            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file == null)
            {
                return;
            }

            var arguments = FFMpegArguments
                .FromFileInput((DirectoryList.SelectedItem as StorageFile).Path, false, options => options
                    .Seek(new TimeSpan(_fragment.Start))
                    .EndSeek(new TimeSpan(_fragment.Stop))
                 )
                .MapMetaData(0)
                .OutputToFile(file.Path, true, options => options
                    .WithCustomArgument("-c copy")
                    .SelectStreams(Player.Video.Streams.Select(stream => stream.StreamIndex))
                    .SelectStreams(Player.Audio.Streams.Select(stream => stream.StreamIndex))
                    .SelectStreams(Player.Subtitles.Streams.Select(stream => stream.StreamIndex))
                );
            Debug.WriteLine(arguments.Arguments);
            SaveButton.IsEnabled = false;
            await arguments.ProcessAsynchronously();
            SaveButton.IsEnabled = true;
        }

        private void SortItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem selectedItem)
            {
                Debug.WriteLine(selectedItem.Tag.ToString());
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshFolder();
        }

        private async void RefreshFolder()
        {
            if (_selectedFolder != null)
            {
                DirectoryList.ItemsSource = await _selectedFolder.GetFilesAsync();
            }
        }

        private async void Grid_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems)) {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items != null && items.Count > 0) {
                    switch (items[0])
                    {
                        case StorageFile file:
                            _selectedFolder = await file.GetParentAsync();
                            var files = await _selectedFolder.GetFilesAsync();
                            DirectoryList.ItemsSource = files;
                            _selectedFile = file;
                            DirectoryList.SelectedItem = _selectedFile;
                            Player.OpenAsync(file.Path);
                            break;
                        case StorageFolder folder:
                            _selectedFolder = folder;
                            DirectoryList.ItemsSource = await folder.GetFilesAsync();
                            break;
                    }
                }
            }
        }

        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
            e.DragUIOverride.Caption = "Open";
        }

        private async void AdvancedSave_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog();
            dialog.XamlRoot = RootGrid.XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = "Advanced Save";
            dialog.PrimaryButtonText = "Save";
            dialog.CloseButtonText = "Cancel";
            dialog.DefaultButton = ContentDialogButton.Primary;
            dialog.Content = new AdvancedSaveContent();

            var result = await dialog.ShowAsync();
            if (result != ContentDialogResult.Primary) {
                return;
            }

        }
    }
}
