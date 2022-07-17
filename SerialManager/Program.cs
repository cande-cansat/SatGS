using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Management;

namespace SerialManager
{
    struct PacketData
    {
        public int Length { get; set; }
        public byte[] Data { get; set; }
    }

    class SerialReceiver
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
            /*
            var serialInfos = GetSerialPortInfos();

            string port = string.Empty;
            foreach (var info in serialInfos)
            {
                if (info.Value.Contains("Arduino"))
                {
                    port = info.Key;
                }
            }
            */

            string port = "COM12";

            try
            {
                serial = new SerialPort(port);
                serial.Open();
                IsOpen = true;
                serial.DataReceived += DataReceived;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
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
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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

    internal class Program
    {
        struct SerialPortInfo
        {
            public string Port { get; set; }
            public string Desc { get; set; }

            public SerialPortInfo(string port, string desc)
            {
                Port = port;
                Desc = desc;
            }
        };

        static Dictionary<string, string> GetSerialPortInfos()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM WIN32_SerialPort"))
            {
                var dict = new Dictionary<string, string>();
                var portNames = SerialPort.GetPortNames();
                var ports = searcher.Get().Cast<ManagementBaseObject>().ToList();
                var tList = (from n in portNames
                             join p in ports on n equals p["DeviceID"].ToString()
                             select new SerialPortInfo(n, p["Caption"].ToString())).ToList();

                tList.ForEach(x => dict.Add(x.Port, x.Desc));

                return dict;
            }
        }

        static SerialPort OpenSerialPort(string port)
        {
            if (!SerialPort.GetPortNames().Contains(port)) return null;

            SerialPort serial = null;

            try
            {
                serial = new SerialPort(port);
                serial.Open();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Environment.Exit(-1);
            }
            return serial;
        }

        static void Main(string[] args)
        {
            var serial = SerialReceiver.Instance();
            serial.PacketReceived += Serial_PacketReceived;

            Console.ReadLine();
        }

        private static void Serial_PacketReceived(object sender, PacketData e)
        {
            Console.WriteLine(Encoding.UTF8.GetString(e.Data));
        }
    }
}
