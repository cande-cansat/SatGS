using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SatGS.Socket
{
    struct PacketData
    {
        public int Length { get; set; }
        public byte[] Data { get; set; }
    }

    struct AsyncState
    {
        public TcpClient client { get; set; }
        public byte[] data { get; set; }
    }

    internal class TcpReceiver
    {
        private static TcpReceiver instance;

        public static TcpReceiver Instance()
        {
            if (instance == null)
                instance = new TcpReceiver();
            return instance;
        }

        const int BufferSize = 640*480 + 1;

        private TcpListener listener;
        private List<TcpClient> clients;

        public event EventHandler<PacketData> PacketReceived;

        private TcpReceiver()
        {
            clients = new List<TcpClient>();
            listener = new TcpListener(IPAddress.Any, 6060);
            listener.Start();

            listener.BeginAcceptTcpClient(new AsyncCallback(AcceptCallback), listener);
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            var listener = ar.AsyncState as TcpListener;

            AsyncState state = new AsyncState();
            state.client = listener.EndAcceptTcpClient(ar);
            if(state.client != null)
            {
                state.data = new byte[BufferSize];
                clients.Add(state.client);
                state.client.Client.BeginReceive(state.data, 0, BufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallback), state);
            }

            listener.BeginAcceptSocket(new AsyncCallback(AcceptCallback), listener);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            var state = (AsyncState)ar.AsyncState;
            var client = state.client;
            var received = client.Client.EndReceive(ar, out var error);

            if(error != SocketError.Success)
            {
                clients.Remove(client);
                CleanUpClient(client);
                return;
            }

            if (received <= 0) return;

            PacketReceived?.Invoke(this, new PacketData()
            {
                Length = received,
                Data = state.data
            });

            state.data = new byte[BufferSize];

            client.Client.BeginReceive(state.data, 0, BufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallback), state);
        }

        private void CleanUpClient(TcpClient client)
        {
            if (client.Connected)
            {
                client.Client.Disconnect(false);
            }
            client.Dispose();
        }

        ~TcpReceiver()
        {
            foreach (var client in clients)
                CleanUpClient(client);
            clients.Clear();
            listener.Stop();
        }
    }
}