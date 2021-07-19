using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

namespace Gerador_Audio.Classes
{
    public class Ferramentas
    {
        public static BitmapImage GetImagemProjeto(string name)
        {
            string path = "pack://application:,,,/Imagens/" + name;
            BitmapImage imagem = new BitmapImage(new Uri(path, UriKind.Absolute));
            return imagem;
        }

        public static ImageBrush GetImagemButton(string img)
        {
            var brushImg = new ImageBrush();
            brushImg.ImageSource = GetImagemProjeto(img);
            return brushImg;
        }
       
    }

}
