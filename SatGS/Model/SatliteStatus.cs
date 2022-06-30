using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatGS.Model
{
    internal class SatliteStatus
    {
        #region member variables
        private Vec3 position;
        
        private float heading;
        private Vec3 rotation;

        private Vec3 acceleration;

        private float temperature;
        private float humidity;
        #endregion


        # region To access to member variables

        public Vec3 Position
        {
            get => position;
            set => position = value;
        }

        public float Heading
        {
            get => heading;
            set => heading = value;
        }
        public Vec3 Rotation
        {
            get => rotation;
            set => rotation = value;
        }

        public Vec3 Acceleration
        {
            get => acceleration;
            set => acceleration = value;
        }

        public float Temperature
        {
            get => temperature;
            set => temperature = value;
        }
        public float Humidity
        {
            get => humidity;
            set => humidity = value;
        }
        #endregion
    }
}
