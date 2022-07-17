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
        // roll, pitch, yaw 값을 quaternion값으로 변경 -> 위성의 현재 기울기 전시 가능
        private Media3D.Quaternion ToQuaternion(Model.Vec3 v)
        {
            float cy = (float)Math.Cos(v.Z * 0.5);
            float sy = (float)Math.Sin(v.Z * 0.5);
            float cp = (float)Math.Cos(v.Y * 0.5);
            float sp = (float)Math.Sin(v.Y * 0.5);
            float cr = (float)Math.Cos(v.X * 0.5);
            float sr = (float)Math.Sin(v.X * 0.5);

            return new Media3D.Quaternion
            {
                W = (cr * cp * cy + sr * sp * sy),
                X = (sr * cp * cy - cr * sp * sy),
                Y = (cr * sp * cy + sr * cp * sy),
                Z = (cr * cp * sy - sr * sp * cy)
            };
        }

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

        private void SerialPacketReceived(object sender, PacketData e)
        {
            var status = Factory.SatliteStatusFactory.Create2(e);

            Application.Current.Dispatcher.Invoke(() =>
            {
                Rotation = new Vec3(status.Roll, status.Pitch, status.Yaw);
                Information = $"{status.Roll} {status.Pitch} {status.Yaw}";
            });
        }

        private void TcpPacketReceived(object sender, PacketData e)
        {
            if (e.Data[0] != 0) return;

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
    }
}
