using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client.ConsoleApp
{
    class Program
    {
        private const string host = "127.0.0.1";
        private const int port = 8888;
        static TcpClient client;
        static NetworkStream stream;

        static void Main(string[] args)
        {
            Console.Write("Enter client name: ");
            string userName = Console.ReadLine()?.Trim();
            client = new TcpClient();
            try
            {
                client.Connect(host, port);
                stream = client.GetStream();

                string message = string.IsNullOrEmpty(userName) ? "Anonym" : userName;
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);

                ReceiveMessage();

                Console.WriteLine($"Welcome, {userName}!");

                SendMessage();
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
                string message = SentenceGenerator.Generator.GenerateMessage();
                Console.WriteLine(message);

                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
                messageCount--;

                var delay = TimeSpan.FromSeconds(rdm.Next(3, 10));

                Console.WriteLine($"freeze for {delay.Seconds} seconds");
                Task.Delay(delay).Wait();
            }

            while (true)
            {
                Console.ReadKey();
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
            stream?.Close();
            client?.Close();
            Console.ReadKey();
            Environment.Exit(0);
        }
    }
}
