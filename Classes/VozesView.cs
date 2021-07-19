using IBM.Watson.TextToSpeech.v1.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gerador_Audio.Classes
{
    public class VozesView
    {
        public string Nome { get; set; }
        public string Linguagem { get; set; }
        public string Genero { get; set; }
        public string Descricao { get; set; }
        public bool Definido { get; set; }

    }
}
