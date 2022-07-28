using SatGS.PathFinder;
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

        public void PathCalculated(object sender, List<Coordinate> coordinates)
        {
            if (!connector.Connected) return;

            var size = coordinates.Count * 3 * 4;
            var buffer = new byte[size];
            var offset = 0;
            foreach(var coordinate in coordinates)
            {
                var bItem1 = BitConverter.GetBytes(coordinate.item1);
                var bItem2 = BitConverter.GetBytes(coordinate.item2);
                var bItem3 = BitConverter.GetBytes(coordinate.item3);

                bItem1.CopyTo(buffer, offset); offset += 4;
                bItem2.CopyTo(buffer, offset); offset += 4;
                bItem3.CopyTo(buffer, offset); offset += 4;
            }

            connector.Client.BeginSend(buffer, 0, size, SocketFlags.None, SendCallback, new AsyncState
            {
                connector = connector,
                bytesToSend = size
            });
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
