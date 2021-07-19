using IBM.Cloud.SDK.Core.Authentication.Iam;
using IBM.Cloud.SDK.Core.Http;
using IBM.Cloud.SDK.Core.Http.Exceptions;
using IBM.Watson.TextToSpeech.v1;
using IBM.Watson.TextToSpeech.v1.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using config = Gerador_Audio.Properties.Settings;

namespace Gerador_Audio.Classes
{
    public class ServiceApi
    {
        string Apikey;
        string UrlServico;
        string customizationId;

        public ServiceApi() {
        }
        public ServiceApi(string apikey, string url)
        {
            Apikey = apikey;
            UrlServico = url;
        }
        
        #region Voices
        public List<VozesView> ListVoices()
        {
            List<VozesView> list = new List<VozesView>();
            try
            {
                IamAuthenticator authenticator = new IamAuthenticator(
                              apikey: Apikey);

                TextToSpeechService service = new TextToSpeechService(authenticator);
                service.SetServiceUrl(UrlServico);

                var result = service.ListVoices();
                VozesView vozes = null;
                foreach (var voz in result.Result._Voices) {
                    vozes = new VozesView();
                    vozes.Nome = voz.Name;
                    vozes.Descricao = voz.Description;
                    vozes.Genero = voz.Gender;
                    vozes.Linguagem = voz.Language;
                    vozes.Definido = voz.Name.Equals(config.Default.vozAtual);
                    list.Add(vozes);
                }
               
            }
            catch (ServiceResponseException ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
            }

            return list;
        }

        public void GetVoice(string v)
        {
            IamAuthenticator authenticator = new IamAuthenticator(
                apikey: Apikey);

            TextToSpeechService service = new TextToSpeechService(authenticator);
            service.SetServiceUrl(UrlServico);

            var result = service.GetVoice(v);

            Console.WriteLine(result.Result);
        }
        #endregion

        #region Synthesis
        public DetailedResponse<MemoryStream> GerarAudio(string t, string a, string v)
        {
            IamAuthenticator authenticator = new IamAuthenticator(
                apikey: Apikey);

            TextToSpeechService service = new TextToSpeechService(authenticator);
            service.SetServiceUrl(UrlServico);

            try
            {
                var result = service.Synthesize(
                text: t,
                accept: a,
                voice: v
                );
                result.StatusCode = 200;
                return result;
            }
            catch (ServiceResponseException ex)
            {
                //MainWindow._tbMensagem.Text = string.Format("{0}:{1}", ex.Error.CodeDescription, ex.Error.Message);
                //MainWindow._borderMensagem.Visibility = System.Windows.Visibility.Visible;

                Debug.WriteLine("Erro GerarAudio: " + ex.Error.Message);
            }
            catch (Exception ex)
            {               
                Debug.WriteLine("Error GerarAudio: " + ex.Message);
            }


            //using (FileStream fs = File.Create("hello_world.wav"))
            //{
            //    result.Result.WriteTo(fs);
            //    fs.Close();
            //    result.Result.Close();
            //}
            return null;
        }
        #endregion

        #region Pronunciation
        public void GetPronunciation(string t, string f, string v)
        {
            IamAuthenticator authenticator = new IamAuthenticator(
                apikey: Apikey);

            TextToSpeechService service = new TextToSpeechService(authenticator);
            service.SetServiceUrl(UrlServico);

            var result = service.GetPronunciation(
                text: t,
                format: f,
                voice: v
                );

            Console.WriteLine(result.Result);
        }
        #endregion

        #region Custom Models
        public void ListVoiceModels()
        {
            IamAuthenticator authenticator = new IamAuthenticator(
                apikey: Apikey);

            TextToSpeechService service = new TextToSpeechService(authenticator);
            service.SetServiceUrl(UrlServico);

            var result = service.ListCustomModels();

            Console.WriteLine(result.Result);
        }

        public void CreateVoiceModel(string n, string l, string d)
        {
            IamAuthenticator authenticator = new IamAuthenticator(
                apikey: Apikey);

            TextToSpeechService service = new TextToSpeechService(authenticator);
            service.SetServiceUrl(UrlServico);

            var result = service.CreateCustomModel(
                name: n,
                language: l,
                description: d
                );

            Console.WriteLine(result.Result);

            customizationId = result.Result.CustomizationId;
        }

