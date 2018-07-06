# LucidGeometry 

Provides a simple set of geometry objects matching the ESRI Rest SDK schema.

The interface shared by all geometry is simple:

```c#
public interface ILucidGeometry
{
    IEnumerable<ILucidVertex> Vertices { get; }
}
```

#### Types
The core types are:
- LucidVertex
- LucidPoint
- LucidMultiPoint
- LucidLine 
- LucidExtent
- LucidPolygon
- LucidVector

#### GeometryHelper

The geometry helper static class provides most of the utility methods and
operator overrides.

Use the static `Create` method on each type to create new instances.
```c#
   var newPoint = LucidPoint.Create(0, 0);
   var newLine = LucidLine.Create(
       newPoint,
       LucidPoint.Create(1,1)
   );
``` 

#### Geometric arithmetic

The Lucid Geomtry library supports numerous arithmetic operations on 
geometry. Most type can be added and subtracted from each other.

```c#
   var aPoint = LucidPoint.Create(0, 0);
   var bPoint = LucidPoint.Create(1, 1); 
   var cPoint = LucidPoint.Create(0, 1);

   // Intersection test
   var doIntersect = aPoint == bPoint;

   var newLine = LucidLine.Create(aPoint, bPoint);
   
   // Line and Point addition
   var extendedLine = newLine + cPoint;

   var aPolygon = ...;
   var bPolygon = ...;

   // Polygonal subtration
   var intersectionPolygon = aPolygon - bPolygon;
   if(intersectionPolygon == null)
      // a and b do not intersect

   // Union
   var mergedPolygon = aPoygon + bPolygon;

```

#### Vector arithmetic

Vector arithmetic is supported through the `LucidVector` type.

```c#
   var vector1 = LucidVector.Create(0, 1);
   var vector2 = LucidVector.Create(2, 3);

   // Common arithmetic support
   var dotProduct = vector1 * vector2; 
   var scaledVector = vector1 * 2 ;

   var addition = vector1 + vector2;
   var subtration = vector1 - vector2;

   // Standard properties
   var magnitude = scaledVector.Magnitude;
   var angle = scaledVector.Angle;
```