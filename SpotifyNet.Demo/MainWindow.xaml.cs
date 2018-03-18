﻿using Newtonsoft.Json;
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            spotifySecrets = JsonConvert.DeserializeObject<SpotifySecrets>(File.ReadAllText(@"..\..\..\spotify.secret"));
            spotifyNet = new Spotify(spotifySecrets.ClientID, spotifySecrets.ClientSecret);
            var d = 1;
            spotifyNet.test();
            var ddd = 1;
        }
    }
}
