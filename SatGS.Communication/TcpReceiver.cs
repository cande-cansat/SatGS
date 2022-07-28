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

namespace SatGS.Communication
{
    public class TcpReceiver
    {
        struct AsyncState
        {
            public TcpClient client { get; set; }
            public byte[] data { get; set; }
        }

        private static TcpReceiver instance;

        public static TcpReceiver Instance()
        {
            if (instance == null)
                instance = new TcpReceiver();
            return instance;
        }

        const int BufferSize = 1024;

        private TcpListener listener;
        private List<TcpClient> clients;

        public event EventHandler<byte[]> PacketReceived;

        private TcpReceiver()
        {
            ReceivingBuffer = new List<byte>();

            clients = new List<TcpClient>();
            listener = new TcpListener(IPAddress.Any, 6060);
            listener.Start();

            listener.BeginAcceptTcpClient(AcceptCallback, listener);
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            var listener = ar.AsyncState as TcpListener;

            try
            {
                AsyncState state = new AsyncState
                {
                    client = listener.EndAcceptTcpClient(ar)
                };

                if (state.client != null)
                {
                    state.data = new byte[BufferSize];
                    clients.Add(state.client);
                    state.client.Client.BeginReceive(state.data, 0, BufferSize, SocketFlags.None, ReceiveCallback, state);
                }

                listener.BeginAcceptSocket(AcceptCallback, listener);
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private int BytesHasRead { get; set; }
        private int BytesToRead { get; set; }
        private List<byte> ReceivingBuffer { get; set; }

        private void ReceiveCallback(IAsyncResult ar)
        {
            var state = (AsyncState)ar.AsyncState;
            var client = state.client;


            try
            {
                var received = client.Client.EndReceive(ar, out var error);

                if (error != SocketError.Success)
                {
                    clients.Remove(client);
                    CleanUpClient(client);
                    return;
                }

                // int형의 파일 크기를 받을 경우
                if (received == 4)
                {
                    BytesHasRead = 0;
                    BytesToRead = BitConverter.ToInt32(state.data, 0);
                }
                else
                {
                    BytesHasRead += received;
                    state.data.ToList().ForEach(ReceivingBuffer.Add);

                    if (BytesHasRead == BytesToRead)
                    {
                        PacketReceived?.Invoke(this, ReceivingBuffer.ToArray());

                        ReceivingBuffer.Clear();
                        BytesToRead = 0;
                    }
                }


                state.data = new byte[BufferSize];

                client.Client.BeginReceive(state.data, 0, BufferSize, SocketFlags.None, ReceiveCallback, state);
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void CleanUpClient(TcpClient client)
        {
            if (client.Connected)
            {
                client.Client.Disconnect(false);
            }
            client.Dispose();
        }

        public void CleanUpSocket()
        {
            foreach (var client in clients)
                CleanUpClient(client);
            clients.Clear();
            
            listener.Server.Dispose();
            listener.Stop();
        }

        ~TcpReceiver()
        {
            CleanUpSocket();
        }
    }
}