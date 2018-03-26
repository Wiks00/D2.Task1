using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd
{
    public class Client
    {
        protected internal string Id { get; }
        protected internal NetworkStream Stream { get; set; }

        public string Name { get; private set; }

        TcpClient client;
        Server server; 

        public Client(TcpClient tcpClient, Server serverObject)
        {
            Id = Guid.NewGuid().ToString();
            client = tcpClient;
            server = serverObject;
            serverObject.AddConnection(this);
        }

        public async Task BeginProcess()
        {
            try
            {
                Stream = client.GetStream();

                string message = GetMessage();
                Name = message;

                message = $"'{Name}' entered chat ";

                await server.SendHistory(Id);
                await server.BroadcastMessage(message, Id);
                Console.WriteLine(message);

                while (client.Available == 0)
                {
                    try
                    {
                        message = GetMessage();

                        if (CheckIfClientConnected())
                        {
                            message = $"{Name}: {message}";

                            await ProcessMessage(message);
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch
                    {
                        break;
                    }
                }

                message = $"{Name}: left the chat";

                await ProcessMessage(message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                server.RemoveConnection(Id);
                EndProcess();
            }
        }

        private async Task ProcessMessage(string message)
        {
            Console.WriteLine(message);

            await server.BroadcastMessage(message, Id);
        }

        private bool CheckIfClientConnected()
        {
            if (client.Client.Poll(0, SelectMode.SelectRead))
            {
                byte[] buff = new byte[1];
                if (client.Client.Receive(buff, SocketFlags.Peek) == 0)
                {
                    return false;
                }
            }
            return true;
        }

        private string GetMessage()
        {
            byte[] data = new byte[64];
            StringBuilder builder = new StringBuilder();
            int bytes;

            //do
            //{
            //    Task.Delay(TimeSpan.FromMilliseconds(10)).Wait();
            //} while (client.Available == 0);

            do
            {
                bytes = Stream.Read(data, 0, data.Length);
                builder.Append(Encoding.UTF8.GetString(data, 0, bytes));
            } while (Stream.DataAvailable);

            return builder.ToString();
        }

        protected internal void EndProcess()
        {
            Stream?.Close();
            client?.Close();
        }
    }
}
