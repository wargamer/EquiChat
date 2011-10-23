using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Windows.Threading;


using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

public delegate void addToChat(string text);
/*
* This program establishes a connection to irc server, joins a channel and greets every nickname that
* joins the channel.
*
* Coded by Pasi Havia 17.11.2001 http://koti.mbnet.fi/~curupted
*/
class IrcBot : DispatcherObject
{    
    // Irc server's port (6667 is default port)
    private static int PORT = 6667;
    // StreamWriter is declared here so that PingSender can access it
    private Thread bot;
    
    private string nick;
    private string channel;
    private string user;
    private string hostname;
    private TextBlock chat;
    static bool started = false;

    private NetworkStream stream;
    private TcpClient irc;
    private StreamReader reader;

    public static StreamWriter writer;

    public IrcBot () {        
        bot = new Thread(new ThreadStart(this.Run));
	}

    public void Start(string pNick, string pChannel, string pUser, string pHostname, TextBlock pChat)
    {
        if (pNick == "" || pChannel == "" || pUser == "" || pHostname == "" || started == true)
            return;
        started = true;
        nick = pNick; channel = pChannel; user = pUser; hostname = pHostname;
        chat = pChat;

        bot.Start();        
    }

    private object InvokeMethod(Delegate method, params object[] args)
    {
        return method.DynamicInvoke(args);
    }

    public void sendMessage(string line)
    {
        writer.WriteLine("PRIVMSG " + channel + " " + line);
        writer.Flush();
        addLineToChat("PRIVMSG " + channel + " " + line);
    }

    private void addLineToChat(string line)
    {
        Action<String> writeLine;
        writeLine = delegate(string s)
        {
            chat.Text = s + "\r\n" + chat.Text;
        };
        chat.Dispatcher.BeginInvoke(DispatcherPriority.Normal, writeLine, line);
    }

    public void Run()
    {        
        string inputLine;        

        try
        {
            if (connect())
            {
                // Start PingSender thread
                PingSender ping = new PingSender(this);
                ping.Start();
                writer.WriteLine("USER " + user);
                writer.Flush();                
                writer.WriteLine("NICK " + nick);
                writer.Flush();                
                writer.WriteLine("JOIN " + channel);                
                writer.Flush();                
                while (true)
                {
                    if ((inputLine = reader.ReadLine()) != null)
                    {
                        if (inputLine.Contains("PRIVMSG"))
                        {
                            Console.WriteLine(inputLine);
                            addLineToChat(inputLine);
                        }

                        //if (inputLine.EndsWith("JOIN :" + channel))                        
                    }
                    // Close all streams
                    //writer.Close();
                    //reader.Close();
                    //irc.Close();
                }
            } else {
                Console.WriteLine("Connection failed.");
                Thread.Sleep(5000);
                Run();
            }
        }
        catch (Exception e)
        {
            // Show the exception, sleep for a while and try to establish a new connection to irc server
            Console.WriteLine(e.ToString());
            Thread.Sleep(5000);
            Run();
        }
    }

    public string getHostname()
    {
        return hostname;
    }

    private bool connect()
    {
        try
        {
            irc = new TcpClient(hostname, PORT);
            stream = irc.GetStream();
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream);
            return true;
        } catch(Exception e) {
            Console.WriteLine(e.ToString());
            return false;
        }
    }
}