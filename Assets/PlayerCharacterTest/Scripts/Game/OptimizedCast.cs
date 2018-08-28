using UnityEngine;
using System.Collections;
using System.Linq;

public static class OptimizedCast
{
    public static RaycastHit[] CapsuleCastAll(Vector3 point1, Vector3 point2, float radius, Vector3 direction, float distance, int layermask)
    {
        Vector3 optPoint1 = point1 - direction * radius;
        Vector3 optPoint2 = point2 - direction * radius;
        float optDistance = distance + radius;

        RaycastHit[] hits = Physics.CapsuleCastAll(optPoint1, optPoint2, radius, direction, optDistance, layermask);

        hits = hits.Where(x => OptimizedCapsuleCastFilter(x, optPoint1, optPoint2, radius, direction)).ToArray();

        for (int i = 0; i < hits.Length; i++)
            hits[i].distance -= radius;

        return hits;
    }

    public static RaycastHit[] CapsuleCastAll(Vector3 point1, Vector3 point2, float radius, Vector3 direction, float distance)
    {
        int layermask = -5;
        return CapsuleCastAll(point1, point2, radius, direction, distance, layermask);
    }

    public static RaycastHit[] CapsuleCastAll(Vector3 point1, Vector3 point2, float radius, Vector3 direction)
    {
        int layermask = -5;
        float distance = float.PositiveInfinity;
        return CapsuleCastAll(point1, point2, radius, direction, distance, layermask);
    }

    public static RaycastHit[] SphereCastAll(Vector3 origin, float radius, Vector3 direction, float distance, int layerMask)
    {
        return CapsuleCastAll(origin, origin, radius, direction, distance, layerMask);
    }

    public static RaycastHit[] SphereCastAll(Vector3 origin, float radius, Vector3 direction, float distance)
    {
        int layerMask = -5;
        return SphereCastAll(origin, radius, direction, distance, layerMask);
    }

    public static RaycastHit[] SphereCastAll(Vector3 origin, float radius, Vector3 direction)
    {
        int layerMask = -5;
        float distance = float.PositiveInfinity;
        return SphereCastAll(origin, radius, direction, distance, layerMask);
    }

    public static RaycastHit[] SphereCastAll(Ray ray, float radius, float distance, int layerMask)
    {
        return SphereCastAll(ray.origin, radius, ray.direction, distance, layerMask);
    }

    public static RaycastHit[] SphereCastAll(Ray ray, float radius, float distance)
    {
        int layerMask = -5;
        return SphereCastAll(ray, radius, distance, layerMask);
    }

    public static RaycastHit[] SphereCastAll(Ray ray, float radius)
    {
        int layerMask = -5;
        float distance = float.PositiveInfinity;
        return SphereCastAll(ray, radius, distance, layerMask);
    }



    private static bool OptimizedCapsuleCastFilter(RaycastHit hit, Vector3 optPoint1, Vector3 optPoint2, float radius, Vector3 direction)
    {
        Vector3 point1 = optPoint1 + direction * radius;
        Vector3 point2 = optPoint2 + direction * radius;

        if (hit.distance > radius)
            return true;

        if (PointInsideCapsule(optPoint1, optPoint2, radius, hit.point))
            return false;

        if (!PointInsideCapsule(point1, point2, radius, hit.point))
            return false;

        return true;
    }

    private static bool PointInsideCapsule(Vector3 point1, Vector3 point2, float radius, Vector3 point)
    {
        if (Vector3.Distance(point, point1) <= radius)
            return true;

        if (Vector3.Distance(point, point2) <= radius)
            return true;

        if (Vector3.Distance(point1, point2) == 0)
            return false;

        Vector3 capsuleDirection = Vector3.Normalize(point2 - point1);
        float capsuleLength = Vector3.Distance(point2, point1);

        float crossMag = Vector3.Cross(point - point1, capsuleDirection).magnitude;
        float dot = Vector3.Dot(point - point1, capsuleDirection);

        if (crossMag > radius)
            return false;

        if (dot < 0 || dot > capsuleLength)
            return false;

        return true;
    }
}
