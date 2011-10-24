﻿using System;
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

namespace EquiChat {
    class IrcBot : DispatcherObject
    {   
        private static int PORT = 6667;    
        private Thread bot;
    
        private string nick;
        private string channel;
        private string user;
        private string hostname;
        private MainWindow window;
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

        public void Start(string pNick, string pChannel, string pUser, string pHostname, MainWindow pWindow)
        {
            if (pNick == "" || pChannel == "" || pUser == "" || pHostname == "" || started == true)
                return;
            started = true;
            nick = pNick; channel = pChannel; user = pUser; hostname = pHostname;
            window = pWindow;

            bot.Name = ("IRC BOT " + hostname + " " + channel);
            bot.Start();
        }

        public void Stop()
        {        
            bot.Abort();
        }
    
        public void sendMessage(string line)
        {
            writer.WriteLine("PRIVMSG " + channel + " " + line);
            writer.Flush();
            addLineToChat("<" + nick + "> " + line, "");
        }

        private string[] parseUserMessage(string rawMessage)
        {
            Match match = Regex.Match(rawMessage, @Constants.usermsgRegexp, RegexOptions.IgnoreCase);         
            if (match.Success)
            {
                // 1 = user, 2 = type (privmsg, nick), 3 = message
                string[] answer = { match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value };
                return answer;
            }
            else
            {
                return null;
            }
        }

        private string[] parseAnnouncement(string rawMessage)
        {
            Match match = Regex.Match(rawMessage, @Constants.announcementRegexp, RegexOptions.IgnoreCase);
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

        private void addLineToChat(string line, string action)
        {
            Action<string, string> writeLine;
            writeLine = delegate(string l, string a)
            {
                window.addLineToChat(l, a);
            };
            window.Dispatcher.BeginInvoke(DispatcherPriority.Normal, writeLine, line, action);
        }

        private void sendNick()
        {
            writer.WriteLine("NICK " + nick);
            writer.Flush();
        }
        private void sendLogin()
        {
            writer.WriteLine("USER " + user);
            writer.Flush();
            sendNick();
            writer.WriteLine("JOIN " + channel);
            writer.Flush();
        }

        private void stopNow()
        {            
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

        public void changeNick(string pNick)
        {
            if(pNick != "")
                nick = pNick;
        }

        public void Run()
        {
            string inputLine = "";        
            bool invalidNick = false;
            bool registered = false;
            string firstNick = nick;

            try
            {
                if (connect())
                {                    
                    ping = new PingSender(this);
                    ping.Start();
                    sendLogin();
                    while (true)
                    {         
                        inputLine = "";
                        if (invalidNick && firstNick != nick)
                        {
                            sendLogin();
                            invalidNick = false;                            
                        }
                        else if (!invalidNick && firstNick != nick)
                        {
                            sendNick();
                        }
                        if (irc.GetStream().DataAvailable && (inputLine = reader.ReadLine()) != "")
                        {
                            string[] message;
                            if (inputLine.Contains("PRIVMSG") && (message = parseUserMessage(inputLine)) != null && message[1] == "PRIVMSG")
                            {
                                string temp = "<" + message[0] + "> " + message[1];
                                addLineToChat(temp, "");                                
                            }
                            else if (inputLine.Contains("NICK") && (message = parseUserMessage(inputLine)) != null && message[1] == "NICK")
                            {
                                firstNick = nick;
                                addLineToChat("", "allowedToSend");                                
                            } 
                            else if ((message = parseAnnouncement(inputLine)) != null)
                            {
                                if (message[0] == "001")
                                {
                                    invalidNick = false;
                                    registered = true;
                                    firstNick = nick;
                                    addLineToChat(message[1] + "\n", "allowedToSend");                                    
                                }
                                else if (message[0] == "433")
                                {
                                    if (registered)
                                    {
                                        nick = firstNick;
                                        addLineToChat("Nickname is already in use!", "resetNick");
                                    }
                                    else
                                    {
                                        addLineToChat("Nickname is already in use!", "notAllowedToSend");
                                        invalidNick = true;
                                    }                                        
                                }
                                else if (message[0] == "438") // Too many nick changes
                                {
                                    addLineToChat(message[1], "");
                                }

                            } 
                            Console.WriteLine("DEBUG: " + inputLine);
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
            catch (ThreadAbortException)
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
}