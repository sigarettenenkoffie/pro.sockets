using System;
using System.Threading.Tasks;

namespace Pro.Sockets.Client.Cons
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string hostname = "VEGA";
            int port = 80;
            await Lib.Client.SendAndReceiveAsync(hostname, port);
        }
    }
}
