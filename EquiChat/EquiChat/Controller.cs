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
        
        public void debug2()
        {
            updatePlayer("Raymond", "lol");
            updatePlayer("Vincent", "MOH");
            Players.Add(new Player("kees", "BF3"));
        }

        public void clearPlayers()
        {
            Players.Clear();
        }

        public void addPlayer(string name)
        {
            Players.Add(new Player(name));
        }

        public bool updatePlayer(string name, string game = "", string newname = "")
        {
            Player player;
            if (newname == string.Empty) newname = name;            
            IEnumerable<Player> query = Players.Where(p => p.Name == name);
            if (query.Count() > 0)
            {
                player = query.Single();
                if (game != string.Empty) player.Playing = game;
                player.Name = newname;
                return true;
            }
            else
                return false;
        }

        public void removePlayer(string name)
        {
            IEnumerable<Player> query = Players.Where(p => p.Name == name);
            if (query.Count() > 0)
            {
                Players.Remove(query.Single());
            }
            
        }
    }
}
