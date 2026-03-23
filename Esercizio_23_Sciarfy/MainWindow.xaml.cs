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
using System.Windows.Navigation;
using System.Media;
using System.IO;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Media.Animation;

namespace Esercizio_23_Sciarfy
{
    public partial class MainWindow : Window
    {
        public List<Playlist> AllPlaylist;
        public List<Brano> SongDatabase;
        FavouriteWindow favouriteWindow;

        // For suggested player
        bool playSuggested = false;
        DispatcherTimer timer;
        bool isSeeking;
        Thread seekThread;

        // For Main player
        bool playMain = false;
        DispatcherTimer mainTimer;
        bool isMainSeeking;
        Thread seekMainThread;

        public MainWindow()
        {
            InitializeComponent();
            // Setting up all songs
            SongDatabase = new List<Brano>()
            {
                new Brano("Wake Me Up", "Avicii", "4:07", "../../songs/avicii_wakemeup.mp3", "../../songs/covers/avicii_wakemeup_cover.jpg"),
                new Brano("Beautiful Day", "U2", "4:08", "../../songs/beautifulDay_u2.mp3", "../../songs/covers/beautifulDayCover_u2.jpg"),
                new Brano("Don't Leave Me Now", "Lost Frequencies", "3:15", "../../songs/dontLeaveMeNow_lostFrequencies.mp3", "../../songs/covers/dontLeaveMeNowCover_lostFrequencies.jpg"),
                new Brano("Don't Look Back in Anger", "Oasis", "4:49", "../../songs/dontLookBackInAnger_oasis.mp3", "../../songs/covers/dontLookBackInAngerCover_oasis.jpg"),
                new Brano("The Funeral", "Band of Horses", "5:22", "../../songs/theFuneral_bandOfHorses.mp3", "../../songs/covers/theFuneralCover_bandOfHorses.jpg"),
                new Brano("Viva La Vida", "Coldplay", "4:02", "../../songs/vivaLaVida_coldplay.mp3", "../../songs/covers/vivaLaVidaCover_coldplay.jpg"),
                new Brano("Fix You", "Coldplay", "4:55", "../../songs/fixYou_coldplay.mp3", "../../songs/covers/fixYouCover_coldplay.jpg"),
                new Brano("Yellow", "Coldplay", "4:26", "../../songs/yellow_coldplay.mp3", "../../songs/covers/yellowCover_coldplay.jpg"),
            };

            // Setting up all the playlists
            AllPlaylist = new List<Playlist>()
            {
                new Playlist("Night Vibes", "/playlists/playlist_1.jpg", new List<Brano>()
                    {
                        SongDatabase[0], SongDatabase[1], SongDatabase[2], SongDatabase[3], SongDatabase[4], SongDatabase[5]
                    }
                ),
                new Playlist("Morning Routine", "/playlists/playlist_2.jpg", new List<Brano>()
                    {
                        SongDatabase[4], SongDatabase[5], SongDatabase[0]
                    }
                ),
                new Playlist("Coldplay", "/playlists/playlist_3.jpg", new List<Brano>()
                    {
                       SongDatabase[5], SongDatabase[6], SongDatabase[7]
                    }
                ),
            };

            Playlist_ListBox.ItemsSource = AllPlaylist;


            // Suggested Song Timers Setup
            mediaElement.Source = new Uri("../../songs/suggested_v3.mp3", UriKind.Relative);
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += Timer_Tick;
            isSeeking = false;

            Song_ListBox.ItemsSource = AllPlaylist[0].ListaBrani;

            // Reset of Playback bar
            MainSongCover.Source = null;
            MainSongTitle.Text = "No song yet";
            MainSongArtist.Text = "-";
            MainProgressBar.Value = 0;

            // MainPlayer Timers Setup
            mainTimer = new DispatcherTimer();
            mainTimer.Interval = TimeSpan.FromMilliseconds(100);
            mainTimer.Tick += MainTimer_Tick;
            isMainSeeking = false;
        }

