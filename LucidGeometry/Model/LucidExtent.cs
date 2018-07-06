using System.Collections.Generic;
using Newtonsoft.Json;

namespace LucidGeometry
{
    public class LucidExtent : LucidGeometry, ILucidGeometry, ILucidExtent
    {
        [JsonProperty("xmin")]
        public double XMin { get; set; }

        [JsonProperty("xmax")]
        public double XMax { get; set; }

        [JsonProperty("ymin")]
        public double YMin { get; set; }

        [JsonProperty("ymax")]
        public double YMax { get; set; }

        public IEnumerable<ILucidVertex> Vertices {
            get {
                yield return LucidVertex.Create(XMin, YMax);
                yield return LucidVertex.Create(XMax, YMax);
                yield return LucidVertex.Create(XMax, YMin);
                yield return LucidVertex.Create(XMin, YMin);
            }
        }

        public double Width => XMax - XMin;

        public double Height => YMax - YMin;

        public ILucidPolygon AsPolygon()
        {
            return LucidPolygon.Create(Vertices, SpatialReference);
        }

        public static ILucidGeometry Create(string geometryJson)
        {
            return JsonConvert.DeserializeObject<LucidExtent>(geometryJson);
        }

        public static ILucidExtent Create(IPoint vertex, double radius)
        {
            return new LucidExtent() {
                XMax = vertex.X + radius,
                XMin = vertex.X - radius,
                YMax = vertex.Y + radius,
                YMin = vertex.Y - radius
            };
        }

        public void Translate(double x, double y)
        {
            XMax += x;
            XMin += x;
            YMax += y;
            YMin += y;
        }

        public ILucidExtent Buffer(double bufferSize)
        {
            return GeometryHelper.Buffer(this, bufferSize);
        }
    }
}
