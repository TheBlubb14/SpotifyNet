using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json;
using SpotifyNet.Model.Playlists.PlaylistData;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;

namespace SpotifyNet.Demo.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<PlaylistItem> Playlists { get; set; }

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
            LoadPlaylists();
        }

        private async void LoadPlaylists()
        {
            Playlists.Clear();

            var playlist = await spotify.GetPlaylists();
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