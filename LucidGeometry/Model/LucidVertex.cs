using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using LucidJson;

namespace LucidGeometry
{
    /// <summary>
    /// A vertex represents a point component of a geometry. Unlike a ILucidPoint a ISimpleVertex does not have a spatial reference
    /// and is not a valid geometry on its own
    /// </summary>
    public class LucidVertex : Array<double>, ILucidVertex, ILucidPoint
    {
        public LucidVertex() : base(2) { }

        public LucidVertex(double x, double y) : this()
        {
            this.X = x;
            this.Y = y;
        }

        public static LucidVertex Create(ILucidPoint point)
        {
            return new LucidVertex() { X = point.X, Y = point.Y };
        }

        public static ILucidVertex Create(double x, double y)
        {
            return new LucidVertex() {
                X = x,
                Y = y
            };
        }

        [JsonIgnore]
        public double X {
            get => this[0];
            set => this[0] = value;
        }

        [JsonIgnore]
        public double Y {
            get => this[1];
            set => this[1] = value;
        }

        public IEnumerable<ILucidVertex> Vertices => this.ItemAsEnumerable();

        public ILucidPoint AsGeometry()
        {
            return new LucidPoint() {
                X = this.X,
                Y = this.Y
            };
        }

        public ILucidVertex AsVertex() { return this; }

        public ILucidPoint AsGeometry(LucidSpatialReference spatialReference)
        {
            return new LucidPoint() {
                X = X,
                Y = Y,
                SpatialReference = spatialReference
            };
        }

        public string ToString(int precision = 2)
        {
            return GeometryHelper.ToString(this, precision) ;
        }

        public override string ToString()
        {
            return this.ToString(2);
        }

        public void Translate(double x, double y)
        {
            X += x;
            Y += y;
        }

        public bool IsCoincident(IPoint otherPoint)
        {
            return GeometryHelper.IsCoincident(this, otherPoint);
        }
    }
}
