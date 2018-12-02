using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
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

        public async void Loaded()
        {
            if (!TryLoadSecrets(@"..\..\..\spotify.secret"))
                ShowMessage("Spotify secrets couldnt be loaded");

            spotify = new Spotify(spotifySecrets.ClientID, spotifySecrets.ClientSecret);
            var currentPlaybackInfo = await spotify.GetCurrentPlaybackInfoAsync();
            CurrentSong = currentPlaybackInfo.Item.Name;
            CurrentArtist = currentPlaybackInfo.Item.Artists.First().Name;

            LoadPlaylists();
            //LoadCurrentSong();
        }

        private async void LoadPlaylists()
        {
            Playlists.Clear();

            var playlist = await spotify.GetPlaylistsAsync();
            Playlists.AddRange(playlist.Items);

            do
            {
                playlist = await spotify.GetNextPageAsync<Playlist, PlaylistItem>(playlist);
                Playlists.AddRange(playlist.Items);
            }
            while (playlist.HasNextPage);

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