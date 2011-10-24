using System;
using System.Threading;

/*
* Class that sends PING to irc server every 15 seconds
*/
class PingSender {
	static string PING = "PING :";
	private Thread pingSender;
    private IrcBot bot;
    private int sleepTime = 15000;
	
	// Empty constructor makes instance of Thread
	public PingSender (IrcBot pBot) {
		pingSender = new Thread (new ThreadStart (this.Run) );
        bot = pBot;
	}
	
	// Starts the thread
	public void Start () {
		pingSender.Start ();
	}

    public void Stop()
    {
        
        pingSender.Abort();
    }

    private void sleepWithBreaks(int sleepTime)
    {
        int sleptTime = 0;
        while (sleptTime < sleepTime)
        {
            Thread.Sleep(10);
            sleptTime += 10;
        }
    }
	
	// Send PING to irc server every 15 seconds
	public void Run () {        
		while (true)
		{
            try
            {
                IrcBot.writer.WriteLine(PING + bot.getHostname());
                IrcBot.writer.Flush();
                sleepWithBreaks(sleepTime);
            }
            catch (ThreadAbortException abortException)
            {
                Console.WriteLine("!!PingSender caught an abort exception, stopping NOW!!");
                Console.WriteLine((string)abortException.ExceptionState);
                return;
            }
		}
	}
}