using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Threading;

namespace Esercizio_23_Sciarfy
{
    public partial class FavouriteWindow : Window
    {
        // For Main player
        bool playMain = false;
        DispatcherTimer mainTimer;
        bool isMainSeeking;
        Thread seekMainThread;

        public FavouriteWindow()
        {
            InitializeComponent();

            // Setting up the favourite list
            List<Brano> Favourites = new List<Brano>();
            foreach (Brano brano in ((MainWindow)Application.Current.MainWindow).SongDatabase)
                if(brano.Favourite)
                    Favourites.Add(brano);

            Favourite_ListBox.ItemsSource = Favourites;

            // Reset of Playback bar
            MainSongCover.Source = null;
            MainSongTitle.Text = "No song yet";
            MainSongArtist.Text = "-";
            MainProgressBar.Value = 0;

            // MainPlayer Timers Setup
            mainTimer = new DispatcherTimer();
            mainTimer.Interval = TimeSpan.FromMilliseconds(100);
            mainTimer.Tick += MainTimer_Tick;
        }

        // Library page
        private void GoLibraryPageBTN_Click(object sender, RoutedEventArgs e)
        {
            playMain = false;
            PlayMainIcon.Source = new BitmapImage(new Uri("/images/playIcon.png", UriKind.Relative));
            MainPlayer.Pause();
            mainTimer.Stop();

            ((MainWindow)Application.Current.MainWindow).Show();
            this.Close();
        }


        // =================== MAIN PLAYER ===================

        private void Favourite_ListBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            // Start new selected song
            if (Favourite_ListBox.SelectedItem != null) // in case the player is playing and the user change the playlist, so the SlectedItem is null
            {
                // Check for favourite icon
                if ((Favourite_ListBox.SelectedItem as Brano).Favourite)
                    FavouriteIcon.Source = new BitmapImage(new Uri("/images/favouriteYES.png", UriKind.Relative));
                else
                    FavouriteIcon.Source = new BitmapImage(new Uri("/images/favouriteNO.png", UriKind.Relative));

                MainPlayer.Source = new Uri((Favourite_ListBox.SelectedItem as Brano).PathSong, UriKind.Relative);
                MainSongCover.Source = new BitmapImage(new Uri((Favourite_ListBox.SelectedItem as Brano).PathIMG, UriKind.Relative));
                MainSongTitle.Text = (Favourite_ListBox.SelectedItem as Brano).Titolo;
                MainSongArtist.Text = (Favourite_ListBox.SelectedItem as Brano).Autore;

                playMain = true;
                PlayMainIcon.Source = new BitmapImage(new Uri("/images/pauseIcon.png", UriKind.Relative));
                MainPlayer.Play();
                mainTimer.Start();
            }
        }

        private void PlayMain_Click(object sender, RoutedEventArgs e)
        {
            if (playMain == false)
            {
                playMain = true;
                PlayMainIcon.Source = new BitmapImage(new Uri("/images/pauseIcon.png", UriKind.Relative));
                MainPlayer.Play();
                mainTimer.Start();
            }
            else
            {
                playMain = false;
                PlayMainIcon.Source = new BitmapImage(new Uri("/images/playIcon.png", UriKind.Relative));
                MainPlayer.Pause();
                mainTimer.Stop();
            }
        }

