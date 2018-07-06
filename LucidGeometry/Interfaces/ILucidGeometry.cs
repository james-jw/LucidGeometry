using Newtonsoft.Json;
using System.Collections.Generic;

namespace LucidGeometry
{
    public interface ILucidGeometry
    {
        [JsonIgnore]
        IEnumerable<ILucidVertex> Vertices { get; }

        void Translate(double x, double y);
    }
}