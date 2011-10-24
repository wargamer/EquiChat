using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Windows.Threading;
using System.Text.RegularExpressions;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

class IrcBot : DispatcherObject
{   
    private static int PORT = 6667;    
    private Thread bot;
    
    private string nick;
    private string channel;
    private string user;
    private string hostname;
    private TextBlock chat;
    private PingSender ping;
    static bool started = false;

    private NetworkStream stream;
    private TcpClient irc;
    private StreamReader reader;

    // StreamWriter is declared here so that PingSender can access it
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

    public void Stop()
    {        
        bot.Abort();
    }

    private object InvokeMethod(Delegate method, params object[] args)
    {
        return method.DynamicInvoke(args);
    }

    public void sendMessage(string line)
    {
        writer.WriteLine("PRIVMSG " + channel + " " + line);
        writer.Flush();
        addLineToChat("<" + nick + "> " + line);
    }

    private string[] parseChatMessage(string rawMessage)
    {
        Match match = Regex.Match(rawMessage, @"^\:(.+)!.+@.+\sPRIVMSG\s\#[A-Z]+\s\:(.+)$", RegexOptions.IgnoreCase);         
        if (match.Success)
        {
            // 1 = user, 2 = message
            string[] answer = { match.Groups[1].Value, match.Groups[2].Value };
            return answer;
        }
        else
        {
            return null;
        }
    }

    private string[] parseAnnouncement(string rawMessage)
    {
        Match match = Regex.Match(rawMessage, @"^\:[A-Z0-9.-]*\.[A-Z0-9.-]+\.[A-Z]{2,4}\s([0-9]+)\s[^\s\t]+\s\:(.+)$", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            // 1 = messagecode, 2 = message
            string[] answer = { match.Groups[1].Value, match.Groups[2].Value };
            return answer;
        }
        else
        {
            return null;
        }
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

    private void stopNow()
    {
        Console.WriteLine("!!IrcBot caught an abort exception, stopping NOW!!");
        ping.Stop();
        writer.Close();
        reader.Close();
        irc.Close();        
    }

    private void sleepWithBreaks(int sleepTime)
    {
        int sleptTime = 0;
        while (sleptTime < sleepTime)
        {            
            Thread.Sleep(10);
            sleptTime += 10;            
        }        
    }

    public void Run()
    {
        string inputLine = "";        

        try
        {
            if (connect())
            {
                ping = new PingSender(this);
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
                        string[] message;                        
                        if (inputLine.Contains("PRIVMSG") && (message = parseChatMessage(inputLine)) != null)
                        {
                            string temp = "<" + message[0] + "> " + message[1];
                            addLineToChat(temp);
                        }
                        else if ((message = parseAnnouncement(inputLine)) != null)
                        {
                            if (message[0] == "001")
                            {
                                addLineToChat(message[1]);
                                addLineToChat("\n");
                            }
                        }
                        Console.WriteLine("!!DEBUG: " + inputLine);                        
                    }                    
                }
            }
            else
            {
                Console.WriteLine("Connection failed.");
                sleepWithBreaks(5000);
                Run();
            }
        }
        catch (ThreadAbortException abortException)
        {
            stopNow();
            return;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            sleepWithBreaks(5000);
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