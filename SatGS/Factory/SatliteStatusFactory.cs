using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatGS.Factory
{
    internal static class SatliteStatusFactory
    {
        public static Model.SatliteStatus Create(byte[] packet)
        {
            int offset = 0;
            const int step = 4;

            Model.Vec3 position = new Model.Vec3();

            // latitude
            position.X = BitConverter.ToSingle(packet, offset); offset += step;
            // longitude
            position.Y = BitConverter.ToSingle(packet, offset); offset += step;
            // altitude
            position.Z = BitConverter.ToSingle(packet, offset); offset += step;

            float heading;
            Model.Vec3 rotation = new Model.Vec3();
            
            heading = BitConverter.ToSingle(packet, offset); offset += step;
            // pitch
            rotation.Y = BitConverter.ToSingle(packet, offset); offset += step;
            // roll
            rotation.X = BitConverter.ToSingle(packet, offset); offset += step;
            // yaw
            rotation.Z = BitConverter.ToSingle(packet, offset); offset += step;

            Model.Vec3 acceleration = new Model.Vec3();

            acceleration.X = BitConverter.ToSingle(packet, offset); offset += step;
            acceleration.Y = BitConverter.ToSingle(packet, offset); offset += step;
            acceleration.Z = BitConverter.ToSingle(packet, offset); offset += step;

            float temperature, humidity;
            temperature = BitConverter.ToSingle(packet, offset); offset += step;
            humidity = BitConverter.ToSingle(packet, offset); offset += step;

            return new Model.SatliteStatus
            {
                Position = position,
                Heading = heading,
                Rotation = rotation,
                Acceleration = acceleration,
                Temperature = temperature,
                Humidity = humidity
            };
        }
    }
}
