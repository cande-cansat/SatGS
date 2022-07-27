using System;
using System.Collections.Generic;
using System.Text;

namespace SatGS.ObjectDetection
{
    class Coordinate
    {
        public float item1;
        public float item2;
        public float item3;

        public Coordinate() { }
        public Coordinate(float item1, float item2, float item3)
        {
            this.item1 = item1;
            this.item2 = item2;
            this.item3 = item3;
        }

        public static Coordinate vectorSum(Coordinate T1, Coordinate T2)
        {
            return new Coordinate(
                T1.item1 + T2.item1,
                T1.item2 + T2.item2,
                T1.item3 + T2.item3
            );
        }

        public static Coordinate realNumMul(Coordinate T1, float n)
        {
            return new Coordinate(T1.item1 * n, T1.item2 * n, T1.item3 * n);
        }

        public static Coordinate sphericalToCartesian(Coordinate sphericalCoord)
        {
            float rho = sphericalCoord.item1;
            float phi = sphericalCoord.item2;
            float theta = sphericalCoord.item3;
            return new Coordinate(
                    (float)(rho * Math.Sin(phi) * Math.Cos(theta)),
                    (float)(rho * Math.Sin(phi) * Math.Sin(theta)),
                    (float)(rho * Math.Cos(phi))
               );
        }

    }

    class PositionData
    {
        private Coordinate satellitePos;
        private Coordinate relativeTargetSphericalPos;
        private Coordinate absoluteTargetCartesianPos;

        public PositionData() { }
        public PositionData(Coordinate satellitePos, Coordinate relativeTargetSphericalPos)
        {
            this.satellitePos = satellitePos;
            this.relativeTargetSphericalPos = relativeTargetSphericalPos;
        }
        public void setSatellitePos(Coordinate satellitePos)
        {
            this.satellitePos = satellitePos;
        }
        public void setRelativeTargetSphericalPos(Coordinate relativeTargetSphericalPos)
        {
            this.relativeTargetSphericalPos = relativeTargetSphericalPos;
        }

        public Coordinate getAbsoluteTargetCartesianPos()
        {
            return this.absoluteTargetCartesianPos;
        }

        public void calcAbsolutePos()
        {
            Coordinate cartesian = Coordinate.sphericalToCartesian(this.relativeTargetSphericalPos);
            this.absoluteTargetCartesianPos = Coordinate.vectorSum(cartesian, satellitePos);
        }
    }
}
