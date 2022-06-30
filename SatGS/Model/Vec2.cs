using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatGS.Model
{
    internal struct Vec2
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Vec2(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }
        public Vec2(Vec2 vec2)
        {
            this.X = vec2.X;
            this.Y = vec2.Y;
        }
    }
}
