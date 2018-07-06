using System.Collections.Generic;

namespace LucidGeometry
{
    public interface ILucidExtent : ILucidGeometry
    {
        double XMax { get; set; }
        double XMin { get; set; }
        double YMax { get; set; }
        double YMin { get; set; }
        double Width { get; }
        double Height { get; }

        ILucidPolygon AsPolygon();
        ILucidExtent Buffer(double bufferSize);
    }
}