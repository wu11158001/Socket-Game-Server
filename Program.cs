using System;
using SocketGameServer.Servers;

namespace SocketGameServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server(6666);
            Console.WriteLine("服務端啟動");
            Console.Read();
        }
    }
}
