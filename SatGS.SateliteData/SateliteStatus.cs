using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatGS.SateliteData
{
    public class SateliteStatus
    {
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public float Altitude { get; set; }
        public float Roll { get; set; }
        public float Pitch { get; set; }
        public float Yaw { get; set; }
        public float Temperature { get; set; }
        public float Humidity { get; set; }
    }

    public class SateliteStatusFactory
    {
        public static SateliteStatus Create(byte[] payload)
        {
            int offset = 0;

            var status = new SateliteStatus();

            status.Latitude = BitConverter.ToSingle(payload, offset);
            offset += sizeof(float);
            status.Longitude = BitConverter.ToSingle(payload, offset);
            offset += sizeof(float);
            status.Altitude = BitConverter.ToInt16(payload, offset);
            offset += sizeof(short);
            status.Roll = BitConverter.ToInt16(payload, offset) / 100.0f;
            offset += sizeof(short);
            status.Pitch = BitConverter.ToInt16(payload, offset) / 100.0f;
            offset += sizeof(short);
            status.Yaw = BitConverter.ToInt16(payload, offset) / 100.0f;
            offset += sizeof(short);
            status.Humidity = BitConverter.ToInt16(payload, offset);
            offset += sizeof(short);
            status.Temperature = BitConverter.ToInt16(payload, offset);
            offset += sizeof(short);

            return status;
        }
    }
}
