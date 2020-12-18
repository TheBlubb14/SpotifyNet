using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json;
using SpotifyNet.Cover.Model;
using SpotifyNet.Model.Player;
using SpotifyNet.Model.Tracks;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace SpotifyNet.Cover.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public int CoverWidth { get; set; }

        public int CoverHeight { get; set; }

        public BitmapImage Cover { get; set; }

        public bool IsPrivateSession { get; set; }

        public ICommand LoadedCommand { get; set; }

        public ICommand MouseWheelCommand { get; set; }

        public ICommand StartResumeCommand { get; set; }

        public ICommand PreviousCommand { get; set; }

        public ICommand NextCommand { get; set; }

        public string CurrentVolumeString { get; set; }

        private Spotify spotify;
        private SpotifySecrets spotifySecrets;
        private DispatcherTimer refreshTimer;

        private CurrentPlaybackInfo status;
        private Track currentTrack;
        private int? currentVolume
        {
            get => _currentVolume;
            set
            {
                _currentVolume = value;
                CurrentVolumeString = value?.ToString() ?? "";
            }
        }
        private int? _currentVolume;
        private const int VOLUME_STEPS = 10;

        bool Designer = false;

        // TODO: Implement https://developer.spotify.com/documentation/general/guides/authorization-guide/#authorization-code-flow-with-proof-key-for-code-exchange-pkce
        public MainViewModel()
        {
            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
            }
            else
            {
                // Code runs "for real"
                LoadedCommand = new RelayCommand(Loaded);
                MouseWheelCommand = new RelayCommand<MouseWheelEventArgs>(MouseWheel);
                StartResumeCommand = new RelayCommand(StartResume);
                PreviousCommand = new RelayCommand(Previous);
                NextCommand = new RelayCommand(Next);
            }
        }

        private async void MouseWheel(MouseWheelEventArgs e)
        {
            if (Designer)
                return;

            await RefreshIfRequired();

            // Return if no volume information
            if (currentVolume is null)
                return;

            var targetVolume = currentVolume.Value + ((e.Delta < 0 ? -1 : 1) * VOLUME_STEPS);
            targetVolume = targetVolume.Clamp(0, 100);

            // Return if same value
            if (currentVolume == targetVolume)
                return;

            // Provide visual feedback
            currentVolume = targetVolume;

            await spotify?.SetPlaybackVolume(targetVolume);
        }

        private bool TryLoadSecrets(string fileName)
        {
            try
            {
                var fileInfo = new FileInfo(fileName);

                if (!File.Exists(fileName))
                {
                    if (Debugger.IsAttached)
                        return false;

                    // Create default secret file and notify user about missing secrets 
                    // if the code is running without debugger
                    fileInfo = new FileInfo("spotify.secret");
                    if (!fileInfo.Exists)
                    {
                        File.WriteAllText(fileInfo.FullName, JsonConvert.SerializeObject(new SpotifySecrets("", "")));
                        MessageBox.Show($"Please fill in your Spotify secrets into the newly generated file at {fileInfo.FullName}", "Could not find Spotify secret", MessageBoxButton.OK);
                        Environment.Exit(0);
                    }
                }

                spotifySecrets = JsonConvert.DeserializeObject<SpotifySecrets>(File.ReadAllText(fileInfo.FullName));

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }

        public async void Loaded()
        {
            if (Designer)
                return;

            if (!TryLoadSecrets(@"..\..\..\..\spotify.secret"))
            {
                ShowMessage("Spotify secrets couldnt be loaded");
                return;
            }

            spotify = new Spotify(spotifySecrets.ClientID, spotifySecrets.ClientSecret);

            refreshTimer = new DispatcherTimer(DispatcherPriority.Background);
            refreshTimer.Interval = TimeSpan.FromSeconds(5);
            refreshTimer.Tick += RefreshTimer_Tick;
            refreshTimer.Start();

            await Refresh().ConfigureAwait(false);
        }

        private async void RefreshTimer_Tick(object sender, EventArgs e)
        {
            await Refresh().ConfigureAwait(false);
        }

        private async Task RefreshIfRequired()
        {
            if (status is null)
                await Refresh();
        }

        private async Task Refresh()
        {
            status = await spotify.GetCurrentPlaybackInfoAsync();

            // In private session we dont get track informations
            IsPrivateSession = status?.Device?.IsPrivateSession ?? false;

            if (currentTrack == status?.Item)
                return;

            currentTrack = status?.Item;
            currentVolume = status?.Device?.VolumePercent;

            SetCover(currentTrack);
        }

        private void SetCover(Track currentSong)
        {
            if (currentSong == null)
                return;

            var width = currentSong.Album.Images.Max(x => x.Width);
            var cover = Array.Find(currentSong.Album.Images, x => x.Width == width);

            CoverWidth = cover.Width ?? CoverWidth;
            CoverHeight = cover.Height ?? CoverHeight;
            Cover = new BitmapImage(new Uri(cover.Url));
        }

        private async void StartResume()
        {
            if (Designer)
                return;

            status = await spotify?.GetCurrentPlaybackInfoAsync();

            if (status.IsPlaying)
                await spotify?.PausePlayback();
            else
                await spotify?.StartResumePlayback();

            SetCover(status.Item);
        }

        private async void Previous()
        {
            if (Designer)
                return;

            await spotify?.SkipPlaybackToPrevious();
            await Refresh().ConfigureAwait(false);
        }

        private async void Next()
        {
            if (Designer)
                return;

            await spotify?.SkipPlaybackToNext();
            await Refresh().ConfigureAwait(false);
        }

        private void ShowMessage(string message)
        {
            Debug.WriteLine(message);
        }
    }
}