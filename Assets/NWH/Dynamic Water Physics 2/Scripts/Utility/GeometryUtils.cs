using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NWH.DWP2
{
    public static class GeometryUtils
    {
        public static Vector3 CalculateCentroid(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 pointD)
        {
            Vector3 centroid = (pointA + pointB + pointC + pointD) / 4;
            return centroid;
        }
        
        public static float CalculateQuadrilateralArea(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 pointD)
        {
            // Calculate the area of the first triangle (pointA, pointB, pointC)
            float areaTriangle1 = CalculateTriangleArea(pointA, pointB, pointC);

            // Calculate the area of the second triangle (pointA, pointC, pointD)
            float areaTriangle2 = CalculateTriangleArea(pointA, pointC, pointD);

            // Return the sum of both triangle areas
            return areaTriangle1 + areaTriangle2;
        }

        public static float CalculateTriangleArea(Vector3 pointA, Vector3 pointB, Vector3 pointC)
        {
            Vector3 vectorAB = pointB - pointA;
            Vector3 vectorAC = pointC - pointA;

            // Calculate the cross product of vectorAB and vectorAC
            Vector3 crossProduct = Vector3.Cross(vectorAB, vectorAC);

            // Calculate the magnitude of the cross product and divide by 2 to get the area
            return crossProduct.magnitude / 2;
        }
        
        public static Vector3 CalculateSurfaceWeightedCenter(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 pointD)
        {
            // Calculate the area and centroid of the first triangle (pointA, pointB, pointC)
            float areaTriangle1 = CalculateTriangleArea(pointA, pointB, pointC);
            Vector3 centroidTriangle1 = CalculateCentroid(pointA, pointB, pointC);

            // Calculate the area and centroid of the second triangle (pointA, pointC, pointD)
            float areaTriangle2 = CalculateTriangleArea(pointA, pointC, pointD);
            Vector3 centroidTriangle2 = CalculateCentroid(pointA, pointC, pointD);

            // Calculate the surface-weighted center
            float totalArea = areaTriangle1 + areaTriangle2;
            Vector3 surfaceWeightedCenter = (centroidTriangle1 * areaTriangle1 + centroidTriangle2 * areaTriangle2) / totalArea;

            return surfaceWeightedCenter;
        }

        private static Vector3 CalculateCentroid(Vector3 pointA, Vector3 pointB, Vector3 pointC)
        {
            return (pointA + pointB + pointC) / 3;
        }
    }
}

