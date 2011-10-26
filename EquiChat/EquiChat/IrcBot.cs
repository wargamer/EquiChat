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
using System.ComponentModel;

namespace EquiChat {
    class IrcBot : DispatcherObject, INotifyPropertyChanged
    {   
        private static int PORT = 6667;    
        private Thread bot;
    
        private string nick;
        private string channel;
        private string techChannel;
        private string user;
        private string hostname;
        private string chatHistory;
        private string playingGame = "Nothing";
        private Action<Action> synchronousInvoker;
        private PingSender ping;
        private GameScanner gs;
        private Controller ctrl;
        private GameUpdate gu;
        private bool started = false;

        private bool connected = false;        
        private bool allowedSend = true;

        private NetworkStream stream;
        private TcpClient irc;
        private StreamReader reader;

        // StreamWriter is declared here so that PingSender can access it
        public static StreamWriter writer;

        public IrcBot(Action<Action> pSynchronousInvoker, GameScanner pGamescanner, Controller pController)
        {            
            synchronousInvoker = pSynchronousInvoker;
            gs = pGamescanner;
            gu = new GameUpdate(gs_GameLaunched);
            gs.GameLaunched += gu;
            ctrl = pController;
        }

        void gs_GameLaunched(object sender, GameUpdateEventArgs e)
        {
            if (e.state == GameUpdateEventArgs.gameState.start)
            {
                playingGame = e.gameName;
                announceGame();
                invokeController(nick, playingGame);
            }
            else if (e.state == GameUpdateEventArgs.gameState.stop)
            {
                playingGame = "";
                announceGame();
                invokeController(nick, "Nothing");                
            }
            else
            {
                throw new NotImplementedException();
            }            
        }	    

        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public string pChatHistory
        {
            get { return chatHistory; }
            set
            {
                if (value != chatHistory)
                {
                    chatHistory = (value + "\r\n");
                    synchronousInvoker(
                        () => Notify("pChatHistory")
                    );                    
                }
            }
        }

        public void Start(string pNick, string pChannel, string pUser, string pHostname, string pTechChannel)
        {            
            if (pNick == "" || pChannel == "" || pUser == "" || pHostname == "" || started == true)
                return;
            started = true;
            nick = pNick; channel = pChannel; user = pUser; hostname = pHostname; techChannel = pTechChannel;

            bot = new Thread(new ThreadStart(this.Run));            
            bot.Name = ("IRC BOT " + hostname + " " + channel);            
            
            bot.Start();
        }

        public void Stop()
        {
            if (started)
            {
                if(bot != null)
                    bot.Abort();
                started = false;
            }            
        }
    
        public void sendMessage(string line, string channelname, bool silent = false)
        {
            if (connected && allowedSend)
            {
                writer.WriteLine("PRIVMSG " + channelname + " " + line);
                writer.Flush();                
                if(!silent)
                    addLineToChat("<" + nick + "> " + line);
            }
            else
            {
                addLineToChat("Choose a valid nickname first");
            }
        }

