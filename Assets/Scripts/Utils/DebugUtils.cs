using UnityEngine;

public static class DebugUtils
{
    public static void DebugDrawArrow(Vector2 start, Vector2 end, Color color = default, float duration = 0)
    {
        ValidateDefaultValues(ref color, ref duration);

        Debug.DrawLine(start, end, color, duration);

        Vector2 center = (end - start) / 2;
        Vector2 dir = (end - start);
        float length = dir.magnitude;
        dir /= length;
        Vector2 perp = new Vector2(-dir.y, dir.x);

        Vector2 p1 = start + center + perp * length / 4;
        Vector2 p2 = start + center - perp * length / 4;
        Debug.DrawLine(end, p1, color, duration);
        Debug.DrawLine(end, p2, color, duration);
    }

    public static void DebugDrawCircle(Vector2 center, float radius, Color color = default, float duration = 0)
    {
        ValidateDefaultValues(ref color, ref duration);

        const int CIRCLE_POINT_COUNT = 16;

        Vector2 prevPoint = center + Vector2.right * radius;
        for (int i = 1; i < CIRCLE_POINT_COUNT; ++i)
        {
            float delta = (float)i / (CIRCLE_POINT_COUNT - 1);
            float angle = delta * 2 * Mathf.PI;

            Vector2 point = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius + center;
            Debug.DrawLine(prevPoint, point, color, duration);
            prevPoint = point;
        }
    }

    public static void DebugDrawSquare(Vector2 center, float radius, Color color = default, float duration = 0)
    {
        ValidateDefaultValues(ref color, ref duration);

        Vector2 oneMinus = new Vector2(1, -1);
        Debug.DrawLine(center + Vector2.one * radius, center + oneMinus * radius, color, duration);
        Debug.DrawLine(center + Vector2.one * radius, center - oneMinus * radius, color, duration);
        Debug.DrawLine(center - Vector2.one * radius, center + oneMinus * radius, color, duration);
        Debug.DrawLine(center - Vector2.one * radius, center - oneMinus * radius, color, duration);
    }

    public static void DebugDrawRectangle(Vector2 center, Vector2 size, Color color = default, float duration = 0)
    {
        ValidateDefaultValues(ref color, ref duration);

        Vector2 oneMinus = new Vector2(1, -1);
        Debug.DrawLine(center + size, center + size * oneMinus, color, duration);
        Debug.DrawLine(center + size, center + size * -oneMinus, color, duration);
        Debug.DrawLine(center - size, center + size * oneMinus, color, duration);
        Debug.DrawLine(center - size, center + size * -oneMinus, color, duration);
    }

    //public static void DrawDrawConnections(IEnumerable<BodyConnection> connections, Color color = default, float duration = 0)
    //{
    //    if (connections == null) return;

    //    ValidateDefaultValues(ref color, ref duration);

    //    foreach (BodyConnection connection in connections)
    //    {
    //        DebugDrawCircle(connection.Point1.Position, connection.Point1.Radius, color, duration);
    //        DebugDrawCircle(connection.Point2.Position, connection.Point2.Radius, color, duration);
    //        DebugDrawArrow(connection.Point1.Position, connection.Point2.Position, color, duration);
    //    }
    //}

    //public static void DrawDrawConnection(BodyConnection connection, Color color = default, float duration = 0)
    //{
    //    if(connection == null) return;

    //    ValidateDefaultValues(ref color, ref duration);

    //    DebugDrawCircle(connection.Point1.Position, connection.Point1.Radius, color, duration);
    //    DebugDrawCircle(connection.Point2.Position, connection.Point2.Radius, color, duration);
    //    DebugDrawArrow(connection.Point1.Position, connection.Point2.Position, color, duration);
    //}

    private static void ValidateDefaultValues(ref Color c, ref float d)
    {
        if (c == default) c = Color.white;
        if (d == 0) d = Time.deltaTime;
    }
}