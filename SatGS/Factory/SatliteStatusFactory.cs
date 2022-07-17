using SatGS.Socket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatGS.Factory
{
    internal static class SatliteStatusFactory
    {
        public static Model.SatliteStatus1 Create1(byte[] payload)
        {
            int offset = 1;
            const int step = 4;

            Model.Vec3 position = new Model.Vec3();

            // latitude
            position.X = BitConverter.ToSingle(payload, offset); offset += step;
            // longitude
            position.Y = BitConverter.ToSingle(payload, offset); offset += step;
            // altitude
            position.Z = BitConverter.ToSingle(payload, offset); offset += step;

            float heading;
            Model.Vec3 rotation = new Model.Vec3();
            
            heading = BitConverter.ToSingle(payload, offset); offset += step;
            // pitch
            rotation.Y = BitConverter.ToSingle(payload, offset); offset += step;
            // roll
            rotation.X = BitConverter.ToSingle(payload, offset); offset += step;
            // yaw
            rotation.Z = BitConverter.ToSingle(payload, offset); offset += step;

            Model.Vec3 acceleration = new Model.Vec3();

            acceleration.X = BitConverter.ToSingle(payload, offset); offset += step;
            acceleration.Y = BitConverter.ToSingle(payload, offset); offset += step;
            acceleration.Z = BitConverter.ToSingle(payload, offset); offset += step;

            float temperature, humidity;
            temperature = BitConverter.ToSingle(payload, offset); offset += step;
            humidity = BitConverter.ToSingle(payload, offset); offset += step;

            return new Model.SatliteStatus1
            {
                Position = position,
                Heading = heading,
                Rotation = rotation,
                Acceleration = acceleration,
                Temperature = temperature,
                Humidity = humidity
            };
        }

        public static Model.SatliteStatus2 Create2(byte[] payload)
        {
            int offset = 0;

            var status = new Model.SatliteStatus2();

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
            status.Temperature = BitConverter.ToInt16(payload, offset);
            offset += sizeof(short);
            status.Humidity = BitConverter.ToInt16(payload, offset);
            offset += sizeof(short);

            return status;
        }
    }
}
