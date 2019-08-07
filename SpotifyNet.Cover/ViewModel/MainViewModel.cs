using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json;
using SpotifyNet.Model.Tracks;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace SpotifyNet.Cover.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public int CoverWidth { get; set; }

        public int CoverHeight { get; set; }

        public BitmapImage Cover { get; set; }

        public ICommand LoadedCommand { get; set; }

        public ICommand StartResumeCommand { get; set; }

        public ICommand PreviousCommand { get; set; }

        public ICommand NextCommand { get; set; }

        private Spotify spotify;
        private SpotifySecrets spotifySecrets;

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
                StartResumeCommand = new RelayCommand(StartResume);
                PreviousCommand = new RelayCommand(Previous);
                NextCommand = new RelayCommand(Next);
            }
        }

        private bool TryLoadSecrets(string fileName)
        {
            try
            {
                if (!File.Exists(fileName))
                    return false;

                spotifySecrets = JsonConvert.DeserializeObject<SpotifySecrets>(File.ReadAllText(fileName));

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
            if (!TryLoadSecrets(@"..\..\..\spotify.secret"))
            {
                ShowMessage("Spotify secrets couldnt be loaded");
                return;
            }

            spotify = new Spotify(spotifySecrets.ClientID, spotifySecrets.ClientSecret);
            await RefreshSong();
        }

        public async Task RefreshSong()
        {
            var currentSong = await spotify.GetCurrentlyPlayingAsync();

            SetCover(currentSong?.Item);
        }

        private void SetCover(Track currentSong)
        {
            if (currentSong == null)
                return;

            var width = currentSong.Album.Images.Max(x => x.Width);
            var cover = currentSong.Album.Images.FirstOrDefault(x => x.Width == width);

            CoverWidth = cover.Width ?? CoverWidth;
            CoverHeight = cover.Height ?? CoverHeight;
            Cover = new BitmapImage(new Uri(cover.Url));
        }

        private async void StartResume()
        {
            var status = await spotify?.GetCurrentPlaybackInfoAsync();

            if (status.IsPlaying)
                await spotify?.PausePlayback();
            else
                await spotify?.StartResumePlayback();

            SetCover(status.Item);
        }

        private async void Previous()
        {
            await spotify?.SkipPlaybackToPrevious();
            await RefreshSong();
        }

        private async void Next()
        {
            await spotify?.SkipPlaybackToNext();
            await RefreshSong();
        }

        private void ShowMessage(string message)
        {
            Debug.WriteLine(message);
        }
    }
}