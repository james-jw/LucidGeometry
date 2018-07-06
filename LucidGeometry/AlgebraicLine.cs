using System.Collections.Generic;
using System.Linq;
using System;

namespace LucidGeometry
{
    public class AlgebraicLine : ILucidLine, ILucidGeometry
    {
        public IPoint Start { get; private set; }
        public IPoint End { get; private set; }

        public double DeltaY { get; private set; }
        public double DeltaX { get; private set; }
        public double C { get; private set; }

        public LucidSpatialReference SpatialReference => throw new System.NotImplementedException();

        public IEnumerable<ILucidVertex> Vertices {
            get {
                yield return Start.AsVertex();
                yield return End.AsVertex();
            }
        }

        public List<List<ILucidVertex>> Paths {
            get => new List<List<ILucidVertex>>() { new List<ILucidVertex>(Vertices) };
            set => throw new NotImplementedException($"{nameof(AlgebraicLine)} does not support setting the '{nameof(Paths)}' property");
        }

        private void Update()
        {
            DeltaY = End.Y - Start.Y;
            DeltaX = Start.X - End.X;
            C = (DeltaY * Start.X) + (DeltaX * Start.Y);
        }

        public AlgebraicLine(IPoint a, IPoint b, LucidSpatialReference spatialReference = null)
        {
            Start = a;
            End = b;

            Update();
        }

        public AlgebraicLine(IEnumerable<IPoint> vertices)
            : this(vertices.First(), vertices.Last())
        { }

        public void Translate(double x, double y)
        {
            Start.Translate(x, y);
            End.Translate(x, y);

            Update();
        }
    }
}
