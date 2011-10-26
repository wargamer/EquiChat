using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace EquiChat
{
/*    [DebuggerDisplay("{name} is playing {playingvalue}")]
    class Player : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;// = delegate { };
        private string name;
        private string playingvalue;
        public string Name
        {
            get {
                return name;
            }
            private set {
                name = value;
            }
        }

        public string Playing
        {
            get {
                return playingvalue;
            }
            set {
                playingvalue = value;
                NotifyPropertyChanged();
            }
        }

        public Player(string name, string playing = "")
        {
            Name = name;
            Playing = playing;
        }

        /*
         * hier zit het probleem. handler = null, wat betekent dat er niemand aan het event gesubscribed is :(
         */ 
     /*   protected void NotifyPropertyChanged()
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs("Playing"));
        }

        public override string ToString()
        {
            if (playingvalue == string.Empty)
                return Name;
            else
                return Name + ", playing " + Playing;
        }
    }*/
    
    class PlayerCollection : ObservableCollection<Player>
    {
 /*       public event NotifyCollectionChangedEventHandler handler;

        private void NotifyPropertyChanged()
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("Playing"));
        }

        public void updatePlayer(string name, string game)
        {
            foreach(Player p in Items)
                if (p.Name == name)
                {
                    p.Playing = game;
                    NotifyPropertyChanged();
                }
        }*/
        
        //dummy class
    }


    [DebuggerDisplay("{name} is playing {playing}")]
    class Player : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string name;
        private string playing;
        public string Name { get { return name; } private set { name = value; } }
        public string Playing { get { return playing; } set { playing = value; displayName = Name + ", playing " + playing; } }

        public Player(string name, string playing = "")
        {
            Name = name;
            Playing = playing;
        }

        private string displayname;
        public string displayName
        {
            get
            {
                if (playing == string.Empty)
                    return Name;
                else
                {
                    return displayname;
                }
            }
            set
            {
                displayname = value;
                NotifyPropertyChanged("displayName");
            }
        }

        private void NotifyPropertyChanged(string which)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(which));
            }
        }
    }
}
