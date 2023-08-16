using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sharpUTM
{
    /// <summary>
    /// Advanced trigonometric functions not included in System.Math
    /// </summary>
    public static class Trig
    {
        public static double DegreesToRadians (double degrees)
        {
            return degrees * Math.PI / 180;
        }

        public static double RadiansToDegrees (double radians)
        {
            return radians * 180 / Math.PI;
        }

        public static double AtanH (double angle)
        {
            double num = 1 + angle;
            double denom = 1 - angle;

            return Math.Abs(angle) switch
            {
                < 1 => Math.Log(num / denom) / 2,
                _ => (Math.Log(num) - Math.Log(denom)) / 2,
            };
        }


    }
}
