using System;
using Pro.Sockets.Lib;

namespace Pro.Sockets.Server.Cons
{
    class Program
    {
        static void Main(string[] args)
        {
            Lib.Server.Listener(80);
            
        }
    }
}
