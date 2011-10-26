using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EquiChat
{
    public static class Constants
    {
        public const string selectStart = "Select * From Win32_ProcessStartTrace";
        public const string selectStop = "Select * From Win32_ProcessStopTrace";
        public const string gameListXML = "../../gamelist.xml";
        public const string announcementRegexp = @"^\:[A-Z0-9.-]*\.[A-Z0-9.-]+\.[A-Z]{2,4}\s([0-9]+)\s[^\s\t]+\s[^\s\t]*\s*\:(.+)$";
        public const string usermsgRegexp = @"^\:(.+)!.+@.+\s(PRIVMSG|NICK)\s\#*[A-Z]*\s*\:(.+)$";
        public const string ircServer = "uws.mine.nu";
        public const string ircChannel = "#chatter";

    }
}
