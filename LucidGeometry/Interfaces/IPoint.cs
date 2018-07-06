using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LucidGeometry
{
    public interface IPoint
    {
        double X { get; set; }
        double Y { get; set; }

        ILucidVertex AsVertex();
        ILucidPoint AsGeometry(LucidSpatialReference spatialReference);

        bool IsCoincident(IPoint otherPoint);

        void Translate(double x, double y);

        string ToString(int precision);
        string ToString();
    }
}
