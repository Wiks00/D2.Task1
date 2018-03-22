using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BackEnd.Utils;

namespace Client.ConsoleApp
{
    class Program
    {
        private const string host = "127.0.0.1";
        private const int port = 8888;
        static TcpClient client;
        static NetworkStream stream;
        static CancellationTokenSource cts;

        static void Main(string[] args)
        {
            try
            {
                while (true)
                {
                    cts = new CancellationTokenSource();
                    var ct = cts.Token;

                    string userName = Generator.GenerateMessage();
                    client = new TcpClient();

             
                    client.Connect(host, port);
                    stream = client.GetStream();

                    byte[] data = Encoding.UTF8.GetBytes(userName);
                    stream.Write(data, 0, data.Length);

                    Task.Run(ReceiveMessage, ct);

                    Console.WriteLine($"Welcome, {userName}!");

                    SendMessage();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Disconnect();
            }
        }

        static void SendMessage()
        {
            var rdm = new Random(DateTimeOffset.UtcNow.Millisecond);
            int messageCount = rdm.Next(1, 10);

            Console.WriteLine($"Sending {messageCount} messages");

            while (messageCount >= 0)
            {
                var delay = TimeSpan.FromSeconds(rdm.Next(3, 10));

                Console.WriteLine($"freeze for {delay.Seconds} seconds");
                Task.Delay(delay).Wait();

                string message = Generator.GenerateMessage();
                Console.WriteLine(message);

                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
                messageCount--;
            }
        }

        static async Task ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    byte[] data = new byte[64]; 
                    StringBuilder builder = new StringBuilder();
                    int bytes;
                    do
                    {
                        bytes = await stream.ReadAsync(data, 0, data.Length);
                        builder.Append(Encoding.UTF8.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    string message = builder.ToString();
                    Console.WriteLine(message);
                }
                catch
                {
                    Console.WriteLine("Connection lost!");
                    Console.ReadLine();
                    Disconnect();
                }
            }
        }

        static void Disconnect()
        {
            cts.Cancel();
            //client.Client.Shutdown(SocketShutdown.Both);
            //client.Client.Close();
            stream?.Close();
            client?.Close();
        }
    }
}
