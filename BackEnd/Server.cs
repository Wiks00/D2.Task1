using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd
{
    public sealed class Server
    {
        private static readonly Lazy<Server> lazy =
            new Lazy<Server>(() => new Server());

        private readonly TcpListener tcpListener;
        private readonly List<Client> clients;
        private readonly List<string> messages;

        public ReadOnlyCollection<string> History { get; }

        public static Server Instance => lazy.Value;

        private Server()
        {
            tcpListener = new TcpListener(IPAddress.Any, 8888);
            clients = new List<Client>();
            messages = new List<string>(20);

            History = new ReadOnlyCollection<string>(messages);
        }

        public void AddConnection(Client clientObject)
        {
            clients.Add(clientObject);
        }

        public void RemoveConnection(string id)
        {
            Client client = clients.FirstOrDefault(c => c.Id == id);
            clients?.Remove(client);
        }

        public void BeginListen()
        {
            try
            {
                tcpListener.Start();
                Console.WriteLine("Server was started. Waiting for connections...");

                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();

                    Client clientObject = new Client(tcpClient, this);
                    Task.Run(clientObject.BeginProcess);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                EndListen();
            }
        }

        public async Task SendHistory(string id)
        {
            Client client = clients.First(item => item.Id.Equals(id, StringComparison.InvariantCulture));

            byte[] header_footer = Encoding.UTF8.GetBytes("--- History ---" + Environment.NewLine);

            await client.Stream.WriteAsync(header_footer, 0, header_footer.Length);

            foreach (var message in History)
            {
                byte[] data = Encoding.UTF8.GetBytes(message + Environment.NewLine);
                await client.Stream.WriteAsync(data, 0, data.Length);
            }

            await client.Stream.WriteAsync(header_footer, 0, header_footer.Length);
        }

        public async Task BroadcastMessage(string message, string id)
        {
            if (messages.Count >= 20)
            {
                for (int i = 1; i < messages.Count; i++)
                {
                    if (i == messages.Count - 1)
                    {
                        messages[i] = message;
                    }
                    else
                    {
                        messages[i - 1] = messages[i];
                    }
                }
            }
            else
            {
                messages.Add(message);
            }

            byte[] data = Encoding.UTF8.GetBytes(message);

            foreach (Client client in clients)
            {
                if (!client.Id.Equals(id, StringComparison.InvariantCulture) && client.Stream.DataAvailable)
                {
                    await client.Stream.WriteAsync(data, 0, data.Length);
                }
            }
        }

        public void EndListen()
        {
            tcpListener.Stop();

            foreach (Client client in clients)
            {
                client.EndProcess();
            }

            Environment.Exit(0);
        }
    }
}
