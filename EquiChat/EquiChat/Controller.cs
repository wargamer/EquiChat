using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace EquiChat
{
    class Controller : DispatcherObject
    {
        public PlayerCollection Players;

        public Controller()
        {
            Players = new PlayerCollection();
        }

        public bool addPlayer(string name)
        {
            if (Players.FirstOrDefault(p => p.Name == name) != null)
                return false;
            else
                Players.Add(new Player(name));
            return true;
        }

        public bool removePlayer(string name)
        {
            Player toRemove = Players.FirstOrDefault(p => p.Name == name);
            if (toRemove == null)
                return false;
            Players.Remove(toRemove);
            return true;
        }

        public bool setPlayerName(string name, string newname)
        {
            Player playerToUpdate = Players.FirstOrDefault(p => p.Name == name);
            if (playerToUpdate == null || string.IsNullOrWhiteSpace(newname))
                return false;

            playerToUpdate.Name = newname;
            return true;
        }

        public bool setPlayerGame(string name, string game)
        {
            Player playerToUpdate = Players.FirstOrDefault(p => p.Name == name);
            if (playerToUpdate == null)
                return false;

            playerToUpdate.Playing= game;
            return true;
        }

        public void clearPlayers()
        {
            Players.Clear();
        }
    }
}
