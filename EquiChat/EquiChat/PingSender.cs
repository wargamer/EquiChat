using System;
using System.Threading;

/*
* Class that sends PING to irc server every 15 seconds
*/
class PingSender {
	static string PING = "PING :";
	private Thread pingSender;
    private IrcBot bot;
	
	// Empty constructor makes instance of Thread
	public PingSender (IrcBot pBot) {
		pingSender = new Thread (new ThreadStart (this.Run) );
        bot = pBot;
	}
	
	// Starts the thread
	public void Start () {
		pingSender.Start ();
	}
	
	// Send PING to irc server every 15 seconds
	public void Run () {
		while (true)
		{
			
            IrcBot.writer.WriteLine (PING + bot.getHostname());
			IrcBot.writer.Flush ();
			Thread.Sleep (15000);
		}
	}
}