        private void MainPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            // Set the maximum value of the progress bar to the duration of the song
            double songDuration = MainPlayer.NaturalDuration.TimeSpan.TotalSeconds;
            MainProgressBar.Maximum = songDuration;
        }

        private void MainPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            // Reset the progress bar when the song finishes playing
            MainProgressBar.Value = 0;

            // Go to the next song
            if (Favourite_ListBox.SelectedItem != null)
            {
                if (Favourite_ListBox.SelectedIndex == (Favourite_ListBox.ItemsSource as List<Brano>).Count - 1)
                    Favourite_ListBox.SelectedIndex = 0;
                else
                    Favourite_ListBox.SelectedIndex++;

                MainPlayer.Source = new Uri((Favourite_ListBox.SelectedItem as Brano).PathSong, UriKind.Relative);
                MainSongCover.Source = new BitmapImage(new Uri((Favourite_ListBox.SelectedItem as Brano).PathIMG, UriKind.Relative));
                MainSongTitle.Text = (Favourite_ListBox.SelectedItem as Brano).Titolo;
                MainSongArtist.Text = (Favourite_ListBox.SelectedItem as Brano).Autore;
            }
        }

        private void MainProgressBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Start a background thread to handle seeking
            seekMainThread = new Thread(() =>
            {
                isMainSeeking = true;
                while (isMainSeeking)
                {
                    // Update the progress bar value
                    Dispatcher.Invoke(() =>
                    {
                        Point mousePos = Mouse.GetPosition(MainProgressBar);
                        double songPosition = mousePos.X / MainProgressBar.ActualWidth * MainProgressBar.Maximum;
                        MainProgressBar.Value = songPosition;
                    });
                    Thread.Sleep(100);
                }
            });
            seekMainThread.Start();
        }

        private void MainProgressBar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Stop the background thread and update the position of the song
            isMainSeeking = false;
            seekMainThread.Join();
            double songPosition = MainProgressBar.Value;
            MainPlayer.Position = TimeSpan.FromSeconds(songPosition);
        }

        private void MainTimer_Tick(object sender, EventArgs e)
        {
            // Update the progress bar value
            if (!isMainSeeking)
            {
                MainProgressBar.Value = MainPlayer.Position.TotalSeconds;
            }
        }

        private void BackSong_Click(object sender, RoutedEventArgs e)
        {
            if (Favourite_ListBox.SelectedItem != null)
            {
                // 1) Stop and reset the player
                MainPlayer.Stop();
                mainTimer.Stop();
                MainProgressBar.Value = 0;
                playMain = false;

                // 2) Previous song
                if (Favourite_ListBox.SelectedIndex == 0)
                    Favourite_ListBox.SelectedIndex = (Favourite_ListBox.ItemsSource as List<Brano>).Count - 1;
                else
                    Favourite_ListBox.SelectedIndex--;

                MainPlayer.Source = new Uri((Favourite_ListBox.SelectedItem as Brano).PathSong, UriKind.Relative);
                MainSongCover.Source = new BitmapImage(new Uri((Favourite_ListBox.SelectedItem as Brano).PathIMG, UriKind.Relative));
                MainSongTitle.Text = (Favourite_ListBox.SelectedItem as Brano).Titolo;
                MainSongArtist.Text = (Favourite_ListBox.SelectedItem as Brano).Autore;
            }
        }

        private void NextSong_Click(object sender, RoutedEventArgs e)
        {
            if (Favourite_ListBox.SelectedItem != null)
            {
                // 1) Stop and reset the player
                MainPlayer.Stop();
                mainTimer.Stop();
                MainProgressBar.Value = 0;
                playMain = false;

                // 2) Next song
                if (Favourite_ListBox.SelectedIndex == (Favourite_ListBox.ItemsSource as List<Brano>).Count - 1)
                    Favourite_ListBox.SelectedIndex = 0;
                else
                    Favourite_ListBox.SelectedIndex++;

                MainPlayer.Source = new Uri((Favourite_ListBox.SelectedItem as Brano).PathSong, UriKind.Relative);
                MainSongCover.Source = new BitmapImage(new Uri((Favourite_ListBox.SelectedItem as Brano).PathIMG, UriKind.Relative));
                MainSongTitle.Text = (Favourite_ListBox.SelectedItem as Brano).Titolo;
                MainSongArtist.Text = (Favourite_ListBox.SelectedItem as Brano).Autore;
            }
        }

        private void FavouriteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Favourite_ListBox.SelectedItem != null)
            {
                if ((Favourite_ListBox.SelectedItem as Brano).Favourite)
                {
                    FavouriteIcon.Source = new BitmapImage(new Uri("/images/favouriteNO.png", UriKind.Relative));
                    (Favourite_ListBox.SelectedItem as Brano).Favourite = false;
                    Favourite_ListBox.Items.Refresh();
                }
                else
                {
                    FavouriteIcon.Source = new BitmapImage(new Uri("/images/favouriteYES.png", UriKind.Relative));
                    (Favourite_ListBox.SelectedItem as Brano).Favourite = true;
                    Favourite_ListBox.Items.Refresh();
                }
            }
        }

        
    }
}