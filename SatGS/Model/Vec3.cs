using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatGS.Model
{
    internal struct Vec3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Vec3(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public Vec3(Vec3 vec3)
        {
            this.X = vec3.X;
            this.Y = vec3.Y;
            this.Z = vec3.Z;
        }

        public Vec3(float x, float y) {
            this.X = x;
            this.Y = y;
            this.Z = 0;
        }
        public Vec3(Vec2 vec2)
        {
            this.X = vec2.X;
            this.Y = vec2.Y;
            this.Z = 0;
        }
    }
}
