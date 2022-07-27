using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
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
        public event EventHandler<Model.SatliteStatus2> PacketReceived;

        private SerialPort serial;
        private Thread packetProcessThread;
        private ConcurrentQueue<byte> receivingBuffer;

        private BinaryWriter logFileStream;
        

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

        private bool FindArduino(out string port)
        {
            var serialInfos = GetSerialPortInfos();

            var filtered = from info in serialInfos
                           from filter in serialFilters
                           where info.Value.Contains(filter)
                           select info.Key;

            if(filtered.Count() <= 0)
            {
                port = null;
                return false;
            }

            port = filtered.First();
            return true;
        }

        private void OpenSerial()
        {
            if (IsOpen) return;

            
            
            if(!FindArduino(out var serialPort))
            {
                MessageBox.Show("연결된 아두이노를 찾을 수 없습니다.");
                return;
            }
            
            
            // For Debugging
            //var serialPort = "COM12";

            try
            {
                serial = new SerialPort(serialPort);
                serial.BaudRate = 115200;
                serial.Open();

                IsOpen = true;

                logFileStream = new BinaryWriter(new FileStream("Serial.log", FileMode.OpenOrCreate), Encoding.UTF8);

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
                var size = serial.BytesToRead;
                var buffer = new byte[size];

                serial.Read(buffer, 0, size);
                buffer.ToList().ForEach(receivingBuffer.Enqueue);
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
                if (receivingBuffer.Count < 22) continue;

                var acc = new Queue<byte>();
                
                while(true)
                {
                    if (acc.Count >= 2 &&
                        acc.ElementAt(acc.Count - 2) == 0x68 &&
                        acc.ElementAt(acc.Count - 1) == 0x69)
                        break;

                    receivingBuffer.TryDequeue(out var b);
                    acc.Enqueue(b);
                }

                if (acc.Count < 22) continue;

                while (acc.Count > 22)
                    acc.Dequeue();

                var payload = acc.ToArray();
                var status = Factory.SatliteStatusFactory.Create2(payload);
                PacketReceived?.Invoke(this, status);

                logFileStream.Write(payload, 0, payload.Length);

                /* For Debugging
                var status = Factory.SatliteStatusFactory.Create2(payload);
                var hexData = payload.Aggregate("", (str, b) =>
                {
                    return str + Convert.ToString(b, 16).PadLeft(2, '0') + ' ';
                });
                */
            }
        }

        private SerialReceiver()
        {
            IsOpen = false;
            OpenSerial();
        }

        public void CleanUpSerial()
        {
            if (!IsOpen) return;

            IsOpen = false;

            logFileStream.Flush();
            logFileStream.Close();

            if (packetProcessThread != null && packetProcessThread.IsAlive)
                packetProcessThread.Abort();
            if (serial.IsOpen)
                serial.Close();
        }

        ~SerialReceiver()
        {
            CleanUpSerial();
        }
    }
}
