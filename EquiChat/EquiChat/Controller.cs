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
        
        public void stop()
        {            
        }

        public void debug2()
        {
            updatePlayer("Raymond", "lol");
            updatePlayer("Vincent", "MOH");
            Players.Add(new Player("kees", "BF3"));
        }

        public void addPlayer(string name)
        {
            Players.Add(new Player(name));
        }

        public void updatePlayer(string name, string game = "", string newname = "")
        {
            Player player;
            if (newname == "") newname = name;            
            IEnumerable<Player> query = Players.Where(p => p.Name == name);
            if (query.Count() > 0)
            {                
                player = query.Single();
                if (game != "") player.Playing = game;
                player.Name = newname;
            }
            else
            {
                addPlayer(newname);
                player = Players.Where(p => p.Name == newname).Single();
                if (game == "") game = "Nothing";
                player.Playing = game;
            }
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
