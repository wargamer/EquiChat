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
    /// 

    public partial class MainWindow : Window
    {
        private IrcBot bot;
        private Controller controller;
        private bool connected;
        private bool allowedSend;        

        public MainWindow()
        {
            InitializeComponent();
            bot = new IrcBot(a => this.Dispatcher.Invoke(a));
            this.DataContext = bot;
            controller = new Controller();
            connected = false;            
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
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            UIsendMessage();
        }

        private void UIsendMessage() 
        {
            if (connected)
            {
                Action<String> writeLine;
                writeLine = delegate(string s)
                {
                    bot.sendMessage(s);
                };
                bot.Dispatcher.Invoke(DispatcherPriority.Normal, writeLine, message.Text);
                message.Clear();
            }
            else if(!connected)
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
            bot.Start(username.Text, "#chatter", username.Text + " 8 * :LAN Party Player", "uws.mine.nu");
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
            controller.stop();
            bot.Stop();
        }
    }
}
