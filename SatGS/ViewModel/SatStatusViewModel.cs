using SatGS.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Media3D = System.Windows.Media.Media3D;
using SatGS.Socket;
using SatGS.Model;

namespace SatGS.ViewModel
{
    internal class SatStatusViewModel : Model.NotifyPropertyChanged
    {
        private string information;
        public string Information 
        {
            get => information;
            set
            {
                var values = value.Split(' ');
                if (values.Length != 3) return;
                information = $"Roll: {values[0]}\nPitch: {values[1]}\nYaw: {values[2]}";
                OnPropertyChanged();
            }
        }

        private Vec3 rotation;
        public Vec3 Rotation
        {
            get => rotation;
            set
            {
                rotation = value;
                OnPropertyChanged();
            }
        }

        TcpReceiver tcpReceiver;
        SerialReceiver serialReceiver;

        public SatStatusViewModel()
        {
            Information = "0 0 0";
            tcpReceiver = TcpReceiver.Instance();
            tcpReceiver.PacketReceived += TcpPacketReceived;
            serialReceiver = SerialReceiver.Instance();
            serialReceiver.PacketReceived += SerialPacketReceived;
        }

        private void SerialPacketReceived(object sender, byte[] e)
        {
            var status = Factory.SatliteStatusFactory.Create2(e);

            Application.Current.Dispatcher.Invoke(() =>
            {
                Rotation = new Vec3(status.Roll, status.Pitch, status.Yaw);
                Information = $"{status.Roll} {status.Pitch} {status.Yaw}";
            });
        }

        private void TcpPacketReceived(object sender, byte[] e)
        {
            if (e[0] != 0) return;

            var status = Factory.SatliteStatusFactory.Create1(e);
            //var quat = ToQuaternion(status.Rotation);

            // 실제 회전
            // 현재 위치에서가 아닌, identity에서의 회전을 구현해야 함

            Application.Current.Dispatcher.Invoke(() =>
            {
                Rotation = status.Rotation;
                Information = $"{status.Rotation.X} {status.Rotation.Y} {status.Rotation.Z}";
            });
        }

        ~SatStatusViewModel()
        {
            //serialReceiver.CleanUpSerial();
        }
    }
}
