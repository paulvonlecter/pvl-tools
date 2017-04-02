using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;

namespace pvl_radio {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        // Making global objects
        public static Uri streamUrl = new Uri("http://giss.tv:8001/paulvonlecter.mp3");
        public MediaPlayer stream = new MediaPlayer();
        public DispatcherTimer timer = new DispatcherTimer();
        public DispatcherTimer metadataTimer = new DispatcherTimer();
        public int hour, min, sec;
        public string h, m, s;
        public bool playing = false;
        // Let's start!
        public MainWindow()
        {
            this.InitializeComponent();
            // Insert code required on object creation below this point.
            Thread metadataThread = new Thread(updateMeta);
            metadataThread.IsBackground = true;
            metadataThread.Start();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += new EventHandler(timer_Tick);
            metadataTimer.Interval = new TimeSpan(0, 0, 5);
            metadataTimer.Tick += MetadataTimer_Tick;
            hour = 0;
            min = 0;
            sec = 0;
            VolumeSlider.Value = stream.Volume;
        }
        private void MetadataTimer_Tick(object sender, EventArgs e)
        {
            updateMeta();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PlayMusic();
        }
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            PlayMusic();
        }
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            StopMusic();
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            StopMusic();
        }
        private void timer_Tick(object sender, EventArgs e)
        {
            TimeCount();
            if (playing) {
                CurrentTime.Text = h + ":" + m + ":" + s;
            } else {
                CurrentTime.Text = "00:00:00";
            }
        }
        public void TimeCount()
        {
            if (sec > 59) {
                sec = 0;
                min++;
            }
            if (min > 59) {
                min = 0;
                hour++;
            }
            if (hour > 23) {
                hour = 0;
            }
            if (sec < 10) s = "0" + sec; else s = sec.ToString();
            if (min < 10) m = "0" + min; else m = min.ToString();
            if (hour < 10) h = "0" + hour; else h = hour.ToString();
            sec++;
        }
        public void PlayMusic()
        {
            stream.Open(streamUrl);
            stream.Play();
            playing = true;
            timer.Start();
            metadataTimer.Start();
            TimeCount();
        }
        public void StopMusic()
        {
            stream.Stop();
            stream.Close();
            timer.Stop();
            hour = 0;
            min = 0;
            sec = 0;
            playing = false;
            CurrentTime.Text = "00:00:00";
        }
        private static string GET(string Url, string Data)
        {
            WebRequest req = WebRequest.Create(Url + "?" + Data);
            WebResponse resp = req.GetResponse();
            Stream stream = resp.GetResponseStream();
            StreamReader sr = new StreamReader(stream);
            string Out = sr.ReadToEnd();
            sr.Close();
            return Out;
        }
        public void updateMeta()
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Background, (ThreadStart)delegate()
            {
                var serializer = new JavaScriptSerializer();
                string json = GET("http://paulvonlecter.name/api/", "");
                string gissStats = GET("http://giss.tv:8001/status2.xsl", "mount=/paulvonlecter.mp3");
                var GISSarray = gissStats.Split(',');
                var JSarray = serializer.Deserialize<Dictionary<string, string>>(json);
                TrackName.Text = JSarray["name"];
                //ListenersCount.Text = JSarray["listeners"];
                ListenersCount.Text = (Convert.ToInt32(GISSarray[15])+1).ToString();
                BitmapImage bm1 = new BitmapImage();
                bm1.BeginInit();
                bm1.UriSource = new Uri("http://paulvonlecter.name/covers/" + JSarray["songid"] + ".jpg");
                bm1.CacheOption = BitmapCacheOption.OnLoad;
                bm1.EndInit();
                TrackCover.Source = bm1;
                DummyCover.Visibility = Visibility.Collapsed;
                TrackCover.Visibility = Visibility.Visible;
            });
        }
        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            stream.Volume = VolumeSlider.Value;
        }
    }

}