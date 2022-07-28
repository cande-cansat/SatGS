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
using SatGS.Communication;

using Vec3 = System.Tuple<float, float, float>;
using SatGS.SateliteData;
using SatGS.Interface;

namespace SatGS.ViewModel
{
    internal class SatStatusViewModel : NotifyPropertyChanged
    {
        private string information;
        public string Information 
        {
            get => information;
            set
            {
                var values = value.Split(' ');
                if (values.Length != 4) return;
                information = $"Altitude: {values[0]}\nRoll: {values[1]}\nPitch: {values[2]}\nYaw: {values[3]}";
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

        public SatStatusViewModel()
        {
            Information = "0 0 0 0";
            SerialReceiver.Instance().PacketReceived += SerialPacketReceived;
        }

        private void SerialPacketReceived(object sender, SateliteStatus e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Rotation = new Vec3(e.Roll, e.Pitch, e.Yaw);
                Information = $"{e.Altitude} {e.Roll} {e.Pitch} {e.Yaw}";
            });
        }

        ~SatStatusViewModel()
        {
            //serialReceiver.CleanUpSerial();
        }
    }
}
