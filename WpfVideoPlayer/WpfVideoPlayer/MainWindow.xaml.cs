using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace WpfVideoPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        int Pointer = 0;
        bool flag = true;
        List<string> queue = new List<string>();

        private void Window_Loaded(object sender, RoutedEventArgs ea)
        {
            OpenFileDialog dlg = GetFiles();

            bool? res = dlg.ShowDialog();
            foreach (var names in dlg.FileNames)
            {
                queue.Add(names);
            }
            if (res.HasValue && res.Value)
            {
                _media.MediaOpened += _media_MediaOpened;
                _media.Clock = new MediaTimeline(new Uri(dlg.FileName, UriKind.Absolute)).CreateClock();
                _media.Clock.CurrentTimeInvalidated += Clock_CurrentTimeInvalidated;
            }
        }

        private OpenFileDialog GetFiles()
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                Filter = "All Files (*.*)|*.*|Video Files (*.avi)|*.avi",
                FilterIndex = 2,
                Multiselect = true
            };
            return dlg;
        }

        private void Clock_CurrentTimeInvalidated(object sender, EventArgs e)
        {
            _mediaProgress.Value = _media.Position.TotalSeconds;

            if (flag)
                pop_time.Content = $"{_media.Position.Hours}:{_media.Position.Minutes}:{_media.Position.Seconds}";
        }

        private void _media_MediaOpened(object sender, RoutedEventArgs e)
        {
            _mediaProgress.Maximum = _media.NaturalDuration.TimeSpan.TotalSeconds;
        }

        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            _media.Clock.Controller.Begin();
        }

        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (_media.Clock.IsPaused)
            {
                btn.Content = "Pause";
                _media.Clock.Controller.Resume();
            }
            else
            {
                btn.Content = "Play";
                _media.Clock.Controller.Pause();
            }
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            _media.Clock.Controller.Stop();
        }

        private void _mediaProgress_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TimeSpan ts = timePos(e);

            _media.Clock.Controller.Seek(ts, TimeSeekOrigin.BeginTime);
        }

        private void _mediaProgress_GotMouseCapture(object sender, MouseEventArgs e)
        {
            flag = false;
            _mediaProgress_MouseMove(sender, e);
        }

        private TimeSpan timePos(MouseEventArgs e)
        {
            double x = e.GetPosition(_mediaProgress).X;
            double pos = x * 100 / _mediaProgress.ActualWidth;
            try
            {
                TimeSpan ts = TimeSpan.FromSeconds(_media.Clock.NaturalDuration.TimeSpan.TotalSeconds / 100.0 * pos);
                return ts;
            }
            catch
            {
                return new TimeSpan();
            }
        }

        private void _mediaProgress_MouseLeave(object sender, MouseEventArgs e)
        {
            flag = true;
        }

        private void trackVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _media.Volume = (double)trackVolume.Value;
        }

        private void _mediaProgress_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                TimeSpan ts = timePos(e);
                TimeSpan te = TimeSpan.FromSeconds(_media.Clock.NaturalDuration.TimeSpan.TotalSeconds);
                pop_time.Content = $"{ts.Hours}:{ts.Minutes}:{ts.Seconds} / {te.Hours}:{te.Minutes}:{te.Seconds}";
            }
            catch { }
        }

        private void NewFilm()
        {
            Pointer++;
            if (Pointer < queue.Count)
            {
                _media.MediaOpened += _media_MediaOpened;
                _media.Clock = new MediaTimeline(new Uri(queue[Pointer], UriKind.Absolute)).CreateClock();
                _media.Clock.CurrentTimeInvalidated += Clock_CurrentTimeInvalidated;
            }
        }

        private void NewFilm(int index)
        {
            Pointer = index;
            if (Pointer < queue.Count)
            {
                _media.MediaOpened += _media_MediaOpened;
                _media.Clock = new MediaTimeline(new Uri(queue[Pointer], UriKind.Absolute)).CreateClock();
                _media.Clock.CurrentTimeInvalidated += Clock_CurrentTimeInvalidated;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = GetFiles();
            dlg.ShowDialog();
            foreach (var names in dlg.FileNames)
            {
                queue.Add(names);
            }
            if (Exp.IsExpanded)
                Exp_Expanded(sender, e);
        }

        private void _media_MediaEnded(object sender, RoutedEventArgs e)
        {
            NewFilm();
        }

        private void Exp_Expanded(object sender, RoutedEventArgs e)
        {
            StackPanel panel = new StackPanel();
            int i = 0;
            foreach (var list in queue)
            {
                string[] str = list.Split('\\');
                Button btn = new Button() { Width = 80, Tag = i, Height = 30, Content = str[str.Length - 1] };
                btn.Click += button_Click;
                panel.Children.Add(btn);
                i++;
            }
            Exp.Content = panel;
        }

        private void button_Click(object sender, EventArgs e)
        {
            int index = Int32.Parse((sender as Button).Tag.ToString());
            NewFilm(index);
        }
    }
}
