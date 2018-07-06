using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using LucidJson;
using LucidGeometry.Converters;

namespace LucidGeometry
{
    public class LucidLine : LucidGeometry, ILucidLine
    {

        [JsonProperty("paths")]

        public List<List<ILucidVertex>> Paths { get; set; }


        public static LucidLine Create(Map geometry)
        {
            return LucidLine.Create(geometry.ToString());
        }

        internal static JsonSerializerSettings _jsonSettings = new JsonSerializerSettings() {
            Converters = new List<JsonConverter>() {
                new GeometryJsonConverter()
            }
        };

        public static LucidLine Create(string geometryJson)
        {
            return JsonConvert.DeserializeObject<LucidLine>(geometryJson, _jsonSettings);
        }

        public static LucidLine Create(IEnumerable<IPoint> path)
        {
            if (path.Count() < 2)
                throw new Exception($"Invalid line. {path} has too few vertices.");

            return new LucidLine() {
                Paths = new List<List<ILucidVertex>>() {
                    new List<ILucidVertex>(path.Select(p => p.AsVertex()))
                }
            };
        }

        [JsonIgnore]
        public double Angle {
            get {
                var start = Vertices.First();
                var end = Vertices.ElementAt(1);
                return Math.Atan2(end.Y - start.Y, end.X - start.X) * 180 / Math.PI;
            }
        }

        public void AddVertex(IPoint vertex)
        {
            AddVertex(vertex is ILucidVertex v ? v : new LucidVertex(vertex.X, vertex.Y));
        }

        /// <summary>
        /// Adds a vertex to the first ring or path
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void AddVertex(ILucidVertex vertex)
        {
            var paths = Paths;
            if (paths == null) {
                paths = new List<List<ILucidVertex>>();
                paths.Add(new List<ILucidVertex>());

                Paths = paths;
            }

            paths[0].Add(vertex);
        }

        [JsonIgnore]
        public IEnumerable<ILucidVertex> Vertices {
            get {
                foreach (var path in Paths) {
                    foreach (var vertex in path)
                        yield return vertex;
                }
            }
        }

        public void Translate(double x, double y)
        {
            foreach (var vertex in Vertices) {
                vertex.Translate(x, y);
            }
        }

        public static LucidLine operator+ (LucidLine first, IPoint point)
        {
            first.AddVertex(point);
            return first;
        }

        public static LucidLine operator+ (LucidLine first, LucidLine second)
        {
            return LucidLine.Create(first.Vertices.Concat(second.Vertices));
        }
    }
}
