using Newtonsoft.Json;
using SpotifyNet.Model.PlaylistData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using System.Windows.Shapes;

namespace SpotifyNet.Demo
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Spotify spotifyNet;
        SpotifySecrets spotifySecrets;

        public MainWindow()
        {
            InitializeComponent();
        }

        private List<PlaylistItem> PlaylistItems = new List<PlaylistItem>();

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            spotifySecrets = JsonConvert.DeserializeObject<SpotifySecrets>(File.ReadAllText(@"..\..\..\spotify.secret"));
            spotifyNet = new Spotify(spotifySecrets.ClientID, spotifySecrets.ClientSecret);

            var playlist = await spotifyNet.GetPlaylists();
            PlaylistItems.AddRange(playlist.Items);

            do
            {
                playlist = await spotifyNet.GetNextPageAsync<Playlist, PlaylistItem>(playlist);
                PlaylistItems.AddRange(playlist.Items);
            }
            while (playlist.HasNextPage);

            ListBoxPlaylist.ItemsSource = PlaylistItems;
            ListBoxPlaylist.DisplayMemberPath = nameof(PlaylistItem.Name);
            //var res = await spotifyNet.GetMeAsync();
        }
    }
}
