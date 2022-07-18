using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SatGS.Socket
{
    internal class SerialReceiver
    {
        #region Singleton
        private static SerialReceiver instance;

        public static SerialReceiver Instance()
        {
            if (instance == null)
                instance = new SerialReceiver();
            return instance;
        }
        #endregion

        public bool IsOpen { get; set; }
        SerialPort serial;
        Thread packetMaker;
        ConcurrentQueue<byte> ReceivingBuffer;
        public event EventHandler<byte[]> PacketReceived;

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
            string[] portFilter = new string[]
            {
                "Arduino",
                "아두이노",
                "USB",
                "usb"
            };

            foreach(var info in serialInfos)
            {
                foreach(var filter in portFilter)
                {
                    if(info.Value.Contains(filter))
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
                serial.BaudRate = 115200;
                serial.Open();

                IsOpen = true;

                ReceivingBuffer = new ConcurrentQueue<byte>();
                packetMaker = new Thread(MakePacket);
                packetMaker.Start();

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
            try
            {
                var recv = serial.ReadExisting().ToList();
                recv.ForEach(c => ReceivingBuffer.Enqueue(Convert.ToByte(c)));
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                IsOpen = false;
            }
        }

        void MakePacket()
        {
            while (IsOpen)
            {
                while (ReceivingBuffer.Count >= 20)
                {
                    var payload = Enumerable.Range(0, 20).Select(i =>
                    {
                        byte b;
                        while (!ReceivingBuffer.TryDequeue(out b)) ;
                        return b;
                    }).ToArray();

                    PacketReceived?.Invoke(this, payload);

                    var status = Factory.SatliteStatusFactory.Create2(payload);

                    DebugConsole.WriteLine($"Serial Received:\n\tRoll: {status.Roll}\n\tPitch: {status.Pitch}\n\tYaw: {status.Yaw}");
                }
            }
        }

        private SerialReceiver()
        {
            OpenSerialPort();
        }

        ~SerialReceiver()
        {
            IsOpen = false;
            packetMaker.Join();
            if (serial.IsOpen) serial.Close();
        }
    }
}
