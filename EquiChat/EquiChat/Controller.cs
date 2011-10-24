using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace EquiChat
{
    class Controller
    {
        private GameScanner gs;

        public Controller()
        {
            gs = new GameScanner();
            gs.GameLaunched += new ChangedEventHandler(gs_GameUpdated);
        }

        private void gs_GameUpdated(object sender, GameUpdateEventArgs e)
        {
            Debug.WriteLine(e.gameName + " " + e.state.ToString());
        }

        public void stop()
        {
            gs.stop();
        }

    }
}