        public void GetVoiceModel(string Id)
        {
            IamAuthenticator authenticator = new IamAuthenticator(
                apikey: Apikey);

            TextToSpeechService service = new TextToSpeechService(authenticator);
            service.SetServiceUrl(UrlServico);

            var result = service.GetCustomModel(
                customizationId: Id
                );

            Console.WriteLine(result.Result);
        }

        public void UpdateVoiceModel()
        {
            IamAuthenticator authenticator = new IamAuthenticator(
                apikey: Apikey);

            TextToSpeechService service = new TextToSpeechService(authenticator);
            service.SetServiceUrl(UrlServico);

            var words = new List<Word>()
            {
                new Word()
                {
                    _Word = "NCAA",
                    Translation = "N C double A"
                },
                new Word()
                {
                    _Word = "iPhone",
                    Translation = "I phone"
                }
            };

            var result = service.UpdateCustomModel(
                customizationId: "{customizationId}",
                name: "First Model Update",
                description: "First custom voice model update",
                words: words
                );

            Console.WriteLine(result.Result);
        }

        public void DeleteVoiceModel()
        {
            IamAuthenticator authenticator = new IamAuthenticator(
                apikey: Apikey);

            TextToSpeechService service = new TextToSpeechService(authenticator);
            service.SetServiceUrl(UrlServico);

            var result = service.DeleteCustomModel(
                customizationId: "{customizationId}"
                );

            Console.WriteLine(result.StatusCode);
        }
        #endregion

        #region Custom Words
        private void AddWords()
        {
            IamAuthenticator authenticator = new IamAuthenticator(
                apikey: Apikey);

            TextToSpeechService service = new TextToSpeechService(authenticator);
            service.SetServiceUrl(UrlServico);

            var words = new List<Word>()
            {
                new Word()
                {
                    _Word = "EEE",
                    Translation = "<phoneme alphabet=\"ibm\" ph=\"tr1Ipxl.1i\"></phoneme>"
                },
                new Word()
                {
                    _Word = "IEEE",
                    Translation = "<phoneme alphabet=\"ibm\" ph=\"1Y.tr1Ipxl.1i\"></phoneme>"
                }
            };

            var result = service.AddWords(
                customizationId: "{customizationId}",
                words: words
                );

            Console.WriteLine(result.StatusCode);
        }

        private void ListWords()
        {
            IamAuthenticator authenticator = new IamAuthenticator(
                apikey: Apikey);

            TextToSpeechService service = new TextToSpeechService(authenticator);
            service.SetServiceUrl(UrlServico);

            var result = service.ListWords(
                customizationId: "{customizationId}"
                );

            Console.WriteLine(result.StatusCode);
        }

        private void AddWord()
        {
            IamAuthenticator authenticator = new IamAuthenticator(
                apikey: Apikey);

            TextToSpeechService service = new TextToSpeechService(authenticator);
            service.SetServiceUrl(UrlServico);

            var result = service.AddWord(
                customizationId: "{customizationId}",
                word: "ACLs",
                translation: "ackles"
                );

            Console.WriteLine(result.StatusCode);
        }

        private void GetWord()
        {
            IamAuthenticator authenticator = new IamAuthenticator(
                apikey: Apikey);

            TextToSpeechService service = new TextToSpeechService(authenticator);
            service.SetServiceUrl(UrlServico);

            var result = service.GetWord(
                customizationId: "{customizationId}",
                word: "ACLs"
                );

            Console.WriteLine(result.StatusCode);
        }

        private void DeleteWord()
        {
            IamAuthenticator authenticator = new IamAuthenticator(
                apikey: Apikey);

            TextToSpeechService service = new TextToSpeechService(authenticator);
            service.SetServiceUrl(UrlServico);

            var result = service.DeleteWord(
                customizationId: "{customizationId}",
                word: "ACLs"
                );

            Console.WriteLine(result.StatusCode);
        }
        #endregion

        #region User Data
        public void DeleteUserData()
        {
            IamAuthenticator authenticator = new IamAuthenticator(
                apikey: Apikey);

            TextToSpeechService service = new TextToSpeechService(authenticator);
            service.SetServiceUrl(UrlServico);

            var result = service.DeleteUserData(
                customerId: "customer_ID"
                );

            Console.WriteLine(result.StatusCode);
        }
        #endregion
    }
}
