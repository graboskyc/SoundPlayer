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
using System.Windows.Shapes;
using System.IO;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Shell;

namespace SoundPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FolderBrowserDialog fbd = new FolderBrowserDialog();
        private string[] soundList;
        private Dictionary<string, string> playInfo = new Dictionary<string,string>();
        private bool isPlaying = false;
        private DispatcherTimer timer;
        private double ticks;
        private double length;

        public MainWindow()
        {
            InitializeComponent();

            playInfo.Add("NowPlaying_name", "");
            playInfo.Add("NowPlaying_index", "");

            isPlaying = false;

            timer = new DispatcherTimer();
            TaskbarItemInfo.Overlay = null;
            timer.Interval = TimeSpan.FromMilliseconds(250);
            timer.Tick += timerUpdate;
        }

        private void timerUpdate(object sender, EventArgs e)
        {
            if (isPlaying)
            {
                ticks = ticks + .25;
                TaskbarItemInfo.ProgressValue = ticks / length;
            }
        }

        private void PlayLoop()
        {
            if (isPlaying)
            {
                TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
                me.Stop();
                me.Source = new Uri(playInfo["NowPlaying_name"], UriKind.RelativeOrAbsolute);
                txt_nowplaying.Text = playInfo["NowPlaying_name"];
                //length = me.NaturalDuration.TimeSpan.TotalSeconds;
                me.Play();
                timer.Start();
            }
        }

        private void RandomizeList()
        {
            Random rnd = new Random();
            string[] newList = soundList.OrderBy(x => rnd.Next()).ToArray();
            soundList = newList;
        }

        private void Inc()
        {
            if (playInfo["NowPlaying_index"] == "")
            {
                playInfo["NowPlaying_name"] = soundList[0];
                playInfo["NowPlaying_index"] = "0";
            }
            else
            {
                int curr = int.Parse(playInfo["NowPlaying_index"].ToString());
                int next = curr+1;

                if (curr >= (soundList.Length - 1))
                {
                    playInfo["NowPlaying_index"] = "0";
                    curr = 0;
                    RandomizeList();
                }
                else
                {
                    curr = curr + 1;
                    playInfo["NowPlaying_index"] = curr.ToString();
                }

                playInfo["NowPlaying_name"] = soundList[curr];
            }
        }

        private void btn_Folder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (fbd.ShowDialog().ToString().Equals("OK"))
            {
                soundList = Directory.GetFiles(fbd.SelectedPath);
                Console.Write(string.Join(",",soundList));
                Inc();
            }

        }

        private void btn_Play_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (chk_random.IsChecked.Value)
            {
                RandomizeList();
            }
            isPlaying = true;
            PlayLoop();
        }

        private void btn_Stop_MouseDown(object sender, MouseButtonEventArgs e)
        {
            me.Stop();
            isPlaying = false;
            timer.Stop();
            TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Paused;
        }

        private void me_MediaOpened(object sender, RoutedEventArgs e)
        {
            length = me.NaturalDuration.TimeSpan.TotalSeconds;
        }

        private void me_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (isPlaying)
            {
                ticks = 0;
                length = int.Parse(txt_timeout.Text);
                Wait(int.Parse(txt_timeout.Text));
                Inc();
                ticks = 0;
                PlayLoop();
            }
        }

        private void Wait(int seconds)
        {
            var frame = new DispatcherFrame();
            new Thread((ThreadStart)(() =>
            {
                Thread.Sleep(TimeSpan.FromSeconds(seconds));
                frame.Continue = false;
            })).Start();
            Dispatcher.PushFrame(frame);
        }
    }
}
