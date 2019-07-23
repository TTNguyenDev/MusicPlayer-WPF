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

namespace MusicPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        BindingList<string> _playlist = new BindingList<string>();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            

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
                        _playlist.Add(file);
                }
              
            }

            mPlaylistListView.ItemsSource = _playlist;
            mediaElement.Source = new Uri(_playlist[0]);
            mediaElement.Play();

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
                }
                        
 

            }

          
        }

        private void MediaElement_GotMouseCapture(object sender, MouseEventArgs e)
        {

        }
    }
}
