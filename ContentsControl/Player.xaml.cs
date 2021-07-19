using Gerador_Audio.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Media;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Gerador_Audio.ContentsControl
{
    /// <summary>
    /// Interação lógica para Player.xam
    /// </summary>
    public partial class Player : ContentControl
    {
        string audioAtual;
        bool isDragging;
        double posicaoAtual;
        DispatcherTimer progressTimer;
   
        public Player()
        {
            InitializeComponent();
            audioAtual = "";
            btnPlay.IsEnabled = false;
            Setap();
        }
        public Player(string arquivo)
        {
            InitializeComponent();
            Setap();
            audioAtual = arquivo;
            SetAudio(true);

        }

        private void Setap()
        {
            progressTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };

            // attach progressTimer_Tick method to event timer Tick Event Handler
            progressTimer.Tick += ProgressTimer_Tick;
        }
        private void SetAudio(bool tocar)
        {
           
            progressTimer.Start();
            try
            {
               mediaPlayer.Source = new Uri(audioAtual);
               tbReproduzindo.Text = audioAtual;
                if (tocar) {
                    mediaPlayer.Play();
                    btnStop.IsEnabled = true;
                    btnPlay.IsEnabled = false;
                    sliderProgress.IsEnabled = true;
                    posicaoAtual = 0;
                }

           
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void mediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("mediaPlayer_MediaOpened");
            if (mediaPlayer.NaturalDuration.HasTimeSpan) { 
                var timeSpan = mediaPlayer.NaturalDuration.TimeSpan;
                sliderProgress.Maximum = timeSpan.TotalSeconds;
                sliderProgress.SmallChange = 1;
                sliderProgress.LargeChange = Math.Min(10, timeSpan.Seconds / 10);
                txtEndTime.Text = TimeSpan.FromSeconds(timeSpan.TotalSeconds).ToString(@"mm\:ss");
            }
       
        }

        private void mediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("mediaPlayer_MediaEnded");
            btnStop.IsEnabled = false;
            btnPause.IsEnabled = false;
            btnPlay.IsEnabled = true;
            mediaPlayer.Stop();
            progressTimer.Stop();
            sliderProgress.Value = 0.0;
            txtEllapsedTime.Text = "00:00";

        }

        private void mediaPlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            Debug.WriteLine("mediaPlayer_MediaFailed");
            progressTimer.Stop();
            mediaPlayer.Stop();

        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Play();
            progressTimer.Start();
            btnStop.IsEnabled = true;
            btnPause.IsEnabled = true;
            btnPlay.IsEnabled = false;
            sliderProgress.IsEnabled = true;
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
            btnStop.IsEnabled = false;
            btnPause.IsEnabled = false;
            btnPlay.IsEnabled = true;
            sliderProgress.IsEnabled = false;
            posicaoAtual = 0;
            sliderProgress.Value = 0;
        }

        private void sliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (txtVolPercent != null) {
            int volPercent = (int)(sliderVolume.Value * 100);

            // update the UI with the percentage
            txtVolPercent.Text = volPercent + "%";
            }

        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Pause();
            btnStop.IsEnabled = false;
            btnPause.IsEnabled = false;
            btnPlay.IsEnabled = true;
            sliderProgress.IsEnabled = false;
        }


        #region Progresso
        void SliderProgress_DragStarted(object sender, DragStartedEventArgs e)
        {
            isDragging = true;
        }

   
        void SliderProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            // set is dragging to false
            isDragging = false;

            // update the position of the media player to the current value of the slider
            mediaPlayer.Position = TimeSpan.FromSeconds(sliderProgress.Value);
        }

        void ProgressTimer_Tick(object sender, EventArgs e)
        {
            if (!mediaPlayer.IsVisible) {
                progressTimer.Stop();
            }
            // if is not dragging, media player source is not null and has TimeSpan
            if (!isDragging && mediaPlayer.Source != null && mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                // set slider value to media player position in seconds
                sliderProgress.Value = mediaPlayer.Position.TotalSeconds;

                // update the UI textEllapsedTime with the value of the slider
                txtEllapsedTime.Text = TimeSpan.FromSeconds(sliderProgress.Value).ToString(@"mm\:ss");

                // set the current position to the slider's value
                posicaoAtual = sliderProgress.Value;
     
            }
        }
        #endregion

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                audioAtual = files[0];
                SetAudio(true);
            }
        }

        private void sliderProgress_GotMouseCapture(object sender, MouseEventArgs e)
        {
           // isDragging = true;
        }

        private void btSom_Click(object sender, RoutedEventArgs e)
        {
  
            if (mediaPlayer.IsMuted) {
                mediaPlayer.IsMuted = false;
                btSom.Background = Ferramentas.GetImagemButton("volume_on.png");
            }
            else
            {
                mediaPlayer.IsMuted = true;        
                btSom.Background = Ferramentas.GetImagemButton("volume_off.png");
            }
        
        }
    }
}
