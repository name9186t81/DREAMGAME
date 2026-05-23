using UnityEngine;

public static class MathUtils
{
    public static void CreatePlane(this Vector3 direction, out Vector3 u, out Vector3 v)
    {
        Vector3 normal = direction.normalized;
        Vector3 temp = Mathf.Abs(normal.y) < 0.99f ? Vector3.up : Vector3.right;

        u = Vector3.Cross(temp, normal);
        v = Vector3.Cross(normal, u);
    }
}