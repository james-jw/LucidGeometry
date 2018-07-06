using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace LucidGeometry
{
    /// <summary>
    /// Represents a Vector in 2D or 3D space.
    /// </summary>
    public class LucidVector 
    {
        /// <summary>
        /// Produces a vector from a provided angle and magnitude
        /// </summary>
        /// <param name="magnitude">The magnitude of the new vector</param>
        /// <param name="angleInRadians">The angle about the origin (0,0) of the new vector</param>
        public LucidVector(double magnitude, double angleInRadians)
        {
            X = magnitude * Math.Cos(angleInRadians);
            Y = magnitude * Math.Sin(angleInRadians);
        }

        /// <summary>
        /// Produces a vector with the provided coordinates
        /// </summary>
        /// <param name="x">The X component of the vector</param>
        /// <param name="y">The Y component of the vector</param>
        /// <param name="z">The Z component of the vector</param>
        public LucidVector(double x, double y, double z = 0)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; } = 0;

        /// <summary>
        /// Returns the magnitude of the vector
        /// </summary>
        [JsonIgnore]
        public double Magnitude {
            get { return Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2) + Math.Pow(Z, 2)); }
        }

        /// <summary>
        /// Returns the angle of the vector in radians.
        /// </summary>
        [JsonIgnore]
        public double Angle {
            get { return Math.Atan(X / Y); }
        }

        /// <summary>
        /// Returns a normalized unit vector of this vector.
        /// </summary>
        [JsonIgnore]
        public LucidVector Normalized {
            get {
                var magnitude = Magnitude;
                return new LucidVector(X / magnitude, Y / magnitude, Z / magnitude);
            }
        }

        public static LucidVector operator +(LucidVector a, LucidVector b)
        {
            return new LucidVector(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static LucidVector operator -(LucidVector a, LucidVector b)
        {
            return new LucidVector(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static double operator *(LucidVector a, LucidVector b)
        {
            return (a.X * b.X) + (a.Y * b.Y) + (a.Z * b.Z);
        }

        public static LucidVector operator *(LucidVector a, double scalar)
        {
            return new LucidVector(a.X * scalar, a.Y * scalar, a.Z * scalar);
        }

        public static LucidVector operator /(LucidVector a, double scalar)
        {
            return new LucidVector(a.X / scalar, a.Y / scalar, a.Z / scalar);
        }
    }
}
