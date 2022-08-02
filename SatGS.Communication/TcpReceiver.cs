using SatGS.SateliteData;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
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
        class AsyncState
        {
            public Socket client { get; set; }
            public byte[] data { get; set; }

            public int received { get; set; }
        }

        private static TcpReceiver instance;

        public static TcpReceiver Instance()
        {
            if (instance == null)
                instance = new TcpReceiver();
            return instance;
        }

        const int BufferSize = 8192;

        private TcpListener listener;
        private List<Socket> clients;

        public event EventHandler<SateliteImage> PacketReceived;

        //private Thread acceptThread;
        private List<Thread> receiveThreads;

        private TcpReceiver()
        {
            clients = new List<Socket>();
            listener = new TcpListener(IPAddress.Any, 6060);
            listener.Start();

            receiveThreads = new List<Thread>();

            /*
            acceptThread = new Thread(AcceptCallback);
            acceptThread.Start();
            */

            fileLen = 0;
            receivingBuffer = new List<byte>();

            listener.BeginAcceptSocket(AcceptCallback, null);
        }


        private void AcceptCallback(IAsyncResult ar)
        {
            var client = listener.EndAcceptSocket(ar);

            if (client != null)
            {
                client.Blocking = true;
                client.ReceiveBufferSize = ushort.MaxValue;

                clients.Add(client);
                /*
                var state = new AsyncState()
                {
                    client = client,
                    data = new byte[BufferSize]
                };
                client.BeginReceive(state.data, 0, 4, SocketFlags.None, ReceiveCallback, state);
                */

                var receiveThread = new Thread(ReceiveCallback);
                receiveThreads.Add(receiveThread);
                receiveThread.Start(client);
            }

            listener.BeginAcceptTcpClient(AcceptCallback, null);
        }

        private int fileLen { get; set; }
        private int current { get; set; }

        private List<byte> receivingBuffer { get; set; }

        private void ReceiveCallback(IAsyncResult ar)
        {
            var state = (AsyncState)ar.AsyncState;

            var client = state.client;

            var received = client.EndReceive(ar);

            if(fileLen == 0)
            {
                if (received == 4)
                {
                    current = 0;
                    fileLen = BitConverter.ToInt32(state.data, 0);
                }
            }
            else
            {
                current += received;
                Enumerable.Range(0, received).ToList().ForEach(i => receivingBuffer.Add(state.data[i]));

                if(fileLen <= current)
                {
                    client.Send(new byte[] { 0 }, 0, 1, SocketFlags.None);
                    //PacketReceived?.Invoke(this, receivingBuffer.ToArray());

                    PacketReceived?.Invoke(this, null);

                    receivingBuffer.Clear();
                    fileLen = 0;
                }
            }

            if(fileLen != 0)
                client.BeginReceive(state.data, 0, Math.Min(fileLen - current, BufferSize), SocketFlags.None, ReceiveCallback, state);
            else
                client.BeginReceive(state.data, 0, 4, SocketFlags.None, ReceiveCallback, state);
        }

        private void InvokeCallback(IAsyncResult ar)
        {
            PacketReceived?.EndInvoke(ar);
        }

        private void AcceptCallback()
        {
            while (listener.Server.IsBound)
            {
                try
                {
                    var client = listener.AcceptSocket();

                    if (client == null) continue;

                    clients.Add(client);
                    var receiveThread = new Thread(ReceiveCallback);
                    receiveThreads.Add(receiveThread);
                    receiveThread.Start(client);
                }
                catch(ThreadAbortException)
                {
                    return;
                }
                catch(Exception e)
                {
                    MessageBox.Show(e.Message, e.GetType().ToString());
                }
            }
        }

        private void ReceiveCallback2(IAsyncResult ar)
        {
            var state = ar.AsyncState as AsyncState;

            state.received = state.client.EndReceive(ar);
        }

        private void SendCallback2(IAsyncResult ar)
        {
            var client = ar.AsyncState as Socket;
            client.EndSend(ar);
        }

        private void ReceiveCallback(object param)
        {
            var client = param as Socket;
            try
            {
                while (client.Connected)
                {
                    try
                    {
                        var lenState = new AsyncState
                        {
                            client = client,
                            data = new byte[4]
                        };

                        lenState.received = client.Receive(lenState.data, 0, 4, SocketFlags.None);

                        if (lenState.received != 4) continue;

                        var fileLen = BitConverter.ToInt32(lenState.data, 0);

                        if (fileLen <= 0) continue;

                        /*
                        var imageState = new AsyncState
                        {
                            client = client,
                            data = new byte[fileLen]
                        };
                        */

                        var current = 0;


                        List<byte> recvBuf = new List<byte>();

                        do
                        {
                            var toRecv = Math.Min(BufferSize, fileLen - current);

                            var buffer = new byte[toRecv];

                            var recv = client.Receive(buffer, 0, toRecv, SocketFlags.None);

                            Enumerable.Range(0, recv).ToList().ForEach(i => recvBuf.Add(buffer[i]));

                            current += recv;

                        } while (fileLen > current);


                        //client.BeginReceive(imageState.data, 0, fileLen, SocketFlags.None, ReceiveCallback2, imageState).AsyncWaitHandle.WaitOne();

                        var fileName = $"{DateTime.Now:MM.dd_HH.mm.ss.fff}.jpg";
                        var path = $"{Directory.GetCurrentDirectory()}\\Images\\{fileName}";

                        using(var stream = new FileStream(path, FileMode.Create))
                        {
                            stream.WriteAsync(recvBuf.ToArray(), 0, fileLen).Wait();
                            stream.Flush();
                        }

                        PacketReceived?.Invoke(this, new SateliteImage(path));

                        client.Send(new byte[] { 0 }, 0, 1, SocketFlags.None);

                    }
                    catch (ThreadAbortException)
                    {
                        return;
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, e.GetType().ToString());
                        CleanUpClient(client);
                    }
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        

        private void CleanUpClient(Socket client)
        {
            if (clients.Exists((e) => e == client))
                clients.Remove(client);
            if (client.Connected)
                client.Disconnect(false);
            client.Dispose();
        }

        public void CleanUpSocket()
        {
            foreach (var client in clients)
                CleanUpClient(client);
            clients.Clear();
            
            foreach(var thread in receiveThreads)
            {
                thread.Abort();
            }
            receiveThreads.Clear();

            listener.Stop();
            //acceptThread.Abort();
        }

        ~TcpReceiver()
        {
            CleanUpSocket();
        }
    }
}