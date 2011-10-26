using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace EquiChat
{
    class Controller
    {
        private GameScanner gs;
        public PlayerCollection Players;

        public Controller()
        {
            gs = new GameScanner();
            Players = new PlayerCollection();
        }

        public void stop()
        {
            gs.stop();
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

        public void updatePlayer(string name, string game)
        {
            Player player = Players.Where(p => p.Name == name).Single();
            player.Playing = game;
        }

        public void removePlayer(string name)
        {
            Players.Remove(Players.Where(p => p.Name == name).Single());
        }
    }
}
