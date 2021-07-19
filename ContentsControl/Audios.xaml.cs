using Gerador_Audio.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using config = Gerador_Audio.Properties.Settings;

namespace Gerador_Audio.ContentsControl
{
    /// <summary>
    /// Interação lógica para Audios.xam
    /// </summary>
    public partial class Audios : UserControl
    {
        private List<AudioView> ListaAudio = new List<AudioView>();
       readonly string pastaAudio = config.Default.pastaAudio;
        private AudioView SelecaoAudio; 
        public Audios()
        {
            InitializeComponent();
            contentPlayer.Content = new Player();
            CarregarArquivos();
        }

        private void CarregarArquivos()
        {
            if (ListaAudio.Count > 0) {
                ListaAudio.Clear();
                listAudios.ItemsSource = null;
            }
            try
            {
                // Only get files that begin with the letter "c".
                string[] files = Directory.GetFiles(pastaAudio, "*.wav");
                Debug.WriteLine("Arquivos {0}.", files.Length);
                AudioView audioView = null;
                foreach (string file in files)
                {
                    Debug.WriteLine(file);
   
                    audioView = new AudioView();
                    audioView.Caminho = file;
                    audioView.Nome = getNome(file);
                    ListaAudio.Add(audioView);
                }
                listAudios.ItemsSource = ListaAudio;
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }

        }

        private string getNome(string path)
        {
            return path.Replace(pastaAudio, "").Replace(".wav", "").Replace("\\", "");
        }

        private void listAudios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
         
            if (listAudios.SelectedItem != null)
            {
                SelecaoAudio = (AudioView) listAudios.SelectedItem;
            }
        }

        private void listAudios_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelecaoAudio == null) return;
            contentPlayer.Content = new Player(SelecaoAudio.Caminho);
        }

        #region MenuContext
        private void mReproduzir_Click(object sender, RoutedEventArgs e)
        {
            if (SelecaoAudio == null) return;
            contentPlayer.Content = new Player(SelecaoAudio.Caminho);          
        }

        private void mRenomear_Click(object sender, RoutedEventArgs e)
        {
            if (SelecaoAudio == null) return;
            editNomeNovo.Text = SelecaoAudio.Nome;
            telaRenome.Visibility = Visibility.Visible;
      
        }

        private void RenomearArquivo(string nome)
        {
            try
            {
                nome = nome.Contains(".wav") ? nome : nome+ ".wav";
                File.Move(SelecaoAudio.Caminho, pastaAudio + "/"+ nome);
                telaRenome.Visibility = Visibility.Collapsed;
                CarregarArquivos();

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void mApagar_Click(object sender, RoutedEventArgs e)
        {
            if (SelecaoAudio == null) return;

            var questao = MessageBox.Show(string.Format("Tem certeza que deseja apagar: {0}?", SelecaoAudio.Nome), "Apagar áudio", MessageBoxButton.YesNo);
            if (questao == MessageBoxResult.Yes)
            {
                try
                {
                    File.Delete(SelecaoAudio.Caminho);

                    CarregarArquivos();
                }
                catch(Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        private void editNomeNovo_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                RenomearArquivo(editNomeNovo.Text);
            }
        }

        #endregion

        private void btClose_Click(object sender, RoutedEventArgs e)
        {
            telaRenome.Visibility = Visibility.Collapsed;
        }
    }
}
