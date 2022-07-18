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
        public event EventHandler<byte[]> PacketReceived;

        private SerialPort serial;
        private Thread packetProcessThread;
        private ConcurrentQueue<byte> receivingBuffer;
        

        private Dictionary<string, string> GetSerialPortInfos()
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

        private readonly List<string> serialFilters = new List<string>
        {
            "Arduino",
            "아두이노",
            "USB",
            "usb"
        };

        private void OpenSerial()
        {
            if (IsOpen) return;

            var serialInfos = GetSerialPortInfos();

            var filtered = from info in serialInfos
                           from filter in serialFilters
                           where info.Value.Contains(filter)
                           select info.Key;

            if(filtered.Count() <= 0)
            {
                MessageBox.Show("연결된 아두이노를 찾을 수 없습니다.");
                return;
            }

            var serialPort = filtered.First();

            try
            {
                serial = new SerialPort(serialPort);
                serial.BaudRate = 115200;
                serial.Open();

                IsOpen = true;

                receivingBuffer = new ConcurrentQueue<byte>();
                packetProcessThread = new Thread(ProcessPacket);
                packetProcessThread.Start();

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
                Encoding.Default.GetBytes(serial.ReadExisting())
                    .ToList()
                    .ForEach(b => receivingBuffer.Enqueue(b));
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                CleanUpSerial();
                OpenSerial();
            }
        }

        private void ProcessPacket()
        {
            while (IsOpen)
            {
                while (receivingBuffer.Count < 20) continue;

                var payload = Enumerable.Range(0, 20).Select(i =>
                {
                    byte b;
                    while (!receivingBuffer.TryDequeue(out b)) ;
                    return b;
                }).ToArray();

                PacketReceived?.Invoke(this, payload);

                var status = Factory.SatliteStatusFactory.Create2(payload);

                DebugConsole.WriteLine($"Serial Received:\n\tRoll: {status.Roll}\n\tPitch: {status.Pitch}\n\tYaw: {status.Yaw}");
            }
        }

        private SerialReceiver()
        {
            IsOpen = false;
            OpenSerial();
        }

        private void CleanUpSerial()
        {
            if (!IsOpen) return;

            IsOpen = false;
            if (packetProcessThread != null && packetProcessThread.IsAlive)
                packetProcessThread.Join();
            if (serial.IsOpen)
                serial.Close();
        }

        ~SerialReceiver()
        {
            CleanUpSerial();
        }
    }
}
