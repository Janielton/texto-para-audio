using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Gerador_Audio
{
    /// <summary>
    /// Lógica interna para WParamento.xaml
    /// </summary>
    public partial class WParamento : Window
    {
        int _referencia;
        public WParamento()
        {
            InitializeComponent();
        }
        public WParamento(string referencia)
        {
            _referencia = referencia.Equals("pitch") ? 1 : referencia.Equals("rate") ? 2 : referencia.Equals("break") ? 3 : 4;
            InitializeComponent();
            editParam.Focus();
            if (_referencia == 1)
            {
                SetTom();
            }
            else if (_referencia == 2)
            {
                SetVelocidade();
            }
            else if (_referencia == 3)
            {
                stakTime.Visibility = Visibility.Visible;
                tbTitulo.Text = "Pausa na fala";
            }
            else
            {
                tbTitulo.Text = referencia;
            }

        }

        private void SetVelocidade()
        {
            this.Height = 300;
            tbTitulo.Text = "Velocidade da vóz";
            List<Item> list = new List<Item>();
            list.Add(new Item("diminui em 50%", "x-slow"));
            list.Add(new Item("diminui em 25%", "slow"));
            list.Add(new Item("aumenta em 25%", "fast"));
            list.Add(new Item("aumenta em 50%", "x-fast"));
            list.Add(new Item("aumenta em 70%", "+70%"));
            listItems.ItemsSource = list;
            listItems.Visibility = Visibility.Visible;
        }
        private void SetTom()
        {
            this.Height = 400;
            tbTitulo.Text = "Tom da vóz";
            List<Item> list = new List<Item>();
            list.Add(new Item("tom baixo - 12 semitons", "x-low"));
            list.Add(new Item("tom baixo - 6 semitons", "low"));
            list.Add(new Item("tom alto - 6 semitons", "high"));
            list.Add(new Item("tom alto - 12 semitons", "x-high"));
            list.Add(new Item("transpor para 150Hz", "150Hz"));
            list.Add(new Item("aumentar para 20Hz", "+20Hz"));
            list.Add(new Item("diminuir para -20Hz", "-20Hz"));
            listItems.ItemsSource = list;
            listItems.Visibility = Visibility.Visible;
        }
        private void btClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

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

        private void editParam_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (_referencia != 3)
                {
                    MainWindow._paramento = editParam.Text;
                }
                else
                {
                    string t = radMS.IsChecked == true ? "ms" : "s";
                    MainWindow._paramento = editParam.Text +""+t;
                }
               
                DialogResult = true;
            }
        }
    }

    class Item
    {
       public string Nome { get; set; }
       public string Tag { get; set; }
        public Item(string nome, string tag)
        {
            Nome = nome;
            Tag = tag;
        }

    }
}
