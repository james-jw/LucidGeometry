using LucidJson;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LucidGeometry
{
    /// <summary>
    /// Helper class for doing various geometric operations.
    /// </summary>
    public class GeometryHelper
    {
        public static ILucidGeometry Create(Map geometry)
        {
            return Create(geometry.ToString());
        }

        public static ILucidGeometry Create(string geometryJson)
        {
            if (geometryJson.Contains("\"x\"")) return LucidPoint.Create(geometryJson);
            if (geometryJson.Contains("\"xmax\"")) return LucidExtent.Create(geometryJson);
            if (geometryJson.Contains("\"paths\"")) return LucidLine.Create(geometryJson);
            if (geometryJson.Contains("\"rings\"")) return LucidPolygon.Create(geometryJson);

            throw new Exception($"Unrecognized geometry type {geometryJson}");
        }

        /// <summary>
        /// Converts an angle or negative or greater than 360 to its absolute 0 - 360 degree value.
        /// </summary>
        /// <param name="angle">The angle to find the absolute angle of</param>
        /// <returns>The absoluate angle. </returns>
        public static double AbsAngle(double angle)
        {
            if(angle < 0) {
                return AbsAngle((180 + angle) + 180);
            }

            return angle;
        }
        private static double Ceiling(double value, int precision)
        {
            double multiplier = Math.Pow(10, precision);
            value *= multiplier;
            value = Math.Ceiling(Convert.ToDouble(value));
            return value / multiplier;
        }


        public static string ToString(IPoint point, int precision = 2)
        {
            return $"{Ceiling(point.X, precision)} {Ceiling(point.Y, precision)}";
        }

        /// <summary>
        /// Finds the distance between two points.
        /// </summary>
        /// <param name="first">The first point.</param>
        /// <param name="second">The second point.</param>
        /// <returns>The distance between the two points</returns>
        public static double DistanceBetween(IPoint first, IPoint second)
        {
            var distance = Math.Sqrt(
                Math.Pow(first.X - second.X, 2) +
                Math.Pow(first.Y - second.Y, 2)
            );

            return Math.Abs(distance);
        }

        public static double Floor(double value, int decimalPlaces = 3)
        {
            double adjustment = Math.Pow(10, decimalPlaces);
            return Math.Floor(value * adjustment) / adjustment;
        }

        /// <summary>
        /// Determines if the two points are coincident provided the precision.
        /// </summary>
        /// <param name="first">The first point to test coincidents on.</param>
        /// <param name="second">The second point.</param>
        /// <param name="precision">The precision in number of decimal places to evalute with.</param>
        /// <returns>True if the features are coincident, false otherwise.</returns>
        public static bool IsCoincident(IPoint first , IPoint second, int precision = 2)
        {
            if (Ceiling(first.X, precision) != Ceiling(second.X, precision)) return false;
            if (Ceiling(first.Y, precision) != Ceiling(second.Y, precision)) return false;

            return true;
        }

        public static bool IsPrettyMuchTheSame(double value1, double value2, double epsilon)
        {
            return Math.Abs(value2 - value1) < epsilon;
        }

        /// <summary>
        /// Checks for duplicate and or invalid vertices
        /// </summary>
        /// <param name="geometry">The geometry to check for duplicate vertices on</param>
        /// <returns>True if their are no invalid vertices</returns>
        public static bool IsValid(ILucidGeometry geometry)
        {
            if(geometry is ILucidLine) {
                var vertexCount = geometry.Vertices.Count();
                var disticntVertexCount = geometry.Vertices.Select(v => v.ToString(2)).Distinct().Count();

                if(vertexCount != disticntVertexCount) {
                    return false;
                }
            }

            return true;
        }

        public static ILucidLine FixDuplicateVertices(ILucidLine geometry, int precision = 2)
        {
            var vertexHash = new HashSet<string>();
            var newVertices = new List<ILucidVertex>();

            var vertices = geometry.Vertices.ToArray();
            for(int i = 0; i < vertices.Length; i++) {
                var vertex = vertices[i];
                var id = vertex.ToString(precision);
                if(!vertexHash.Contains(id)) {
                    vertexHash.Add(id);
                    newVertices.Add(vertex);
                } else if(i == vertices.Length - 1) {
                    // We want to remove the interior duplicate not the last vertex
                    newVertices.Remove(newVertices.First(v => v.ToString(precision) == id));
                    newVertices.Add(vertex);
                }
            }

            var lineOut =LucidLine.Create(newVertices);
            return lineOut;
        }

        public static bool Intersects(IPoint point, ILucidLine line, bool infinitLine = false, int precision = 1)
        {
            var coincidentPoint = line.Vertices.Where(v => v.IsCoincident(point)).FirstOrDefault();

            if (coincidentPoint == null) {
                var pointLine = LucidLine.Create(new IPoint[] {
                    new LucidPoint(0, 0),
                    point
                });

                var intersection = LineIntersections(pointLine, line, infinitLine).FirstOrDefault();
                if (intersection == null || !GeometryHelper.IsCoincident(intersection, point, precision)) {
                    return false;
                }
            }

            return true;
        }

        public static bool IsCoincident(ILucidLine first, ILucidLine second)
        {
            foreach(var vertex in first.Vertices) {
                if(!Intersects(vertex, second)) {
                    return false;
                }
            }

            return true;
        }

        public static IEnumerable<ILucidGeometry> FurthestGeometry(IEnumerable<ILucidGeometry> geometries, IPoint referencePoint)
        {
            List<ILucidGeometry> furthest= new List<ILucidGeometry>(4);
            double furthestDistance = 0d;

            foreach(var geometry in geometries) {
                var nearestVertex = NearestVertex(geometry.Vertices, referencePoint);
                var distance = nearestVertex.Distance;

                if(distance > furthestDistance) {
                    furthestDistance = distance;
                    if (furthest.Count == 0 || furthest[0] != geometry) {
                        furthest.Clear();
                        furthest.Add(geometry);
                    }
                } else if(distance == furthestDistance)
                {
                    furthest.Add(geometry);
                }
            }

            return furthest.ToArray();
        }

        public static IEnumerable<ILucidGeometry> NearestGeometry(IPoint referencePoint, params ILucidGeometry[] geometries) 
        {
            List<ILucidGeometry> nearest = new List<ILucidGeometry>(4);
            double nearestDistance = 1000000000d;

            foreach(var geometry in geometries) {
                var nearestVertex = NearestVertex(geometry.Vertices, referencePoint);
                var distance = nearestVertex.Distance;

                if(distance < nearestDistance) {
                    nearestDistance = distance;
                    if (nearest.Count == 0 || nearest[0] != geometry) {
                        nearest.Clear();
                        nearest.Add(geometry);
                    }
                } else if(distance == nearestDistance)
                {
                    nearest.Add(geometry);
                }
            }

            return nearest.ToArray();
        }

        /// <summary>
        /// Rotates a point around a reference point by the provide angle in radians
        /// </summary>
        /// <param name="point">The point to rotate</param>
        /// <param name="referencePoint">The reference point to rotate about</param>
        /// <param name="angle">The angle to rotate it in radians</param>
        /// <returns>The rotated point</returns>
        public static IPoint Rotate(IPoint point, IPoint referencePoint, double angle)
        {
            var sine = Math.Sin(angle);
            var cosine = Math.Cos(angle);

            var pointOut = new LucidPoint(point.X - referencePoint.X, point.Y - referencePoint.Y);

            var newX = pointOut.X * cosine - pointOut.Y * sine;
            var newY = pointOut.X * sine + pointOut.Y * cosine;

            pointOut.X = newX + referencePoint.X;
            pointOut.Y = newY + referencePoint.Y;

            return pointOut;
        }

        /// <summary>
        /// Finds the vertex which is the furthest from the referencePoint in relation to all vertices provided
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="referencePoint"></param>
        /// <returns>A tuple of type (IPoint, double) of the further vertex 'Point' and its distance 'Distance'</returns>
        public static (IPoint Point, double Distance) FurthestVertex(IEnumerable<IPoint> vertices, IPoint referencePoint)
        {
            var maxDistance = 0d;
            IPoint furthestPoint = null;
            foreach (var vertex in vertices) {
                var distance = DistanceBetween(referencePoint, vertex);
                if (distance > maxDistance) {
                    maxDistance = distance;
                    furthestPoint = vertex;
                }
            }

            return (furthestPoint, maxDistance);
        }

        /// <summary>
        /// Finds the verticies which are the furthest from the referencePoint in relation to all vertices provided. If multiple vertices are found with the same
        /// furthest distance, they will all be returned
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="referencePoint"></param>
        /// <returns></returns>
        public static (IPoint[] Point, double Distance) FurthestVertices(IEnumerable<IPoint> vertices, IPoint referencePoint)
        {
            var maxDistance = 0d;
            List<IPoint> furthest = new List<IPoint>(4);
            foreach (var vertex in vertices)
            {
                var distance = DistanceBetween(referencePoint, vertex);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    furthest.Clear();
                    furthest.Add(vertex);
                } else if(distance == maxDistance)
                {
                    furthest.Add(vertex);
                }
            }

            return (furthest.ToArray(), maxDistance);
        }

        /// <summary>
        /// Finds the average of the provided angles in degrees
        /// </summary>
        /// <param name="angles"></param>
        /// <returns></returns>
        public static double AverageAngle(params double[] angles)
        {
            var sines = angles.Select(a => Math.Sin(a * (Math.PI / 180))).Sum();
            var cosines = angles.Select(a => Math.Cos(a * (Math.PI / 180))).Sum();

            var angleOut = Math.Atan2(sines, cosines) * (180 / Math.PI);
            return angleOut;
        }

        /// <summary>
        /// Returns the nearest line segment from a line
        /// </summary>
        /// <param name="wholeLine"></param>
        /// <param name="referencePoint"></param>
        /// <returns></returns>
        public static ILucidLine NearestLineSegment(ILucidLine wholeLine, IPoint referencePoint)
        {
            if (wholeLine.Vertices.Count() == 2)
                return wholeLine;

            var vertices = wholeLine.Vertices.SlidingWindow(2);
            var first = LucidLine.Create(vertices.First());
            var last = LucidLine.Create(vertices.Last());

            var nearest = NearestGeometry(referencePoint, first, last).First();
            return (ILucidLine)nearest;
        }

        public static (IPoint Point, double Distance) NearestVertex(IEnumerable<IPoint> vertices, IPoint referencePoint)
        {
            var minDistance = 100000000d;
            IPoint closestPoint = null;
            foreach (var vertex in vertices) {
                var distance = DistanceBetween(referencePoint, vertex);
                if (distance < minDistance) {
                    minDistance = distance;
                    closestPoint = vertex;
                }
            }

            return (closestPoint, minDistance);
        }
        public static bool SelfIntersects(ILucidLine lineIn)
        {
            return RemoveSelfIntersections(lineIn).Count() != lineIn.Vertices.Count();
        }

        public static IEnumerable<ILucidVertex> RemoveSelfIntersections(ILucidLine lineIn)
        {
            var segments = (from s in lineIn.Vertices.SlidingWindow(2)
                            select s).Where(w => w.Count() == 2).ToArray();

            var state = true;
            foreach(var segment in segments) {
                if(state == true) {
                    yield return segment.First();
                }

                foreach(var otherSegment in from s in segments
                                            where s != segment
                                            select s) {
                    var intersect = GeometryHelper.LineIntersections(LucidLine.Create(segment), LucidLine.Create(otherSegment)).Any();
                    if(intersect) {
                        state = !state;
                        break;
                    }
                }
            }

            yield return lineIn.Vertices.Last();
        }

        public static IEnumerable<ILucidExtent> SubdivideExtent(ILucidExtent extent, int width, int height)
        {
            var extentWidth = Math.Ceiling(extent.Width / width);
            var extentHeight = Math.Ceiling(extent.Height / height);

            for(int c = 0; c < width; c++) {
                for (var r = 0; r < height; r++) {
                    var windowX = extent.XMin + (c * extentWidth);
                    var windowY = extent.YMin + (r * extentHeight);
                    yield return new LucidExtent() {
                        XMax = windowX + extentWidth,
                        XMin = windowX,
                        YMax = windowY + extentHeight,
                        YMin = windowY
                    };
                }
            }
        }

        public static bool Intersects(IPoint point, ILucidExtent extent)
        {
            if (point.X < extent.XMin) return false;
            if (point.X > extent.XMax) return false;
            if (point.Y < extent.YMin) return false;
            if (point.Y > extent.YMax) return false;

            return true;
        }

        public static bool Intersects(ILucidExtent first, ILucidExtent second)
        {
            if (first.XMax < second.XMin) return false;
            if (first.XMin > second.XMax) return false;
            if (first.YMax < second.YMin) return false;
            if (first.YMin > second.YMax) return false;

            return true;
        }

        public static bool Intersects(ILucidGeometry first, ILucidGeometry second)
        {
            return Intersects(CalculateExtent(first), CalculateExtent(second));
        }

        /// <summary>
        /// Returns the point of intersection or null if they are parallel
        /// </summary>
        public static IPoint Intersection(AlgebraicLine first, AlgebraicLine second)
        {
            var delta = (first.DeltaY * second.DeltaX) - (second.DeltaY * first.DeltaX);
            if (delta == 0)
                return null;

            double x = (second.DeltaX * first.C - first.DeltaX * second.C) / delta;
            double y = (first.DeltaY * second.C - second.DeltaY * first.C) / delta;

            return new LucidVertex() {
                X = x,
                Y = y
            };
        }

        public static IEnumerable<IPoint> LineIntersections(ILucidLine first, ILucidLine second, bool infinitLines = false)
        {
            var firstExtent = CalculateExtent(first);
            var secondExtent = CalculateExtent(second);

            if (infinitLines || GeometryHelper.Intersects(first, second)) {
                foreach (var firstLine in from p in first.Vertices.TumblingWindow(2)
                                          select new AlgebraicLine(p.First(), p.Last())) { 

                    foreach (var secondLine in from p in second.Vertices.TumblingWindow(2)
                                               select new AlgebraicLine(p.First(), p.Last())) { 
                        var intersection = Intersection(firstLine, secondLine);

                        if (intersection != null && (infinitLines || (GeometryHelper.Intersects(intersection, firstExtent) && GeometryHelper.Intersects(intersection, secondExtent)))) {
                            yield return intersection;
                        }
                    }
                }
            }
        }

        public static IEnumerable<ILucidLine> Split(ILucidLine lineIn, ILucidLine by)
        {
            return Split(lineIn.Vertices, by);
        }

        /// <summary>
        /// Calls the supplied action function for each vertex passed in including the vertex itself and its index
        /// </summary>
        /// <param name="geometry">Vertices to iterate</param>
        /// <param name="action">The action to call on each vertex</param>
        public static void ForeachVertex(IEnumerable<IPoint> geometry, Action<IPoint, int> action)
        {
            int index = 0;
            foreach(var vertex in geometry) {
                action(vertex, index++);
            }
        }

        public static IEnumerable<ILucidGeometry> FindNearby(IEnumerable<IPoint> referencePoints, double distance, IEnumerable<ILucidGeometry> geometries, bool recursive = false)
        {
            var nearbyGeometries = (from g in geometries
                                   let nearbyVertices = from v in g.Vertices
                                                        where referencePoints.Where(p => GeometryHelper.DistanceBetween(p, v) < distance).Any()
                                                        select v
                                   where nearbyVertices.Any()
                                   select g);

            foreach (var geometry in nearbyGeometries) {
                yield return geometry;
            }

            if (recursive && nearbyGeometries.Any()) {
                foreach (var nearby in FindNearby(nearbyGeometries.SelectMany(g => g.Vertices), distance, geometries.Except(nearbyGeometries), recursive)) {
                    yield return nearby;
                }
            }
        }

        public static LucidMultipoint CreateMultipoint(IEnumerable<ILucidVertex> vertices, LucidSpatialReference spatialReference = null)
        {
            return LucidMultipoint.Create(vertices, spatialReference);
        }

        public static double Length(ILucidLine line)
        {
            double length = 0;
            foreach(var subline in line.Vertices.SlidingWindow(2)) {
                length += Math.Abs(GeometryHelper.DistanceBetween(subline.First(), subline.Last()));
            }

            return length;
        }

        /// <summary>
        /// Splits the path by the supplied by line
        /// </summary>
        /// <param name="path">Path to split</param>
        /// <param name="cutter">Cutter to cut the path by</param>
        /// <returns>An enumeration of parts, or a single part if not split occurred</returns>
        public static IEnumerable<ILucidLine> Split(IEnumerable<IPoint> path, ILucidLine cutter)
        {
            var current = new LucidLine();
            current.AddVertex(path.First());

            foreach (var vertices in path.SlidingWindow(2)) {
                var line = new AlgebraicLine(vertices);
                var lineExtent = CalculateExtent(line);

                foreach (var cutterLine in cutter.Vertices.SlidingWindow(2)) {
                    var byLine = new AlgebraicLine(cutterLine);
                    var intersection = Intersection(line, byLine);

                    if (intersection != null && Intersects(intersection, lineExtent) &&
                        Intersects(intersection, CalculateExtent(byLine))) {
                        current.AddVertex(intersection);
                        yield return current;

                        current = new LucidLine();
                        current.AddVertex(intersection);
                        break;
                    }
                }

                current.AddVertex(line.End);
            }

            yield return current;
        }

        /// <summary>
        /// Splits the path at the given point if the point lies on the path.
        /// </summary>
        /// <param name="path">Path to split</param>
        /// <param name="splitPoint">Point at which to cut the path</param>
        /// <returns>An enumeration of parts, or a single part if no split occurred</returns>
        public static IEnumerable<ILucidLine> Split(IEnumerable<IPoint> path, IPoint splitPoint)
        {
            var current = new LucidLine();
            current.AddVertex(path.First());

            foreach (var vertices in path.SlidingWindow(2))
            {
                var line = new AlgebraicLine(vertices);

                var lineExtent = CalculateExtent(line);
                if (Intersects(splitPoint, lineExtent))
                {
                    current.AddVertex(splitPoint);
                    yield return current;

                    current = new LucidLine();
                    current.AddVertex(splitPoint);
                }

                current.AddVertex(line.End);
            }

            yield return current;
        }
        public static double AngleToRadians(int degrees)
        {
            return AngleToRadians(Convert.ToDouble(degrees));
        }

        public static double AngleToRadians(double degrees)
        {
            return degrees * (Math.PI / 180d);
        }

        public static double AngleToDegrees(double angleInRadians)
        {
            return angleInRadians * (180d / Math.PI);
        }

        /// <summary>
        /// Calculates an angle between a origin reference point and target point in radians
        /// </summary>
        /// <param name="referencePoint">The origin point</param>
        /// <param name="targetPoint">The target point</param>
        /// <returns>The angle between the origin and the target in radians.</returns>
        public static double Angle(IPoint referencePoint, IPoint targetPoint)
        {
            return Math.Atan2(targetPoint.Y - referencePoint.Y, targetPoint.X - referencePoint.X);
        }

        /// <summary>
        /// Returns the 
        /// </summary>
        /// <param name="angleDegrees"></param>
        /// <returns></returns>
        public static double PerpendicularAngle(double angleDegrees)
        {
            var bisectOut = angleDegrees + 90;
            return bisectOut >= 360 ? bisectOut - 360 : bisectOut;
        }

        /// <summary>
        /// Determines the angle in radians of a targetPoint vector with the origin being (0,0).
        /// </summary>
        /// <param name="targetPoint">The point to find the angle of.</param>
        /// <returns>The angle of the target point about (0,0)</returns>
        public static double Angle(IPoint targetPoint)
        {
            return Angle(LucidPoint.Create(0, 0), targetPoint);
        }

        /// <summary>
        /// Denotes if two points intersect exactly
        /// </summary>
        /// <param name="first">The first point to test.</param>
        /// <param name="second">The second point to test.</param>
        /// <returns>True if the points are exactly intersecting.</returns>
        public static bool Intersects(IPoint first, IPoint second)
        {
            if (first.X != second.X) return false;
            if (first.Y != second.Y) return false;

            return true;
        }

        /// <summary>
        /// Dentoes if two points are almost coincident provided some epsilon.
        /// </summary>
        /// <param name="first">The first point to test.</param>
        /// <param name="second">The second point to test.</param>
        /// <param name="epsilon">The proximity to coincidence required.</param>
        /// <returns></returns>
        public static bool NearlyCoincident(IPoint first, IPoint second, double epsilon)
        {
            if (Math.Abs(first.X - second.X) > epsilon) return false;
            if (Math.Abs(first.Y - second.Y) > epsilon) return false;

            return true;
        }


        /// <summary>
        /// Finds the largest angular gap between lines that all touch at a reference point
        /// and returns the angle, in degrees, of a line passing through the reference point
        /// that bisects that widest angle.
        /// </summary>
        /// <param name="referencePoint">The point of reference for calculating angles.</param>
        /// <param name="lines">The lines to avoid</param>
        /// <returns>An optimal angle for placment for a point</returns>
        public static double GetPlacementAngleAvoidingExistingLines(IPoint referencePoint,
            IEnumerable<IEnumerable<IPoint>> existingLines)
        {
            var lineAngles = new SortedList<double, double>();
            foreach (var line in existingLines)
            {
                var lineVertices = line.ToList();

                if (lineVertices.Count < 2) continue;

                var anglePoints = new List<IPoint>();
                if (NearlyCoincident(referencePoint, lineVertices.First(), 0.001))
                {
                    anglePoints.Add(lineVertices[1]);
                }
                else if (NearlyCoincident(referencePoint, lineVertices.Last(), 0.001))
                {
                    anglePoints.Add(lineVertices[lineVertices.Count - 2]);
                }
                else
                {
                    var vertices = lineVertices.SlidingWindow(3);
                    foreach (var window3 in vertices)
                    {
                        var v3 = window3.ToList();
                        if (NearlyCoincident(v3[1].AsVertex(), referencePoint.AsVertex(), 0.001))
                        {
                            anglePoints.Add(v3[0]);
                            anglePoints.Add(v3[2]);
                        }
                    }
                }
                var angles = anglePoints.Select(p => GeometryHelper.Angle(referencePoint, p));

                // The angles so far are in the range (-Pi, Pi), but we need them normalized
                // to the range (0, 2Pi).
                angles = angles.Select(a => a >= 0 ? a : a + 2D * Math.PI);

                foreach (var a in angles)
                {
                    if (!lineAngles.ContainsKey(a))
                    {
                        lineAngles.Add(a, a);
                    }
                }
            }

            // Special case: if there is only one line terminating on the reference point,
            // then we will recommend placement at a 90 degree angle to that line.
            if (lineAngles.Count == 1)
            {
                var perpendicularAngle = lineAngles.Keys[0] + 0.5 * Math.PI;
                return 180D * perpendicularAngle / Math.PI;
            }

            // Find the bisector angle of the largest angular gap between adjacent line angles.
            double bisectorAngle = 0;
            double maxDiff = double.MinValue;
            for (int i = 0; i < lineAngles.Count; i++)
            {
                var angle1 = lineAngles.Keys[i];
                var angle2 = lineAngles.Keys[(i + 1) % lineAngles.Count];
                if (IsPrettyMuchTheSame(angle1, angle2, 0.000001)) continue;

                    var diff = angle2 - angle1;
                if (diff < 0) diff = 2d * Math.PI + diff;

                var bisector = angle1 + diff / 2.0D;
                if (bisector > 2D*Math.PI) bisector -= 2D * Math.PI;

                if (diff > maxDiff)
                {
                    maxDiff = diff;
                    bisectorAngle = bisector;
                }
            }

            return 180D * bisectorAngle / Math.PI;
        }

        /// <summary>
        /// Buffers an extent by the provided amount.
        /// </summary>
        /// <param name="extentIn">The extent to buffer.</param>
        /// <param name="amount">The amount to buffer the extent by.</param>
        /// <returns>A new extent representing the buffer.</returns>
        public static ILucidExtent Buffer(ILucidExtent extentIn, double amount)
        {
            return new LucidExtent() {
                XMax = extentIn.XMax += amount,
                XMin = extentIn.XMin -= amount,
                YMax = extentIn.YMax += amount,
                YMin = extentIn.YMin -= amount
            };
        }

        /// <summary>
        /// Returns a circular buffer geometry for a point with the provided radius and vertex count.
        /// </summary>
        /// <param name="point">The point to buffer.</param>
        /// <param name="radius">The radius to buffer the point by.</param>
        /// <param name="edgeCount">The number of vertices, complexity, of the produced circular buffer.</param>
        /// <returns></returns>
        public static ILucidPolygon BufferPoint(IPoint point, double radius, int edgeCount = 24)
        {
            var path = CircularPathFromPoint(point, radius, edgeCount);
            return LucidPolygon.Create(path.Vertices);
        }

        /// <summary>
        /// Creates a point at the provided angle in degrees and distance from the referencePoint
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="distance"></param>
        /// <param name="referencePoint"></param>
        /// <returns></returns>
        public static IPoint CreatePoint(double angle, double distance, IPoint referencePoint)
        {
            angle = angle * (Math.PI / 180);
            return new LucidPoint() {
                X = (distance * Math.Cos(angle)) + referencePoint.X,
                Y = (distance * Math.Sin(angle)) + referencePoint.Y
            }; 
        }

        /// <summary>
        /// Converts an angle in degrees to an angle where up, or north, is at 90 degrees and angular rotation is reversed 
        /// </summary>
        /// <param name="angleInDegrees">The angle in degrees to convert to a bearing</param>
        /// <returns>The provided angle as a bearing</returns>
        public static int ToNorthBearing(int angleInDegrees)
        {
            var bearing = (360 - angleInDegrees) + 90;
            while(bearing >= 360)
                bearing -= 360;

            return bearing; 
        }

        /// <summary>
        /// Creates a circular path form the provided referenc point, with the provided radius (size) and number of vertices
        /// </summary>
        /// <param name="point">Center point</param>
        /// <param name="size">The radius of the circular path around the center point</param>
        /// <param name="vertexOutCount">The number of vertices on the path</param>
        /// <returns></returns>
        public static ILucidLine CircularPathFromPoint(IPoint point, double size, int vertexOutCount)
        {
            double vertextSpacing = ((2d * Math.PI) / (double)vertexOutCount);
            double currentAngle = 0;
            var lineOut = new LucidLine();

            for (int i = 0; i < vertexOutCount; i++) {
                lineOut.AddVertex(new LucidVertex() {  
                    X = (size * Math.Cos(currentAngle)) + point.X,
                    Y = (size * Math.Sin(currentAngle)) + point.Y
                 });

                currentAngle += vertextSpacing;
            }

            lineOut.AddVertex(lineOut.Vertices.First());

            return lineOut;
        }

        /// <summary>
        /// Returns the extent of the provided geometries
        /// </summary>
        /// <param name="geometries">The geometries to calculate the extent from.</param>
        /// <returns>The geometries extent. </returns>
        public static ILucidExtent CalculateExtent(params ILucidGeometry[] geometries)
        {
            ILucidExtent extentOut = new LucidExtent();
            bool initialSet = false;
            int count = 0;

            foreach (var geometry in geometries) {
                foreach (var point in geometry.Vertices) {
                count++;
                    var x = point.X;
                    var y = point.Y;

                    if (!initialSet) {
                        extentOut.XMax = extentOut.XMin = x;
                        extentOut.YMax = extentOut.YMin = y;

                        initialSet = true;
                    }
                    else {

                        if (extentOut.XMin > x)
                            extentOut.XMin = x;

                        if (extentOut.XMax < x)
                            extentOut.XMax = x;

                        if (extentOut.YMin > y)
                            extentOut.YMin = y;

                        if (extentOut.YMax < y)
                            extentOut.YMax = y;
                    }

                }
            }

            if (count == 1)
                extentOut = GeometryHelper.Buffer(extentOut, .5);


            return extentOut;
        }
    }
}
