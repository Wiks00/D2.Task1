using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BackEnd;

namespace Server.ConsoleApp
{
    class Program
    {
        static BackEnd.Server server;

        static void Main(string[] args)
        {

            try
            {
                server = BackEnd.Server.Instance;
                Thread listenThread = new Thread(server.BeginListen);
                listenThread.Start();
            }
            catch (Exception ex)
            {
                server.EndListen();
                Console.WriteLine(ex.Message);
            }

        }
    }
}
