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

        public MainWindow()
        {
            InitializeComponent();            
            bot = new IrcBot();            
        }

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

        public void addToChat(string text)
        {
            chat.Text = chat.Text + text;
        }

        private void login_Click(object sender, RoutedEventArgs e)
        {
            TextBlock chat = this.chat;
            bot.Start(username.Text, "#chatter", "CSharpBot 8 * :I'm a C# irc bot", "uws.mine.nu", chat);             
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            Action<String> writeLine;
            writeLine = delegate(string s)
            {
                bot.sendMessage(s);
            };
            bot.Dispatcher.Invoke(DispatcherPriority.Normal, writeLine, message.Text);
        }
    }
}
