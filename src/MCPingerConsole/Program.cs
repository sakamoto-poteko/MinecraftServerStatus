using MCServerStatus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MCPingerConsole
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IMinecraftPinger pinger = new MinecraftPinger("play.minesuperior.com", 25565);

            Task.Run(async () =>
            {
                while (true)
                {
                    var status = await pinger.RequestAsync();

                    Console.WriteLine(status.Players.Online + " people online");

                    Task.Delay(1000).Wait();
                }
            }).Wait();


        }
    }
}
