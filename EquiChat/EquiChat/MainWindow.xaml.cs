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
using System.ComponentModel;

namespace EquiChat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        private IrcBot bot;        
        private Controller controller;
        private GameScanner gamescanner;
        private bool connected;
        private string lastMessage;
        private Window about;

        public MainWindow()
        {
            InitializeComponent();                        
            controller = new Controller();
            gamescanner = new GameScanner();
            bot = new IrcBot(a => this.Dispatcher.Invoke(a), gamescanner, controller);
            chat.DataContext = bot;
            
            playersBox.ItemsSource = controller.Players;
            connected = false;
            lastMessage = "";
            chat.VerticalContentAlignment = VerticalAlignment.Bottom;
        }

        /**
         * Public functions
         */

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
            if (e.Key == Key.Up)
                message.Text = lastMessage;
            if (e.Key == Key.Down)
                message.Clear();
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            UIsendMessage();
        }

        private void UIsendMessage() 
        {
            if (connected && message.Text.Length > 0)
            {
                Action<String> writeLine;
                writeLine = delegate(string s)
                {
                    bot.sendMessage(s, Constants.ircChannel);                    
                };
                bot.Dispatcher.Invoke(DispatcherPriority.Normal, writeLine, message.Text);
                lastMessage = message.Text;
                message.Clear();
                chat.UpdateLayout();
                chat.ScrollToVerticalOffset(double.MaxValue);
            }
            else if (!connected && message.Text.Length > 0)
            {
                chat.Text += "Connect first!\r\n";
            }
        }
        
        /**
         * Private, login related functions
         */
        private void username_MouseEnter(object sender, MouseEventArgs e)
        {
            if (username.Text == "Nickname")
                username.Text = "";
        }

        private void username_MouseLeave(object sender, MouseEventArgs e)
        {
            if(username.Text == "" && !username.IsFocused)
                username.Text = "Nickname";
        }

        private void username_LostFocus(object sender, RoutedEventArgs e)
        {
            if (username.Text == "")
                username.Text = "Nickname";
        }

        private void login_Click(object sender, RoutedEventArgs e)
        {
            if (!connected)
            {
                UIlogin();
            }
            else if (connected)
            {
                UILogout();
            }
        }

        private void username_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !connected)
            {
                UIlogin();
            }
            else if (e.Key == Key.Enter && connected)
            {
                Action<String> changeNick;
                changeNick = delegate(string nick)
                {
                    bot.changeNick(nick);
                };
                bot.Dispatcher.Invoke(DispatcherPriority.Normal, changeNick, username.Text);
                message.Clear();
            }
                
        }

        private void UIlogin()
        {   
            login.Content = "Disconnect";
            bot.Start(username.Text, Constants.ircChannel, username.Text + " 8 * :LAN Party Player", Constants.ircServer, Constants.ircTechChannel);
            connected = true;
        }

        private void UILogout()
        {
            bot.Stop();            
            login.Content = "Connect";
            connected = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bot.Stop();
            controller.stop();
            gamescanner.stop();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {          
            playersBox.ItemsSource = controller.Players;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (about == null || !about.IsLoaded)
            {
                about = new Window1();
                about.Show();
            }                
        }
    }
}
