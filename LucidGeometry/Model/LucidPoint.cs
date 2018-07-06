using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using LucidJson;

namespace LucidGeometry
{
    public class LucidPoint : LucidGeometry, ILucidGeometry, ILucidPoint
    {

        public LucidPoint() { }

        public LucidPoint(double x, double y, LucidSpatialReference spatialReference = null)
        {
            this.X = x;
            this.Y = y;
            this.SpatialReference = spatialReference;
        }

        [JsonProperty("x")]
        public double X { get; set; }

        [JsonProperty("y")]
        public double Y { get; set; }
        

        [JsonIgnore]
        public IEnumerable<ILucidVertex> Vertices {
            get => this.AsVertex().ItemAsEnumerable();
        }

        public static LucidPoint Create(Map geometry)
        {
            return LucidPoint.Create(geometry.ToString());
        }

        public static LucidPoint Create(string geometryJson)
        {
            return JsonConvert.DeserializeObject<LucidPoint>(geometryJson);
        }

        public static LucidPoint Create(double x, double y, LucidSpatialReference spatialReference = null)
        {
            return new LucidPoint() {
                X = x,
                Y = y,
                SpatialReference = spatialReference
            };
        }

        public string ToString(int precision = 2)
        {
            return GeometryHelper.ToString(this, precision) ;
        }

        public static LucidPoint Create(double[] parts, LucidSpatialReference spatialReference = null)
        {
            return LucidPoint.Create(parts[0], parts[1]);
        }

        public ILucidVertex AsVertex()
        {
            return LucidVertex.Create(this);
        }

        public ILucidPoint AsGeometry(LucidSpatialReference spatialReference = null)
        {
            if (spatialReference != null && SpatialReference != null &&
                spatialReference.WellknownId != this.SpatialReference.WellknownId) {
                throw new Exception($"Spatial references do not match. SpatialReference: {spatialReference}, Geometry : {this.AsMap()}");
            }

            return this;
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
