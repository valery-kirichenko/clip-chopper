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
using System.IO;
using Microsoft.Win32;
using Microsoft.UI.Xaml.Input;
using Windows.System;

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
        private const int ICON_SMALL = 0;
        private const int ICON_BIG = 1;
        private const int ICON_SMALL2 = 2;
        private const int WM_GETICON = 0x007F;
        private const int WM_SETICON = 0x0080;
        UISettings settings = new();
        SymbolIcon iconPlay = new SymbolIcon(Symbol.Play);
        SymbolIcon iconPause = new SymbolIcon(Symbol.Pause);
        private FragmentSelection? _fragment;
        public ObservableCollection<Tuple<int, string>> AudioTracks { get; } = new();
        private FileInfo? SelectedFile { get; set; }
        private DirectoryInfo? _selectedFolder;
        private DirectoryInfo? SelectedFolder
        {
            get => _selectedFolder;
            set {
                _selectedFolder = value;
                if (_selectedFolder != null)
                {
                    using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\ClipChopper"))
                    {
                        key.SetValue("PreviousPath", _selectedFolder.FullName);
                    }
                    RefreshFolder();
                }
            }
        }

        public MainWindow()
        {
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            string sExe = Process.GetCurrentProcess().MainModule.FileName;
            System.Drawing.Icon ico = System.Drawing.Icon.ExtractAssociatedIcon(sExe);
            SendMessage(hWnd, WM_SETICON, ICON_BIG, ico.Handle);

            GlobalFFOptions.Configure(new FFOptions { BinaryFolder = Path.Combine(AppContext.BaseDirectory, "FFmpeg") });
            // Initializes Engine (Specifies FFmpeg libraries path which is required)
            Engine.Start(new EngineConfig()
            {
                #if DEBUG
                LogOutput = @"C:\Flyleaf\log.txt",
                LogLevel = LogLevel.Debug,
                FFmpegLogLevel = FFmpegLogLevel.Debug,
                #endif

                UIRefresh = true, // For Activity Mode usage
                UICurTimePerSecond = false,
                UIRefreshInterval = 8,
                FFmpegPath = Path.Combine(AppContext.BaseDirectory, "FFmpeg"),
                FFmpegDevices = false,
            });
            Config config = new Config();
            Color color = ((SolidColorBrush)Application.Current.Resources["ApplicationPageBackgroundThemeBrush"]).Color;
            config.Video.BackgroundColor = (System.Windows.Media.Color)ColorConverter.ConvertFromString(color.ToHex());
            config.Player.SeekAccurate = true;
            config.Player.AutoPlay = false;
            config.Decoder.AllowProfileMismatch = true;
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

            KeyboardAccelerator ka1 = new KeyboardAccelerator();
            ka1.Key = (VirtualKey)188;
            SeekFrameBackwardButton.KeyboardAcceleratorPlacementMode = KeyboardAcceleratorPlacementMode.Hidden;            
            SeekFrameBackwardButton.KeyboardAccelerators.Add(ka1);

            KeyboardAccelerator ka2 = new KeyboardAccelerator();
            ka2.Key = (VirtualKey)190;
            SeekFrameForwardButton.KeyboardAcceleratorPlacementMode = KeyboardAcceleratorPlacementMode.Hidden;
            SeekFrameForwardButton.KeyboardAccelerators.Add(ka2);

            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\ClipChopper"))
            {
                if (key.GetValue("PreviousPath") is string previousPath && Directory.Exists(previousPath))
                {
                    SelectedFolder = new DirectoryInfo(previousPath);
                }
            }
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
            DispatcherQueue.TryEnqueue(() =>
            {
                FragmentRange.Width = 0;
            });
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
            if (folder == null)
            {
                return;
            }
            SelectedFolder = new DirectoryInfo(folder.Path);
        }

        private async void DirectoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count < 1) {
                return;
            }
            var NewFile = e.AddedItems[0] as FileInfo;
            if (NewFile == null)
            {
                return;
            }
            NewFile.Refresh();
            if (!NewFile.Exists)
            {
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
            if (NewFile.FullName.Equals(SelectedFile?.FullName))
            {
                return;
            }
            SelectedFile = NewFile;            
            AudioTracks.Clear();
            Player.OpenAsync(SelectedFile.FullName);
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
            savePicker.SuggestedFileName = (DirectoryList.SelectedItem as FileInfo).Name;
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
            savePicker.SuggestedFileName = "Chopped " + SelectedFile.Name;
            savePicker.FileTypeChoices.Add("MP4 video", new List<string>() { ".mp4" });
            savePicker.FileTypeChoices.Add("MKV video", new List<string>() { ".mkv" });
            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file == null)
            {
                return;
            }

            var arguments = FFMpegArguments
                .FromFileInput(SelectedFile.FullName, false, options => options
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
            if (SelectedFolder == null)
            {
                return;
            }
            DirectoryList.ItemsSource = SelectedFolder.EnumerateFiles();
            SelectedFile?.Refresh();
            if (SelectedFile != null && !SelectedFile.Exists)
            {
                SelectedFile = null;
            }
            if (SelectedFile != null && (SelectedFile.Directory?.FullName.Equals(SelectedFolder.FullName) ?? false))
            {
                DirectoryList.SelectedIndex = DirectoryList.Items.TakeWhile(x => !SelectedFile.FullName.Equals(((FileInfo)x).FullName)).Count();
            }
        }

        private async void Grid_Drop(object sender, DragEventArgs e)
        {
            if (!e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                return;
            }
            var items = await e.DataView.GetStorageItemsAsync();
            if (items == null || items.Count <= 0)
            {
                return;
            }
            switch (items[0])
            {
                case StorageFile file:
                    SelectedFolder = new DirectoryInfo((await file.GetParentAsync()).Path);
                    SelectedFile = new FileInfo(file.Path);
                    break;
                case StorageFolder folder:
                    SelectedFolder = new DirectoryInfo(folder.Path);
                    break;
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


        private void VolumeSlider_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            // There's probably a better way to differentiate between mouse scroll wheel (for which we want to do increments of 5 for each wheel step)
            // and touch pad input (we want smooth increase of volume slider), but for now it is what it is
            // TBH, I don't even know if this event will fire for touch pad two finger scrolling
            Player.Audio.Volume += e.GetCurrentPoint((UIElement)sender).Properties.MouseWheelDelta / 24;
        }
    }
}
