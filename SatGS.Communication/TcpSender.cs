using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SatGS.Communication
{
    public class TcpSender
    {
        struct AsyncState
        {
            public TcpClient connector;
            public int bytesToSend;
        }

        private static TcpSender instance;
        public static TcpSender Instance()
        {
            if(instance == null)
            {
                instance = new TcpSender();
            }
            return instance;
        }

        TcpClient connector;

        private TcpSender()
        {
            connector = new TcpClient();
            connector.BeginConnect(System.Net.IPAddress.Any, 50001, ConnectCallback, connector);
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            var connector = ar.AsyncState as TcpClient;
            try
            {
                connector.EndConnect(ar);
            }
            catch
            {
                connector.BeginConnect(System.Net.IPAddress.Any, 50001, ConnectCallback, connector);
            }
        }

        public void PathCalculated(object sender, byte[] data)
        {
            if (!connector.Connected) return;

            connector.Client.BeginSend(data, 0, data.Length, SocketFlags.None, SendCallback, new AsyncState
            {
                connector = connector,
                bytesToSend = data.Length
            }).AsyncWaitHandle.WaitOne();
        }

        private void SendCallback(IAsyncResult ar)
        {
            var state = (AsyncState)ar.AsyncState;
            var sent = state.connector.Client.EndSend(ar, out var error);

            if(error != SocketError.Success)
            {
                MessageBox.Show("원격지와의 연결이 종료되었습니다.", "연결 강제 종료");
            }

            if(sent != state.bytesToSend)
            {
                MessageBox.Show("좌표 정보가 제대로 송신되지 않았습니다", "원격지 송신 오류");
            }
        }
    }
}
