﻿using System;
using System.Collections.Generic;
//using System.Text;
using System.Xml;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.Text;

namespace EquiChat
{
    public delegate void GameUpdate(object sender, GameUpdateEventArgs e);

    class GameScanner
    {
        private Dictionary<string, string> gameList = new Dictionary<string, string>();
        public string currentlyPlaying { get; private set; }
        public event GameUpdate GameLaunched;

        public System.Management.ManagementEventWatcher ProcessStartWatcher;
        public System.Management.ManagementEventWatcher ProcessStopWatcher;

        private void onGameStart(GameUpdateEventArgs e)
        {
            if (GameLaunched != null)
                GameLaunched(this, e);
        }

        private static GameScanner instance;
        public static GameScanner getInstance()
        {
            if (instance == null)
                instance = new GameScanner();
            return instance;
        }

        private GameScanner()
        {
            fetchGameList();
            
            ProcessStartWatcher = new System.Management.ManagementEventWatcher(Constants.selectStart);
            ProcessStopWatcher = new System.Management.ManagementEventWatcher(Constants.selectStop);

            ProcessStartWatcher.EventArrived += new System.Management.EventArrivedEventHandler(ProcessUpdateWatcher_EventArrived);
            ProcessStopWatcher.EventArrived += new System.Management.EventArrivedEventHandler(ProcessUpdateWatcher_EventArrived);

            ProcessStartWatcher.Start();
            ProcessStopWatcher.Start();
        }

        public void fetchGameList()
        {
            gameList.Clear();
            var doc = new XmlDocument();
            var list = "";
            if (Constants.gameListURL != string.Empty)
            {
                try
                {
                    WebClient webclient = new WebClient();
                    Stream stream = webclient.OpenRead(Constants.gameListURL);
                    StreamReader reader = new StreamReader(stream);

                    list = reader.ReadToEnd();
                    stream.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    list = string.Empty;
                }
            }
            if (list == string.Empty || Constants.gameListURL == string.Empty)
            {
                using (StreamReader reader = new StreamReader(Constants.gameListXML))
                {
                    list = reader.ReadToEnd();
                }
            }
            doc.LoadXml(list);
            var games = doc.GetElementsByTagName("game");
            foreach (XmlNode node in games)
            {
                var key = node.SelectSingleNode("pname").InnerText;
                var value = node.SelectSingleNode("name").InnerText;
                gameList.Add(key + ".exe", value);
            }
        }

        private void ProcessUpdateWatcher_EventArrived(object sender, System.Management.EventArrivedEventArgs e)
        {           
            string gameName = getGameNameFromProc((string)e.NewEvent["ProcessName"]);
            if (gameName == string.Empty)
                return;
            System.Management.ManagementEventWatcher MEW = (System.Management.ManagementEventWatcher)sender;
            if (MEW.Query.QueryString == Constants.selectStart)
            {
                currentlyPlaying = gameName;
                onGameStart(new GameUpdateEventArgs(gameName, GameUpdateEventArgs.gameState.start));
            }
            else if (MEW.Query.QueryString == Constants.selectStop)
            {
                onGameStart(new GameUpdateEventArgs(gameName, GameUpdateEventArgs.gameState.stop));
                currentlyPlaying = string.Empty;
            }
            else                            
                throw new System.PlatformNotSupportedException();
        }

        private string getGameNameFromProc(string procName)
        {
            string gameMatched;
            if (!gameList.TryGetValue(procName, out gameMatched))
                return string.Empty;
            return gameMatched;
        }

        public void stop()
        {
            ProcessStartWatcher.Stop();
            ProcessStopWatcher.Stop();
        }

/*        public string donePlaying()
        {
            var runningGame = from process in System.Diagnostics.Process.GetProcesses()
                              where gameList.Keys.Contains(process.ProcessName)
                              select process;
            if (runningGame.ToList().Count == 0)
                return null;
            var game = runningGame.ToList()[0].ProcessName;
            string value = null;
            gameList.TryGetValue(game, out value);
            return value;
        }*/
    }

    public class GameUpdateEventArgs : EventArgs
    {
        public string gameName;
        public enum gameState
        {
            start,
            stop
        }
        public gameState state;
        public GameUpdateEventArgs(string gameName, gameState state)
        {
            this.gameName = gameName;
            this.state = state;
        }
    }
}

