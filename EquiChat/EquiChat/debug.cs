using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EquiChat
{
    static class debugCase
    {
        private static Controller controller;

        public static void init(Controller ctrl)
        {
            controller = ctrl;
            addSomePlayers();
        }

  /*      public static void playerUpdate(string playerName, string gameName)
        {
            controller.updatePlayer(playerName, gameName);
        }
*/
        public static void addSomePlayers()
        {
            controller.addPlayer("Hugo");
            controller.addPlayer("Vincent");
            controller.addPlayer("Raymond");
        }

        public static void removePlayer(string playerName)
        {
            controller.removePlayer(playerName);
        }

        public static void debug()
        {
     //       playerUpdate("Vincent", "MOH");
        }
    }
}
