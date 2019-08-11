using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Threading;
using System.Xml;
using System.Configuration;

namespace MusicPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public delegate void timerTick();
        DispatcherTimer ticks = new DispatcherTimer();
        timerTick tick;

        bool _isPlaying = false;
        bool _isShuffleEnable = false;
        bool _isRepeatEnable = false;


        BindingList<string> _playlist = new BindingList<string>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NameOfSong.Content = "Chưa chọn bài hát";
            LoadAppConfig();

            this.KeyDown += new KeyEventHandler(MainWindow_KeyDown);

        }

        private void LoadAppConfig()
        {
            var url = System.AppDomain.CurrentDomain.BaseDirectory + "/appConfig.xml";
            if (File.Exists(url))
            {
                loadPlaylist(url);
                MessageBox.Show("Previous Playlist Loaded");
            }
        }

        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.N) //next
            {
                playNextSong();
            }
            else if (e.Key == Key.B) //back
            {
                playPreviousSong();
            } 
            else if (e.Key == Key.P) //playPause option 
            {
                play_pauseMedia();
            }
        }

        private string getNameBySplitPath(string path)
        {
            string[] result = path.Split('\\');
            return result.Last();
        }

        private void newPlaylistClicked(object sender, RoutedEventArgs e)
        {
            var screen = new OpenFileDialog();

            screen.Multiselect = true;
            if (screen.ShowDialog() == true)
            {
                foreach(var file in screen.FileNames)
                {
                    //only allow audio files
                    FileInfo fi = new FileInfo(file);
                    string extention = fi.Extension;
            
                    if (extention == ".mp3")
                    {
                       // var name = getNameBySplitPath(file);
                        _playlist.Add(file);
                    }               
                }
                mPlaylistListView.ItemsSource = _playlist;
            }
        }

        private void MediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            slider.Minimum = 0;
            slider.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
            MaxDuration.Content = Milliseconds_to_Minute((long)mediaElement.NaturalDuration.TimeSpan.TotalMilliseconds);
            mediaElement.Position = new TimeSpan(0, 0, 0, 0, (int)slider.Value);
            ticks.Interval = TimeSpan.FromMilliseconds(1);
            ticks.Tick += ticks_Tick;
            tick = new timerTick(changeStatus);
            ticks.Start();
        }

        void ticks_Tick(object sender, object e)
        {
            Dispatcher.Invoke(tick);
        }

        void changeStatus()
        {
            /* If you want the Slider to Update Regularly Just UnComment the Line Below*/
            slider.Value = mediaElement.Position.TotalSeconds;
            Duration.Content = Milliseconds_to_Minute((long)mediaElement.Position.TotalMilliseconds);
        }

        public string Milliseconds_to_Minute(long milliseconds)
        {
            int minute = (int)(milliseconds / (1000 * 60));
            int seconds = (int)(milliseconds / 1000 % 60);
         
            return (minute + " : " + seconds);
        }

        //Optional function
        private void DurationSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            /* Binds it to the Media Element */
            TimeSpan ts = new TimeSpan(0, 0, 0, 0, (int)slider.Value);
            mediaElement.Position = ts;
        }

        private void playASong(object sender, RoutedEventArgs e)
        {
            var screen = new OpenFileDialog();

            screen.Multiselect = false;
            if (screen.ShowDialog() == true)
            {
                //only allow audio files
                FileInfo fi = new FileInfo(screen.FileName);
                string extention = fi.Extension;

                if (extention == ".mp3")
                {
                    mediaElement.Source = new Uri(screen.FileName);
                    mediaElement.Play();
                    NameOfSong.Content = screen.FileName;
                    _isPlaying = true;
                }
            }
        }

        private void play_pauseMedia()
        {
            if (_isPlaying)
            {
                mediaElement.Pause();
            }
            else
            {
                mediaElement.Play();
            }

            _isPlaying = !_isPlaying;
        }

        private void play_pauseButton(object sender, RoutedEventArgs e)
        {
            play_pauseMedia();
        }

        private void playlistChangedSelection(object sender, SelectionChangedEventArgs e)
        {
           var index =  mPlaylistListView.SelectedIndex;
            playFromPlaylist(index);
        }

        private void playFromPlaylist(int index)
        {
            NameOfSong.Content = _playlist[index];
            mediaElement.Source = new Uri(_playlist[index]);
            mediaElement.Play();
            _isPlaying = true;
        }

        private int randomNumberWithRange(int min, int max)
        {
            Random random = new Random();
            return random.Next(min, max);
        }

        private void playNextSong()
        {
            int index;
            if (_isShuffleEnable)
            {
                index = randomNumberWithRange(0, _playlist.Count());
                mPlaylistListView.SelectedIndex = index;
                playFromPlaylist(index);
            }
            else
            {
                index = mPlaylistListView.SelectedIndex + 1;

                if (index < _playlist.Count())
                {
                    mPlaylistListView.SelectedIndex = index;
                    playFromPlaylist(index);
                }
                else
                {
                    if (_isRepeatEnable)
                    {
                        index = 0;
                        mPlaylistListView.SelectedIndex = index;
                        playFromPlaylist(index);
                    } else
                    {
                        MessageBox.Show("End of Playlist", "Alert");
                    }
                }
            }
        }

        private void nextSongButton(object sender, RoutedEventArgs e)
        {
            playNextSong();
        }

        private void playPreviousSong()
        {
            int index = mPlaylistListView.SelectedIndex;
            if (index == 0)
            {
                index = _playlist.Count() - 1;
                mPlaylistListView.SelectedIndex = index;
            }
            else
            {
                index--;
                mPlaylistListView.SelectedIndex = index;
            }
            playFromPlaylist(index);
        }

        private void usingShuffle(object sender, RoutedEventArgs e)
        {
            _isShuffleEnable = !_isShuffleEnable;
        }

        private void previousSongButton(object sender, RoutedEventArgs e)
        {
            playPreviousSong();
        }

        private void changeRepeatModeButton(object sender, RoutedEventArgs e)
        {
            _isRepeatEnable = !_isRepeatEnable;
        }

        private void handleMediaEnded(object sender, RoutedEventArgs e)
        {
            playNextSong();
        }

        private void savePlaylist(string fileName)
        {
                var doc = new XmlDocument();

                var root = doc.CreateElement("Music_Player");
                root.SetAttribute("IsPlayingAtIndex", mPlaylistListView.SelectedIndex.ToString());
                root.SetAttribute("SliderValue", (-1).ToString()); //hard code slider value -> will fix later
                root.SetAttribute("IsRepeat", _isRepeatEnable.ToString());
                root.SetAttribute("IsShuffle", _isShuffleEnable.ToString());

                var state = doc.CreateElement("PlayList");
                root.AppendChild(state);

                foreach(var song in _playlist)
                {
                    var line = doc.CreateElement("Line");
                    line.SetAttribute("Value", $"{song}");
                    state.AppendChild(line);
                }

                doc.AppendChild(root);
                doc.Save(fileName); 
        }

        private void loadPlaylist(string fileName)
        {
            var doc = new System.Xml.XmlDocument();
            doc.Load(fileName);

            var root = doc.DocumentElement;
            mPlaylistListView.SelectedIndex = int.Parse(root.Attributes["IsPlayingAtIndex"].Value);
            _isShuffleEnable = root.Attributes["IsRepeat"].Value == "True" ? true : false;
            _isRepeatEnable = root.Attributes["IsShuffle"].Value == "True" ? true : false;

            foreach (var song in _playlist)
            {
                _playlist.Remove(song);
            }

            foreach (XmlNode node in root.FirstChild.ChildNodes)
            {

                string song = node.Attributes["Value"].Value;
                _playlist.Add(song);
            }
            mPlaylistListView.ItemsSource = _playlist;
        }



        private void hanldeLoadPlaylistFromXML(object sender, RoutedEventArgs e)
        {
            var screen = new Microsoft.Win32.OpenFileDialog();
            if (screen.ShowDialog() == true)
            {
                loadPlaylist(screen.FileName);
            }
        }

        private void handleSavePlaylist(object sender, RoutedEventArgs e)
        {
            if (_playlist.Count() <= 0)
            {
                MessageBox.Show("Can't save! You must have atleast one song in playlist.");
                return;
            }

            var screen = new SaveFileDialog();

            if (screen.ShowDialog() == true)
            {
                savePlaylist(screen.FileName);
                MessageBox.Show("Save successfully");
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            //save app Config 

            if (_playlist.Count() <= 0) return;

            var url = System.AppDomain.CurrentDomain.BaseDirectory + "/appConfig.xml";
            savePlaylist(url);
            MessageBox.Show("Appconfig Saved");
        }
    }
}
