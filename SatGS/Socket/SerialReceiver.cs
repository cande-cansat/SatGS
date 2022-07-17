using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SatGS.Socket
{
    internal class SerialReceiver
    {
        private static SerialReceiver instance;

        public static SerialReceiver Instance()
        {
            if (instance == null)
                instance = new SerialReceiver();
            return instance;
        }

        SerialPort serial;
        const int BufferSize = 32;

        Dictionary<string, string> GetSerialPortInfos()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM WIN32_SerialPort"))
            {
                var dict = new Dictionary<string, string>();
                var portNames = SerialPort.GetPortNames();
                var ports = searcher.Get().Cast<ManagementBaseObject>().ToList();
                var tList = (from n in portNames
                             join p in ports on n equals p["DeviceID"].ToString()
                             select new KeyValuePair<string, string>(n, p["Caption"].ToString())).ToList();

                tList.ForEach(x => dict.Add(x.Key, x.Value));

                return dict;
            }
        }

        void OpenSerialPort()
        {
            IsOpen = false;
            var serialInfos = GetSerialPortInfos();

            string port = string.Empty;
            foreach(var info in serialInfos)
            {
                if (info.Value.Contains("Arduino") || info.Value.Contains("아두이노"))
                {
                    port = info.Key;
                }
            }

            if (string.IsNullOrEmpty(port))
            {
                MessageBox.Show("연결된 아두이노를 찾을 수 없습니다.");
                return;
            }

            try
            {
                serial = new SerialPort(port);
                serial.Open();
                IsOpen = true;
                serial.DataReceived += DataReceived;
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var serial = (SerialPort)sender;
            var buffer = new byte[BufferSize];
            try
            {
                var recvLen = serial.Read(buffer, 0, BufferSize);
                PacketReceived?.Invoke(this, new PacketData()
                {
                    Length = recvLen,
                    Data = buffer
                });
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                IsOpen = false;
            }
        }

        public bool IsOpen { get; set; }

        public event EventHandler<PacketData> PacketReceived;

        private SerialReceiver()
        {
            OpenSerialPort();
        }

        ~SerialReceiver()
        {
            if (serial.IsOpen) serial.Close();
        }
    }
}
