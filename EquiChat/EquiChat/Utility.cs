using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace EquiChat
{
    [DebuggerDisplay("{name} is playing {playing}")]
    class Player : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string name;
        private string playing;
        public string Name { get { return name; } private set { name = value; } }
        public string Playing { get { return playing; } set { playing = value; NotifyPropertyChanged(); } }

        public Player(string name, string playing = "")
        {
            Name = name;
            Playing = playing;
        }

        private void NotifyPropertyChanged()
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("Playing"));
            }
        }
    }
    
    class PlayerCollection : ObservableCollection<Player>
    {

    }
}
