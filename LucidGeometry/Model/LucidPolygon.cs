using LucidJson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LucidGeometry
{
    public class LucidPolygon : LucidGeometry, ILucidGeometry, ILucidPolygon
    {
        [JsonProperty("rings")]
        public List<List<ILucidVertex>> Rings { get; set; }

        /// <summary>
        /// Adds a vertex to the first ring or path
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void AddVertex(ILucidVertex vertex)
        {
            var rings = Rings;
            if(rings == null) {
                rings = new List<List<ILucidVertex>>();
                rings.Add(new List<ILucidVertex>());

                Rings = rings;
            }

            rings[0].Add(vertex);
        }

        [JsonIgnore]
        public IEnumerable<ILucidVertex> Vertices {
            get {
                foreach(var ring in Rings) {
                    foreach (var vertex in ring)
                        yield return vertex;
                }
            }
        }

        public ILucidExtent AsExtent()
        {
            return GeometryHelper.CalculateExtent(this);
        }

        public static LucidPolygon Create(Map geometry, LucidSpatialReference spatialReference = null)
        {
            return LucidPolygon.Create(geometry.ToString(), spatialReference);
        }

        public static LucidPolygon Create(string geometryJson, LucidSpatialReference spatialReference = null)
        {
            var geometryOut = JsonConvert.DeserializeObject<LucidPolygon>(geometryJson, LucidLine._jsonSettings);
            if (spatialReference != null)
                geometryOut.SpatialReference = spatialReference;

            return geometryOut;
        }

        public static LucidPolygon Create(IEnumerable<IPoint> initialRing, LucidSpatialReference spatialReference = null)
        {
            if (initialRing.Count() < 2)
                throw new Exception($"Invalid polygon ring. {initialRing} Too few vertices.");

            var ring = GeometryHelper.Intersects(initialRing.First(), initialRing.Last()) 
                ? initialRing : initialRing.Concat(initialRing.Take(1));

            return new LucidPolygon() {
                Rings = new List<List<ILucidVertex>>() {
                    new List<ILucidVertex>(ring.Select(p => p.AsVertex()))
                },
                SpatialReference = spatialReference
            };
        }

        public void Translate(double x, double y)
        {
            foreach(var vertex in Vertices) {
                vertex.Translate(x, y);
            }
        }

        public bool ExtentOverlaps(LucidPolygon other)
        {
            return GeometryHelper.Intersects(this.AsExtent(), other.AsExtent());
        }

        public static LucidPolygon operator+ (LucidPolygon polygon, IPoint point)
        {
            polygon.AddVertex(point is ILucidVertex vertex ? vertex : LucidVertex.Create(point.X, point.Y));
            return polygon;
        }

        public static LucidPolygon operator- (LucidPolygon first, LucidPolygon second)
        {
            throw new NotImplementedException();

            LucidPolygon intersection = null;
            if (!first.ExtentOverlaps(second))
                return null;

            foreach(var segment in first.Vertices.SlidingWindow(2))
            {
                foreach(var secondSegment in second.Vertices.SlidingWindow(2))
                {
                    var point = GeometryHelper.LineIntersections(LucidLine.Create(segment), LucidLine.Create(secondSegment)).FirstOrDefault();
                    if(point != null)
                    {
                        if(intersection == null) {
                            intersection = new LucidPolygon();
                        }

                        intersection += point;
                    }
                }
            }

            return intersection;
        }
    }

}
