using Gerador_Audio.Classes;
using Gerador_Audio.ContentsControl;
using IBM.Cloud.SDK.Core.Http;
using IBM.Cloud.SDK.Core.Http.Exceptions;
using IBM.Watson.TextToSpeech.v1.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Xml;
using config = Gerador_Audio.Properties.Settings;


namespace Gerador_Audio
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly string PathAudio = config.Default.pastaAudio;
        string arquivoGerado;
        string arquivoNome;
        ServiceApi serviceApi;
        int TabAtual = 1;
        VozesView SelecaoVoz;
        List<VozesView> listaVozes = new List<VozesView>();

        public static string _paramento = "";


        public MainWindow()
        {
            InitializeComponent();
            Setap();
        }


        private void Setap()
        {
            serviceApi = new ServiceApi(Constats.apiKey, Constats.apiUrl);
            if (!Directory.Exists(PathAudio)) Directory.CreateDirectory(PathAudio);
            editPasta.Text = config.Default.pastaAudio;
        }

        private string GetTextBox()
        {

            return new TextRange(editTexto.Document.ContentStart, editTexto.Document.ContentEnd).Text.Trim();
        }

        private void SetTextBox(string text)
        {
            editTexto.Document.Blocks.Clear();
            editTexto.AppendText(text);
        }

        #region GERAR

        private void btGerar_Click(object sender, RoutedEventArgs e)
        {

            string texto = GetTextBox();

            if (texto.Length < 5) return;
            try
            {
                GetTextToAudio(texto);
            }
            catch (ServiceResponseException ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
            }

        }


        private void GetTextToAudio(string texto)
        {

            progresso.Visibility = Visibility.Visible;
            progresso.IsIndeterminate = true;
            btGerar.Content = "Gerando áudio...";
            arquivoNome = string.IsNullOrEmpty(editNomeArquivo.Text) ? DateTime.Now.ToString("dd-MM-yyyy_hh-ss") : editNomeArquivo.Text;

            BackgroundWorker worker = new BackgroundWorker();
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.WorkerReportsProgress = true;
            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerAsync(argument: texto);
        }


        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            string text = (string)e.Argument;
            var worker = sender as BackgroundWorker;
            worker.ReportProgress(0, "Carregando...");
            var request = serviceApi.GerarAudio(text, "audio/wav", config.Default.vozAtual);
            if (request != null)
            {
                e.Result = (int)request.StatusCode;
                criarAudio(request);
            }
            else
            {
                e.Result = 400;
            }
            worker.ReportProgress(100, "Carregado 100%");
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs result)
        {

            progresso.Visibility = Visibility.Collapsed;
            progresso.IsIndeterminate = false;
            btGerar.Content = "Gerar novo";

            if ((int)result.Result == 200)
            {
                ShowHidePlay(true);
            }
            else
            {
                tbMensagem.Text = "Erro ao gerar áudio";
                borderMensagem.Visibility = Visibility.Visible;

            }
        }

        private void criarAudio(DetailedResponse<MemoryStream> resultadoStream)
        {

            string path = Path.Combine(PathAudio, arquivoNome + ".wav");
            arquivoGerado = path;
            if (resultadoStream != null)
            {
                using (FileStream fs = File.Create(path))
                {
                    resultadoStream.Result.WriteTo(fs);
                    fs.Close();
                    resultadoStream.Result.Close();
                }

            }

        }

        #endregion


        #region ListaVozes
        private void workerlist_DoWork(object sender, DoWorkEventArgs e)
        {

            var worker = sender as BackgroundWorker;
            worker.ReportProgress(0, "Carregando...");
            var list = serviceApi.ListVoices();
            e.Result = list;
            worker.ReportProgress(100, "Carregado 100%");

        }

        private void workerlist_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs result)
        {
            listaVozes = (List<VozesView>)result.Result;
            listVozes.ItemsSource = listaVozes;
            progresso.Visibility = Visibility.Collapsed;
            progresso.IsIndeterminate = false;
            //  editTexto.Document.Blocks.Clear();
        }
        #endregion

        private void btHome_Click(object sender, RoutedEventArgs e)
        {
            SetTab(1);

        }

        private void btVozes_Click(object sender, RoutedEventArgs e)
        {
            SetTab(2);
            if (listVozes.ItemsSource == null) CarregarVozes();
        }

        private void CarregarVozes()
        {
            progresso.Visibility = Visibility.Visible;
            progresso.IsIndeterminate = true;

            BackgroundWorker worker = new BackgroundWorker();
            worker.RunWorkerCompleted += workerlist_RunWorkerCompleted;
            worker.WorkerReportsProgress = true;
            worker.DoWork += workerlist_DoWork;
            worker.RunWorkerAsync();
        }


        private void btClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        #region MENU CONTEXT
        private void mcAcao_Click(object sender, RoutedEventArgs e)
        {
            var menu = (MenuItem)sender;
            string tag = menu.Tag.ToString();

            var param = tag.Equals("pitch") || tag.Equals("rate");
            bool sayAs = tag.Equals("date") || tag.Equals("cardinal") || tag.Equals("digits") || tag.Equals("letters") || tag.Equals("telephone");

            if (param || tag.Equals("date") || tag.Equals("volume") || tag.Equals("break"))
            {
                WParamento paramento = new WParamento(tag);
                if (paramento.ShowDialog() == true)
                {
                    AddTag(tag, sayAs, _paramento);
                }
            }
            else
            {
                AddTag(tag, false, "");
            }


        }



        private void AddTag(string tag, bool sayAs, string format)
        {
            if (editTexto.Selection.Text.Length <= 0) return;

            var selectionStart = editTexto.Selection.Start;
            var selectionEnd = editTexto.Selection.End;

            if (sayAs)
            {
                string t;
                if (tag.Equals("date"))
                {
                    t = string.Format("<say-as interpret-as=\"{0}\" format=\"{1}\">", tag, format);
                }
                else if (tag.Equals("telephone"))
                {
                    t = "<say-as interpret-as=\"digits\" format=\"telephone\">";
                }
                else
                {
                    t = "<say-as interpret-as=\"" + tag + "\">";
                }
                selectionStart.InsertTextInRun(t);
                selectionEnd.InsertTextInRun("</say-as>");
            }

            else if (tag.Equals("speak"))
            {
                string t = string.Format("<speak version=\"{0}\">", "1.0");

                selectionStart.InsertTextInRun(t);
                selectionEnd.InsertTextInRun("</" + tag + ">");
            }
            else if (tag.Equals("pitch"))
            {
                string t = string.Format("<prosody pitch=\"{0}\">", format);
                selectionStart.InsertTextInRun(t);
                selectionEnd.InsertTextInRun("</prosody>");
            }
            else if (tag.Equals("rate"))
            {
                string t = string.Format("<prosody rate=\"{0}\">", format);
                selectionStart.InsertTextInRun(t);
                selectionEnd.InsertTextInRun("</prosody>");
            }
            else if (tag.Equals("volume"))
            {
                string t = string.Format("<prosody volume=\"{0}\">", format);
                selectionStart.InsertTextInRun(t);
                selectionEnd.InsertTextInRun("</prosody>");
            }
            else if (tag.Equals("break"))
            {
                string t = string.Format("<break time=\"{0}\">", format);
                selectionStart.InsertTextInRun(t);
                selectionEnd.InsertTextInRun("</break>");
            }

            else
            {
                selectionStart.InsertTextInRun("<" + tag + ">");
                selectionEnd.InsertTextInRun("</" + tag + ">");
            }

            editTexto.Selection.Select(editTexto.CaretPosition.DocumentEnd, editTexto.CaretPosition.DocumentEnd);
        }

        #endregion

        #region BARRA de Ação
        private void btSalvar_Click(object sender, RoutedEventArgs e)
        {
            config.Default.Upgrade();
            config.Default.conteudoBox = GetTextBox();
            config.Default.Save();
        }

        private void btRestaurar_Click(object sender, RoutedEventArgs e)
        {
            SetTextBox(config.Default.conteudoBox);
        }

        private void btAudios_Click(object sender, RoutedEventArgs e)
        {
            SetTab(3);
        }

        #endregion

        private void Janela_MouseMove(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        private void btClosePlay_Click(object sender, RoutedEventArgs e)
        {
            ShowHidePlay(false);
        }

        private void mDefinirVoz_Click(object sender, RoutedEventArgs e)
        {
            config.Default.Upgrade();
            config.Default.vozAtual = SelecaoVoz.Nome;
            config.Default.Save();
            listVozes.ItemsSource = null;
            CarregarVozes();
        }

        private void listVozes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listVozes.ItemsSource != null) SelecaoVoz = (VozesView)listVozes.SelectedItem;

        }


        private void ShowHidePlay(bool show)
        {
            var sb = new Storyboard();
            var ta = new ThicknessAnimation();
            ta.BeginTime = new TimeSpan(0);
            ta.SetValue(Storyboard.TargetNameProperty, "borderContent");
            Storyboard.SetTargetProperty(ta, new PropertyPath(MarginProperty));

            if (show)
            {
                fundo.Visibility = Visibility.Visible;
                contentBase.Content = new Player(arquivoGerado);
                ta.From = new Thickness(-360);
                ta.To = new Thickness(0);
                ta.Duration = new Duration(TimeSpan.FromMilliseconds(400));
            }
            else
            {
                fundo.Visibility = Visibility.Collapsed;
                contentBase.Content = null;
                ta.From = new Thickness(0);
                ta.To = new Thickness(-360);
                ta.Duration = new Duration(TimeSpan.FromMilliseconds(200));
            }

            sb.Children.Add(ta);
            sb.Begin(this);
        }


        private void btCloseMensagem(object sender, MouseButtonEventArgs e)
        {
            borderMensagem.Visibility = Visibility.Collapsed;
        }


        private void btSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.ShowDialog();
                editPasta.Text = dialog.SelectedPath;
                config.Default.Upgrade();
                config.Default.pastaAudio = dialog.SelectedPath;
                config.Default.Save();
            }
        }

        private void btConfig_Click(object sender, RoutedEventArgs e)
        {
            if (gridPasta.IsVisible)
            {
                gridPasta.Visibility = Visibility.Collapsed;
            }
            else
            {
                gridPasta.Visibility = Visibility.Visible;
            }
        }


        #region TABS

        private void SetTab(int tab)
        {
            ResetTab();
            if (tab == 1)
            {
                if (!stakBotoesAcao.IsVisible) stakBotoesAcao.Visibility = Visibility.Visible;
                if (contentAudios.Content != null) contentAudios.Content = null;
                containerHome.Visibility = Visibility.Visible;
                btHome.IsEnabled = false;
            }
            else if (tab == 2)
            {
                if (stakBotoesAcao.IsVisible) stakBotoesAcao.Visibility = Visibility.Collapsed;
                if (contentAudios.Content != null) contentAudios.Content = null;
                containerVozes.Visibility = Visibility.Visible;
                btVozes.IsEnabled = false;
            }
            else
            {
                if (stakBotoesAcao.IsVisible) stakBotoesAcao.Visibility = Visibility.Collapsed;
                containerAudios.Visibility = Visibility.Visible;
                btAudios.IsEnabled = false;
                contentAudios.Content = new Audios();
            }
            TabAtual = tab;
        }

        private void ResetTab()
        {
            if (TabAtual == 1)
            {
                containerHome.Visibility = Visibility.Collapsed;
                btHome.IsEnabled = true;

            }
            else if (TabAtual == 2)
            {

                containerVozes.Visibility = Visibility.Collapsed;
                btVozes.IsEnabled = true;
            }
            else
            {
                containerAudios.Visibility = Visibility.Collapsed;
                btAudios.IsEnabled = true;
            }

        }
        #endregion
    }
}