        // Favourite page
        private void GoFavouritePageBTN_Click(object sender, RoutedEventArgs e)
        {
            playSuggested = false;
            SuggestedPlayIcon.Source = new BitmapImage(new Uri("/images/playIcon.png", UriKind.Relative));
            mediaElement.Stop();
            timer.Stop();
            playMain = false;
            PlayMainIcon.Source = new BitmapImage(new Uri("/images/playIcon.png", UriKind.Relative));
            MainPlayer.Pause();
            mainTimer.Stop();

            favouriteWindow = new FavouriteWindow();
            favouriteWindow.Owner = this;

            favouriteWindow.Show();
            this.Hide();
        }

        // =================== SUGGESTED SONG PLAYER ===================
        private void PlaySuggested_Click(object sender, RoutedEventArgs e)
        {
            if (playSuggested == false)
            {
                playSuggested = true;
                SuggestedPlayIcon.Source = new BitmapImage(new Uri("/images/pauseIcon.png", UriKind.Relative));
                mediaElement.Play();
                timer.Start();

                // and stops the main player
                playMain = false;
                PlayMainIcon.Source = new BitmapImage(new Uri("/images/playIcon.png", UriKind.Relative));
                MainPlayer.Pause();
                mainTimer.Stop();

            }
            else
            {
                playSuggested = false;
                SuggestedPlayIcon.Source = new BitmapImage(new Uri("/images/playIcon.png", UriKind.Relative));
                mediaElement.Pause();
                timer.Stop();
            }
        }

        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            // Set the maximum value of the progress bar to the duration of the song
            double songDuration = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
            progressBar.Maximum = songDuration;
        }

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            // Reset the progress bar when the song finishes playing
            progressBar.Value = 0;
            playSuggested = false;
            SuggestedPlayIcon.Source = new BitmapImage(new Uri("/images/playIcon.png", UriKind.Relative));
            mediaElement.Stop();
            timer.Stop();
        }

        private void ProgressBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Start a background thread to handle seeking
            seekThread = new Thread(() =>
            {
                isSeeking = true;
                while (isSeeking)
                {
                    // Update the progress bar value
                    Dispatcher.Invoke(() =>
                    {
                        Point mousePos = Mouse.GetPosition(progressBar);
                        double songPosition = mousePos.X / progressBar.ActualWidth * progressBar.Maximum;
                        progressBar.Value = songPosition;
                    });
                    Thread.Sleep(100);
                }
            });
            seekThread.Start();
        }

        private void ProgressBar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Stop the background thread and update the position of the song
            isSeeking = false;
            seekThread.Join();
            double songPosition = progressBar.Value;
            mediaElement.Position = TimeSpan.FromSeconds(songPosition);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Update the progress bar value
            if (!isSeeking)
            {
                progressBar.Value = mediaElement.Position.TotalSeconds;
            }
        }


        // =================== MAIN PLAYER ===================
        private void Song_ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Start new selected song
            if(Song_ListBox.SelectedItem != null) // in case the player is playing and the user change the playlist, so the SlectedItem is null
            {
                // Check for favourite icon
                if((Song_ListBox.SelectedItem as Brano).Favourite)
                    FavouriteIcon.Source = new BitmapImage(new Uri("/images/favouriteYES.png", UriKind.Relative));
                else
                    FavouriteIcon.Source = new BitmapImage(new Uri("/images/favouriteNO.png", UriKind.Relative));

                MainPlayer.Source = new Uri((Song_ListBox.SelectedItem as Brano).PathSong, UriKind.Relative);
                MainSongCover.Source = new BitmapImage(new Uri((Song_ListBox.SelectedItem as Brano).PathIMG, UriKind.Relative));
                MainSongTitle.Text = (Song_ListBox.SelectedItem as Brano).Titolo;
                MainSongArtist.Text = (Song_ListBox.SelectedItem as Brano).Autore;


                playMain = true;
                PlayMainIcon.Source = new BitmapImage(new Uri("/images/pauseIcon.png", UriKind.Relative));
                MainPlayer.Play();
                mainTimer.Start();

                // and stops the suggested player
                playSuggested = false;
                SuggestedPlayIcon.Source = new BitmapImage(new Uri("/images/playIcon.png", UriKind.Relative));
                mediaElement.Pause();
                timer.Stop();
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

                // and stops the suggested player
                playSuggested = false;
                SuggestedPlayIcon.Source = new BitmapImage(new Uri("/images/playIcon.png", UriKind.Relative));
                mediaElement.Pause();
                timer.Stop();

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
            if (Song_ListBox.SelectedItem != null)
            {
                if (Song_ListBox.SelectedIndex == (Song_ListBox.ItemsSource as List<Brano>).Count - 1)
                    Song_ListBox.SelectedIndex = 0;
                else
                    Song_ListBox.SelectedIndex++;

                MainPlayer.Source = new Uri((Song_ListBox.SelectedItem as Brano).PathSong, UriKind.Relative);
                MainSongCover.Source = new BitmapImage(new Uri((Song_ListBox.SelectedItem as Brano).PathIMG, UriKind.Relative));
                MainSongTitle.Text = (Song_ListBox.SelectedItem as Brano).Titolo;
                MainSongArtist.Text = (Song_ListBox.SelectedItem as Brano).Autore;
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

        private void Playlist_ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Change the itemsource to the new playlist selected
            Song_ListBox.ItemsSource = (Playlist_ListBox.SelectedItem as Playlist).ListaBrani;
            Song_ListBox.Items.Refresh();
        }

        private void RandomPlaylistBTN_Click(object sender, RoutedEventArgs e)
        {
            Random r = new Random();
            int i = r.Next(0, AllPlaylist.Count);

            if(AllPlaylist[i] != null)
            {
                Song_ListBox.ItemsSource = AllPlaylist[i].ListaBrani;
                Song_ListBox.Items.Refresh();
            } 
        }

        private void BackSong_Click(object sender, RoutedEventArgs e)
        {
            if (Song_ListBox.SelectedItem != null)
            {
                // 1) Stop and reset the player
                MainPlayer.Stop();
                mainTimer.Stop();
                MainProgressBar.Value = 0;
                playMain = false;

                // 2) Previous song
                if (Song_ListBox.SelectedIndex == 0)
                    Song_ListBox.SelectedIndex = (Song_ListBox.ItemsSource as List<Brano>).Count - 1;
                else
                    Song_ListBox.SelectedIndex--;

                MainPlayer.Source = new Uri((Song_ListBox.SelectedItem as Brano).PathSong, UriKind.Relative);
                MainSongCover.Source = new BitmapImage(new Uri((Song_ListBox.SelectedItem as Brano).PathIMG, UriKind.Relative));
                MainSongTitle.Text = (Song_ListBox.SelectedItem as Brano).Titolo;
                MainSongArtist.Text = (Song_ListBox.SelectedItem as Brano).Autore;
            }
        }

        private void NextSong_Click(object sender, RoutedEventArgs e)
        {
            if (Song_ListBox.SelectedItem != null)
            {
                // 1) Stop and reset the player
                MainPlayer.Stop();
                mainTimer.Stop();
                MainProgressBar.Value = 0;
                playMain = false;

                // 2) Next song
                if (Song_ListBox.SelectedIndex == (Song_ListBox.ItemsSource as List<Brano>).Count - 1)
                    Song_ListBox.SelectedIndex = 0;
                else
                    Song_ListBox.SelectedIndex++;
                
                MainPlayer.Source = new Uri((Song_ListBox.SelectedItem as Brano).PathSong, UriKind.Relative);
                MainSongCover.Source = new BitmapImage(new Uri((Song_ListBox.SelectedItem as Brano).PathIMG, UriKind.Relative));
                MainSongTitle.Text = (Song_ListBox.SelectedItem as Brano).Titolo;
                MainSongArtist.Text = (Song_ListBox.SelectedItem as Brano).Autore;
            }
        }

        private void FavouriteBtn_Click(object sender, RoutedEventArgs e)
        {
            if(Song_ListBox.SelectedItem != null)
            {
                if ((Song_ListBox.SelectedItem as Brano).Favourite)
                {
                    FavouriteIcon.Source = new BitmapImage(new Uri("/images/favouriteNO.png", UriKind.Relative));
                    (Song_ListBox.SelectedItem as Brano).Favourite = false;
                }
                else
                {
                    FavouriteIcon.Source = new BitmapImage(new Uri("/images/favouriteYES.png", UriKind.Relative));
                    (Song_ListBox.SelectedItem as Brano).Favourite = true;
                }
            }
        }

    }
}