        private string[] parseUserMessage(string rawMessage)
        {
            Match match = Regex.Match(rawMessage, @Constants.usermsgRegexp, RegexOptions.IgnoreCase);         
            if (match.Success)
            {
                // 1 = user, 2 = type (privmsg, nick), 3 = channel, 4 = message
                string[] answer = { match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value, match.Groups[4].Value };
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

        private string[] parseTechMessage(string rawMessage)
        {
            Match match = Regex.Match(rawMessage, @Constants.techmsgRegexp, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                // 1 = messagecode, 2 = nick/request, 3 = action/*, 4 = parameter/*
                // Ex:  !#! 0 whoisplaying * *
                //      !#! 1 <nick> playing game
                string[] answer = { match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value, match.Groups[4].Value };
                return answer;
            }
            else
            {
                return null;
            }
        }

        private void addLineToChat(string line)
        {
            /*
                Action<string, string> writeLine;
                writeLine = delegate(string l, string a)
                {
                    window.addLineToChat(l, a);
                };
                window.Dispatcher.BeginInvoke(DispatcherPriority.Normal, writeLine, line, action);
            */
            pChatHistory = pChatHistory + line;
        }

        private void sendNick()
        {
            writer.WriteLine("NICK " + nick);
            writer.Flush();            
        }

        private void joinChannel(string channelname)
        {
            writer.WriteLine("JOIN " + channelname);
            writer.Flush();
        }

        private void sendLogin()
        {
            writer.WriteLine("USER " + user);
            writer.Flush();
            sendNick();
            joinChannel(channel);
            joinChannel(techChannel);
        }

        private void announceGame()
        {            
            if(allowedSend) {            
                if (playingGame == "")
                {
                    sendMessage("!#! 1 <" + nick + "> playing Nothing", techChannel, true);
                }
                else
                {
                    sendMessage("!#! 1 <" + nick + "> playing " + playingGame, techChannel, true);
                }
            }
        }

        private void whois()
        {
            if(allowedSend)
                sendMessage("!#! 0 whoisplaying * *", techChannel, true);            
        }

        private void invokeController(string pFromNick, string gamename = "", string pToNick = "")
        {            
            if (gamename == "")
            {
                if (pToNick == null)
                {
                    Action<string> removePlayer;
                    removePlayer = delegate(string nick)
                    {
                        ctrl.removePlayer(nick);
                    };
                    ctrl.Dispatcher.BeginInvoke(DispatcherPriority.Normal, removePlayer, pFromNick);
                }
                else
                {
                    Action<string, string> addPlayer;
                    addPlayer = delegate(string fromNick, string toNick)
                    {
                        if (fromNick != toNick)
                            ctrl.updatePlayer(fromNick, "", toNick);
                        else
                            ctrl.updatePlayer(fromNick);
                    };
                    ctrl.Dispatcher.BeginInvoke(DispatcherPriority.Normal, addPlayer, pFromNick, pToNick);
                }
            }
            else
            {
                Action<string, string, string> updatePlayer;
                updatePlayer = delegate(string nick, string game, string newnick)
                {
                    ctrl.updatePlayer(nick, game, newnick);
                };
                ctrl.Dispatcher.BeginInvoke(DispatcherPriority.Normal, updatePlayer, pFromNick, gamename, pToNick);
            }
            
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
            // Whether or not the nick is invalid and needs to be changed
            bool invalidNick = false;
            // Whether or not the current nick has been "registered"
            bool registered = false;
            // Used to make sure we don't flood the server with nick changes,
            // It waits for some response before retrying to change the nick, if needed
            bool pendingApproval = false;
            string firstNick = nick;

            try
            {
                if (connect())
                {
                    if(!connected) connected = true;
                    ping = new PingSender(this);
                    ping.Start();
                    sendLogin();
                    while (true)
                    {         
                        inputLine = "";
                        if (invalidNick && firstNick != nick && !pendingApproval)
                        {
                            sendLogin();
                            invalidNick = false;
                            pendingApproval = true;
                        }
                        else if (!invalidNick && firstNick != nick && !pendingApproval)
                        {
                            sendNick();
                            pendingApproval = true;
                        }
                        if (irc.GetStream().DataAvailable && (inputLine = reader.ReadLine()) != "")
                        {
                            string[] message;
                            if (inputLine.Contains("PRIVMSG") && (message = parseUserMessage(inputLine)) != null && message[1] == "PRIVMSG")
                            {
                                if (message[2] == channel)
                                {                                    
                                    string temp = "<" + message[0] + "> " + message[3];
                                    addLineToChat(temp);
                                }
                                else if (message[2] == techChannel)
                                {                                    
                                    if (message[3].StartsWith("!#!"))
                                    {
                                        string[] techMessage;
                                        if ((techMessage = parseTechMessage(message[3])) != null)
                                        {
                                            if (techMessage[0] == "0")
                                            {
                                                if (techMessage[1] == "whoisplaying")
                                                {
                                                    announceGame();
                                                }
                                            }
                                            else if (techMessage[0] == "1" && techMessage[1] == message[0])
                                            {
                                                invokeController(techMessage[1], techMessage[3]);                                                
                                            }
                                            else if (techMessage[0] == "1" && techMessage[1] != message[0])
                                            {
                                                Console.WriteLine("Imposter detected, " + message[0] + " is trying to impersonate " + techMessage[1]);
                                            }
                                            else
                                            {
                                                Console.WriteLine("Tech Messagetype not recognized: " + techMessage[0]);
                                            }
                                        }
                                        
                                    }
                                    // Anything else will be ignored, probably just normal chat
                                }
                                else
                                {
                                    // Received a message from an unknown channel or just received a corrupted privmsg line
                                    Console.WriteLine("Received message from " + message[2] + ", ignoring because channel is unknown");
                                }

                            }
                            else if (inputLine.Contains("NICK") && (message = parseUserMessage(inputLine)) != null && message[1] == "NICK" && message[0] == firstNick)
                            {
                                invokeController(firstNick);
                                firstNick = nick;
                                allowedSend = true;
                                pendingApproval = false;
                                addLineToChat("Nickname successfully changed to " + message[3]);
                            }
                            else if (inputLine.Contains("NICK") && (message = parseUserMessage(inputLine)) != null && message[1] == "NICK" && message[0] != firstNick)
                            {
                                invokeController(message[0], "", message[3]);
                            }
                            else if (inputLine.Contains("QUIT") && (message = parseUserMessage(inputLine)) != null && message[1] == "QUIT")
                            {
                                invokeController(message[0], "", null);
                            }
                            else if ((message = parseAnnouncement(inputLine)) != null)
                            {
                                if (message[0] == "001")
                                {
                                    invokeController(nick);
                                    invalidNick = false;
                                    registered = true;
                                    firstNick = nick;
                                    addLineToChat((message[1]));
                                    allowedSend = true;
                                    pendingApproval = false;
                                    whois();
                                    announceGame();
                                }
                                else if (message[0] == "433")
                                {
                                    if (registered)
                                    {
                                        nick = firstNick;
                                        addLineToChat("Nickname is already in use!");
                                        pendingApproval = false;
                                    }
                                    else
                                    {
                                        addLineToChat("Nickname is already in use!");
                                        allowedSend = false;
                                        invalidNick = true;
                                        pendingApproval = false;
                                    }
                                }
                                else if (message[0] == "432")
                                {
                                    if (registered)
                                    {
                                        nick = firstNick;
                                        addLineToChat("Invalid character in nick, keep in mind that it has to start with a letter.");
                                        pendingApproval = false;
                                    }
                                    else
                                    {
                                        addLineToChat("Invalid character in nick, keep in mind that it has to start with a letter.");
                                        allowedSend = false;
                                        invalidNick = true;
                                        pendingApproval = false;
                                    }
                                }
                                else if (message[0] == "438") // Too many nick changes
                                {
                                    addLineToChat(message[1]);
                                    pendingApproval = false;
                                    nick = firstNick;
                                }

                            } 
                            //Console.WriteLine("DEBUG: " + inputLine);
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