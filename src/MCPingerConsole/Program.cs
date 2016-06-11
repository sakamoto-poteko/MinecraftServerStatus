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
            MCPinger pinger = new MCPinger("mc.afa.moe", 25565);

            Task.Run(async () =>
            {
                var status = await pinger.Ping();

                int a = 0;
            }).Wait();


        }
    }
}
