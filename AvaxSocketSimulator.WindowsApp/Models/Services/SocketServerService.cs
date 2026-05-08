using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace AvaxSocketSimulator.WindowsApp.Models.Services
{
    public class SocketServerService : IDisposable
    {
        public string ServerAddress { get; }

        public int ServerPort { get; }

        protected TcpClient Client { get; }

        protected NetworkStream NetworkStream { get; }

        public SocketServerService(string serverAddress, int serverPort)
        {
            ServerAddress = serverAddress;
            ServerPort = serverPort;

            try
            {
                Client = new TcpClient(serverAddress, serverPort);

                NetworkStream = Client.GetStream();
            }
            catch (Exception e)
            {
                Client = null;
                NetworkStream = null;
            }
        }

        public void Send(string entry)
        {
            if (NetworkStream != null)
            {
                byte[] data = ParseDataToBytes(entry).ToArray();

                NetworkStream.Write(data, 0, data.Length);
            }
        }

        public void Dispose() => 
            Client?.Dispose();

        private IEnumerable<byte> ParseDataToBytes(string entry)
        {
            IList<byte> data = entry.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(byte.Parse)
                .Concat(new byte[]{13, 10})
                .ToList();
            
            return data;
        }
    }
}