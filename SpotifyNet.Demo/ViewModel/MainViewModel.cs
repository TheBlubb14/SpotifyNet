using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json;
using SpotifyNet.Model.Playlists.PlaylistData;

namespace SpotifyNet.Demo.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<PlaylistItem> Playlists { get; set; }

        public string CurrentSong { get; set; }

        public BitmapImage CurrentSongCover { get; set; }

        public string CurrentArtist { get; set; }

        public ICommand LoadedCommand { get; set; }

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
                Playlists = new ObservableCollection<PlaylistItem>();
                LoadedCommand = new RelayCommand(Loaded);
            }
        }

        public void Loaded()
        {
            if (!TryLoadSecrets(@"..\..\..\spotify.secret"))
                ShowMessage("Spotify secrets couldnt be loaded");

            spotify = new Spotify(spotifySecrets.ClientID, spotifySecrets.ClientSecret);

            _ = LoadCurrentSong();
            _ = LoadPlaylists();
        }

        private async Task LoadPlaylists()
        {
            Playlists.Clear();

            var playlist = await spotify.GetPlaylistsAsync();

            if (playlist == null)
                return;

            Playlists.AddRange(playlist.Items);

            do
            {
                playlist = await spotify.GetNextPageAsync<Playlist, PlaylistItem>(playlist);
                Playlists.AddRange(playlist.Items);
            }
            while (playlist.HasNextPage);

        }

        private async Task LoadCurrentSong()
        {
            var currentSong = await spotify.GetCurrentlyPlayingAsync();

            if (currentSong == null)
                return;

            CurrentSong = currentSong.Item.Name;
            CurrentArtist = currentSong.Item.Artists.First().Name;

            var url = currentSong.Item.Album.Images.FirstOrDefault(x => x.Width == currentSong.Item.Album.Images.Max(y => y.Width)).Url;
            CurrentSongCover = new BitmapImage(new Uri(url));
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

        private void ShowMessage(string message)
        {
            Debug.WriteLine(message);
        }
    }
}