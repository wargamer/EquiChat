using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EquiChat
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
        }

        private void label1_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            text.Text = "\t\tEquichat\n\nA simple chat client that uses IRC to communicate.\nIt shows the players that are online and which game they are playing.\nAnd it is open source.\n\n" +
            "Authors:\n" +
            "Raymond Meerburg\n" +
            "Hugo Meijer\n" +
            "Vincent Spaa\n";
        }
    }
}
