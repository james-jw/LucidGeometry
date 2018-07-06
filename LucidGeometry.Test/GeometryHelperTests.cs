using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LucidGeometry.Tests
{
    [TestClass()]
    public class GeometryHelperTests
    {
        [TestMethod()]
        public void CoincidentTest()
        {
            var first = new LucidPoint() { X = 2.4, Y = 3 };
            var second = new LucidVertex() { X = 2.4, Y = 3 };

            Assert.IsTrue(GeometryHelper.Intersects(first, (IPoint)second));

            foreach (var change in new Action[] {
                () => second.Y = 4,
                () => {
                    first.Y = 4;
                    first.X = 2;
                },
                () => second.Y = 2.232
            })
            {
                change();
                Assert.IsFalse(GeometryHelper.Intersects(first, (IPoint)second));
            }
        }

        [TestMethod()]
        public void CircularPathFromPointTest()
        {
            var circleOut = GeometryHelper.CircularPathFromPoint(new LucidPoint() { X = 0, Y = 0 }, 5, 5);

            // First and last are coincident and thus represent a single point
            Assert.AreEqual(6, circleOut.Vertices.Count());
        }

        [TestMethod()]
        public void CircularPathToPolygonTest()
        {
            var circleOut = GeometryHelper.CircularPathFromPoint(new LucidPoint() { X = 0, Y = 0 }, 5, 100);
            var polygon = LucidPolygon.Create(circleOut.Vertices);

            Assert.AreEqual(1, polygon.Rings.Count());
        }

        [TestMethod()]
        public void DistanceBetweenTest()
        {
            var pointOne = new LucidPoint(0, 0);
            var pointTwo = new LucidPoint(5, 0);

            Assert.AreEqual(5, GeometryHelper.DistanceBetween(pointOne, pointTwo));
        }

        [TestMethod()]
        public void IntersectsTest()
        {
            var first = LucidExtent.Create(new LucidVertex(1, 1), 2);
            var second = LucidExtent.Create(new LucidVertex(0, 0), 3);

            Assert.IsTrue(GeometryHelper.Intersects(first, second));
            Assert.IsTrue(GeometryHelper.Intersects(second, first));

            second.Translate(-5, 2);

            Assert.IsFalse(GeometryHelper.Intersects(first, second));
            Assert.IsFalse(GeometryHelper.Intersects(second, first));

        }

        [TestMethod()]
        public void IntersectionLineTest()
        {
            var first = LucidLine.Create(new IPoint[] {
                new LucidPoint(0, 0), new LucidPoint(5, 5)
            });

            var second = LucidLine.Create(new IPoint[] {
                new LucidPoint(-1, 4), new LucidPoint(5, 0)
            });

            var intersection = GeometryHelper.LineIntersections(first, second).First();

            Assert.AreEqual(2, intersection.X);
            Assert.AreEqual(2, intersection.Y);

            second.Translate(-5, -5);
            var intersections = GeometryHelper.LineIntersections(first, second);

            Assert.AreEqual(0, intersections.Count());
        }

        [TestMethod()]
        public void SplitCircularLineTest()
        {
            var point = "-9167647.29 3463555.71";
        }

        [TestMethod()]
        public void SplitTest()
        {
            var first = LucidLine.Create(new IPoint[] {
                new LucidPoint(0, 0), new LucidPoint(5, 5), new LucidPoint(10, 9)
            });

            var second = LucidLine.Create(new IPoint[] {
                new LucidPoint(-1, 4), new LucidPoint(5, 0)
            });

            var parts = GeometryHelper.Split(first, second);

            Assert.AreEqual(2, parts.Count());
            Assert.AreEqual(2, parts.First().Vertices.Count());
            Assert.AreEqual(3, parts.ElementAt(1).Vertices.Count());

            second.Translate(-10, -10);
            parts = GeometryHelper.Split(first, second);

            Assert.AreEqual(1, parts.Count());
            Assert.AreEqual(3, parts.First().Vertices.Count());

        }

        [TestMethod()]
        public void FurthestVertexTest()
        {
            var line = new LucidLine();
            line.AddVertex(new LucidVertex()
            {
                X = 1,
                Y = 1
            });

            line.AddVertex(new LucidVertex()
            {
                X = 10,
                Y = 10
            });

            var result = GeometryHelper.FurthestVertex(line.Vertices, new LucidPoint() { X = 0, Y = 0 });

            Assert.AreEqual(10, result.Point.X);
            Assert.AreEqual(10, result.Point.Y);
        }

        [TestMethod()]
        public void NearestVertexTest()
        {
            var line = new LucidLine();
            line.AddVertex(new LucidVertex()
            {
                X = 1,
                Y = 1
            });

            line.AddVertex(new LucidVertex()
            {
                X = 10,
                Y = 10
            });

            var result = GeometryHelper.NearestVertex(line.Vertices, new LucidPoint() { X = 0, Y = 0 });

            Assert.AreEqual(1, result.Point.X);
            Assert.AreEqual(1, result.Point.Y);

        }

        [TestMethod()]
        public void IsValidTest()
        {
            foreach (var iteration in new[] { 1, 2, 3 })
            {
                var geometry = GeometryHelper.Create(File.ReadAllText($"../../Data/duplicateVerticesGeometry{iteration}.json"));
                Assert.IsFalse(GeometryHelper.IsValid(geometry));
            }
        }

        [TestMethod()]
        public void FixDuplicateVerticesTest()
        {
            foreach (var iteration in new[] { 1, 2, 3 })
            {
                var geometry = GeometryHelper.Create(File.ReadAllText($"../../Data/duplicateVerticesGeometry{iteration}.json"));
                var fixedGeometry = GeometryHelper.FixDuplicateVertices((ILucidLine)geometry);

                Assert.IsTrue(GeometryHelper.IsValid(fixedGeometry));
                Assert.IsTrue(geometry.Vertices.First() == fixedGeometry.Vertices.First());
                Assert.IsTrue(geometry.Vertices.Last() == fixedGeometry.Vertices.Last());
            }


        }

        //var portal = new Uri("https://win-o7h4l8voqt9.arcfmsolution.com/portal106");
        //var token = ContextualFeature.Client.Portal.GenerateTokenAsync(portal, req => {
        //    req.Expiration(60);
        //    req.Client(ClientIdType.Referer);
        //    req.Referer(new Uri("https://win-o7h4l8voqt9.arcfmsolution.com/arcgis106"));
        //    req.Username("siteadmin");
        //    req.Password("G0miner!!22222");
        //}).Result;
        //var service = new Uri("https://win-o7h4l8voqt9.arcfmsolution.com/arcgis106/rest/services/SchneidervilleUNGas/FeatureServer");
        //var first = new ContextualFeature(new Guid("{FA064790-0E98-493A-9EAF-4259EDB5FC4F}"), 215, service, token.Token);
        //var geometry = (ILucidLine)first.Geometry;
        //first.Geometry = fixedGeometry;

        //var set = new ContextualFeatureSet(first);
        //var editResults = set.ApplyEdits().Result;

        [TestMethod()]
        public void RemoveSelfIntersectionsTest()
        {
            var geometry = (ILucidLine)GeometryHelper.Create(File.ReadAllText("../../Data/selfIntersectingGeometry.json"));
            var fixedGeometry = LucidLine.Create(GeometryHelper.RemoveSelfIntersections(geometry));
            Assert.IsTrue(GeometryHelper.IsValid(fixedGeometry));
            Assert.IsFalse(GeometryHelper.SelfIntersects(fixedGeometry));

            geometry = (ILucidLine)GeometryHelper.Create(File.ReadAllText("../../Data/selfIntersectingGeometry2.json"));
            fixedGeometry = LucidLine.Create(GeometryHelper.RemoveSelfIntersections(geometry));
            Assert.IsTrue(GeometryHelper.IsValid(fixedGeometry));
            Assert.IsFalse(GeometryHelper.SelfIntersects(fixedGeometry));
        }

        [TestMethod()]
        public void SplitLengthTest()
        {
            var line = (ILucidLine)GeometryHelper.Create(File.ReadAllText("../../Data/lineToSplit.json"));
            var referencePoint = new LucidPoint(-9165777.5026, 3457646.8105999976);

            var cutter = GeometryHelper.CircularPathFromPoint(referencePoint, .5, 20);
            var parts = GeometryHelper.Split(line, cutter).ToArray();

            Assert.AreEqual(GeometryHelper.Length(line),
                GeometryHelper.Length(parts[0]) + GeometryHelper.Length(parts[1]));
        }


        [TestMethod()]
        public void LengthTest()
        {
            var length = GeometryHelper.Length(LucidLine.Create(new IPoint[] {
                LucidPoint.Create(0, 0),
                LucidPoint.Create(1, 0),
                LucidPoint.Create(1, 2),
            }));

            Assert.AreEqual(3, length);

            length = GeometryHelper.Length(LucidLine.Create(new IPoint[] {
                LucidPoint.Create(0, 0),
                LucidPoint.Create(0, 5)
            }));

            Assert.AreEqual(5, length);
        }

        [TestMethod()]
        public void AbsAngleTest()
        {
            Assert.AreEqual(359, GeometryHelper.AbsAngle(-1));
            Assert.AreEqual(359, GeometryHelper.AbsAngle(-361));
        }

        [TestMethod()]
        public void AverageAngleTest()
        {
            var averageAngle = GeometryHelper.AverageAngle(5d, 15d);
            Assert.AreEqual(10, Math.Round(averageAngle));
        }

        [TestMethod()]
        public void ToNorthBearingAngleTest()
        {
            Assert.AreEqual(0, GeometryHelper.ToNorthBearing(90));
            Assert.AreEqual(90, GeometryHelper.ToNorthBearing(0));
            Assert.AreEqual(180, GeometryHelper.ToNorthBearing(270));
            Assert.AreEqual(270, GeometryHelper.ToNorthBearing(180));
        }

        [TestMethod()]
        public void GetAngleInRadiansTest()
        {
            var refPoint = new LucidPoint(0, 0);

            Assert.AreEqual(0, GeometryHelper.Angle(refPoint, new LucidPoint(1, 0)));
            Assert.AreEqual(Math.PI, GeometryHelper.Angle(refPoint, new LucidPoint(-1, 0)));
        }

        [TestMethod()]
        public void AngleToDegreesTest()
        {
            Assert.AreEqual(180, GeometryHelper.AngleToDegrees(Math.PI));
            Assert.AreEqual(90, GeometryHelper.AngleToDegrees(Math.PI / 2));
            Assert.AreEqual(270, GeometryHelper.AngleToDegrees((Math.PI * 3) / 2));
        }

        [TestMethod()]
        public void AngleToRadiansTest()
        {
            Assert.AreEqual(Math.PI, GeometryHelper.AngleToRadians(180));
            Assert.AreEqual(Math.PI / 2, GeometryHelper.AngleToRadians(90));
            Assert.AreEqual((Math.PI * 3) / 2, GeometryHelper.AngleToRadians(270));
        }

        [TestMethod()]
        public void PerpendicularAngleTest()
        {
            Assert.AreEqual(90, GeometryHelper.PerpendicularAngle(0));
            Assert.AreEqual(60, GeometryHelper.PerpendicularAngle(330));
            Assert.AreEqual(270, GeometryHelper.PerpendicularAngle(180));
            Assert.AreEqual(0, GeometryHelper.PerpendicularAngle(270));
        }
    }
}