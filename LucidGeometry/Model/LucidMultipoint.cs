using System.Collections.Generic;
using Newtonsoft.Json;

namespace LucidGeometry
{
    public class LucidMultipoint : LucidGeometry, ILucidGeometry
    {
        [JsonProperty("points")]
        public List<ILucidVertex> Points { get; set; }

        public static LucidMultipoint Create(IEnumerable<ILucidVertex> points, LucidSpatialReference spatialReference = null)
        {
            return new LucidMultipoint() {
                Points = new List<ILucidVertex>(points),
                SpatialReference = spatialReference
            };
        }

        [JsonIgnore]
        public IEnumerable<ILucidVertex> Vertices {
            get {
                foreach (var point in Points) {
                    yield return point;
                }
            }
        }

        public void Translate(double x, double y)
        {
            foreach(var vertex in Vertices) {
                vertex.Translate(x, y);
            }
        }
    }
}
