using System;
using System.Collections.Generic;
//using System.Text;
using System.Xml;
using System.IO;
using System.Diagnostics;
//using System.Linq;

namespace EquiChat
{
    public delegate void ChangedEventHandler(object sender, EventArgs e);

    class GameScanner
    {
        private const string defaultPath = "../../gamelist.txt";
        private Dictionary<string, string> gameList = new Dictionary<string, string>();
        public string currentlyPlaying { get; private set; }
        public event ChangedEventHandler GameLaunched;

        public System.Management.ManagementEventWatcher mgmtWtch;

        private void onGameStart(EventArgs e)
        {
            if (GameLaunched != null)
                GameLaunched(this, e);
        }

        public GameScanner(string pathToGameList = defaultPath)
        {
            var doc = new XmlDocument();
            using (StreamReader reader = new StreamReader(pathToGameList))
            {
                var list = reader.ReadToEnd();
                doc.LoadXml(list);
            }
            var games = doc.GetElementsByTagName("game");
            foreach (XmlNode node in games)
            {
                var key = node.SelectSingleNode("pname").InnerText;
                var value = node.SelectSingleNode("name").InnerText;
                gameList.Add(key + ".exe", value);
            }
            mgmtWtch = new System.Management.ManagementEventWatcher("Select * From Win32_ProcessStartTrace");
            mgmtWtch.EventArrived += new System.Management.EventArrivedEventHandler(mgmtWtch_EventArrived);
            mgmtWtch.Start();
        }

        void mgmtWtch_EventArrived(object sender, System.Management.EventArrivedEventArgs e)
        {
            var launchedProcess = (string)e.NewEvent["ProcessName"];
            Debug.WriteLine(launchedProcess);
            string gameMatched;
            if (!gameList.TryGetValue(launchedProcess, out gameMatched))
                return;
            currentlyPlaying = gameMatched;
            onGameStart(EventArgs.Empty);
        }

        public void stop()
        {
            mgmtWtch.Stop();
        }

        //public string currentlyPlaying()
        //{
        //    var runningGame = from process in System.Diagnostics.Process.GetProcesses()
        //                      where gameList.Keys.Contains(process.ProcessName)
        //                      select process;
        //    if (runningGame.ToList().Count == 0)
        //        return null;
        //    var game = runningGame.ToList()[0].ProcessName;
        //    string value = null;
        //    gameList.TryGetValue(game, out value);
        //    return value;
        //}
    }
}
