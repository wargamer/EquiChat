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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections;
using System.Threading;
using System.Windows.Threading;

namespace EquiChat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>


    public partial class MainWindow : Window
    {
        private IrcBot bot;
        private Controller controller;

        public MainWindow()
        {
            InitializeComponent();            
            bot = new IrcBot();
            controller = new Controller();
            listBox1.ItemsSource = controller.Players;
        }

        void controller_playerUpdate(Player p)
        {
            
        }

        /**
         * Public functions
         */

        public void addToChat(string text)
        {
            chat.Text = chat.Text + text;
        }

        /**
         * Private, Message related functions
         */
        
        private void message_MouseEnter(object sender, MouseEventArgs e)
        {
            if (message.Text == "Message...")
                message.Text = "";
        }

        private void message_MouseLeave(object sender, MouseEventArgs e)
        {            
            if (message.Text == "" && !message.IsFocused)
                message.Text = "Message...";            
        }

        private void message_LostFocus(object sender, RoutedEventArgs e)
        {
            if (message.Text == "")
                message.Text = "Message...";
        }

        private void message_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
                UIsendMessage();
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            UIsendMessage();
        }

        private void UIsendMessage() 
        {            
            Action<String> writeLine;
            writeLine = delegate(string s)
            {
                bot.sendMessage(s);
            };
            bot.Dispatcher.Invoke(DispatcherPriority.Normal, writeLine, message.Text);
        }
        
        /**
         * Private, login related functions
         */
        private void username_MouseEnter(object sender, MouseEventArgs e)
        {
            if (username.Text == "Username")
                username.Text = "";
        }

        private void username_MouseLeave(object sender, MouseEventArgs e)
        {
            if(username.Text == "" && !username.IsFocused)
                username.Text = "Username";
        }

        private void username_LostFocus(object sender, RoutedEventArgs e)
        {
            if (username.Text == "")
                username.Text = "Username";
        }

        private void login_Click(object sender, RoutedEventArgs e)
        {
            if (login.Content.ToString() == "Login")
            {
                UIlogin();
            }
            else if (login.Content.ToString() == "Logout")
            {
                UILogout();
            }
            
        }

        private void username_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
                UIlogin();
        }

        private void UIlogin()
        {
            username.IsReadOnly = true;
            username.Background = Brushes.LightSlateGray; 
            login.Content = "Logout";
            bot.Start(username.Text, "#chatter", username.Text + " 8 * :LAN Party Player", "uws.mine.nu", this.chat);
        }

        private void UILogout()
        {
            bot.Stop();            
            username.IsReadOnly = false;
            username.Background = Brushes.White;
            login.Content = "Login";            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            controller.stop();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            listBox1.ItemsSource = controller.Players;
           // listBox1.i
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            controller.debug2();
        }
    }
}
