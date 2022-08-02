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

                var receiveThread = new Thread(ReceiveCallback);
                receiveThreads.Add(receiveThread);
                receiveThread.Start(client);
            }

            listener.BeginAcceptTcpClient(AcceptCallback, null);
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