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
        protected internal NetworkStream Stream { get; private set; }
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
                string userName = message;

                message = $"{userName} entered chat";

                await server.BroadcastMessage(message, Id);
                Console.WriteLine(message);

                while (true)
                {
                    try
                    {
                        message = GetMessage();
                        message = $"{userName}: {message}";
                        Console.WriteLine(message);
                        await server.BroadcastMessage(message, Id);
                    }
                    catch
                    {
                        message = $"{userName}: left the chat";
                        Console.WriteLine(message);
                        await server.BroadcastMessage(message, Id);
                        break;
                    }
                }
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

        private string GetMessage()
        {
            byte[] data = new byte[64];
            StringBuilder builder = new StringBuilder();
            int bytes;

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
