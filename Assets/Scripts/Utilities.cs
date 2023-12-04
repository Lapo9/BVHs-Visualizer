using UnityEngine;

public class Utilities
{
    /// <summary>
    /// Draws the gizmo for an AABB.
    /// </summary>
    /// <param name="points">
    /// Must have length 8, and nodesMeshes should be in this layout:
    ///  2_______________6  
    ///  |\              \ 
    ///  | \______________\7
    /// 0| 3|          4. |
    ///   \ |             |
    ///    \|_____________|
    ///     1             5
    /// </param>
    public static void drawAabbGizmo(Aabb aabb, Color color = new Color())
    {
        var points = aabbToPoints(aabb);

        if (color != new Color()) Gizmos.color = color;

        Gizmos.DrawLine(points[0], points[1]);
        Gizmos.DrawLine(points[1], points[3]);
        Gizmos.DrawLine(points[3], points[2]);
        Gizmos.DrawLine(points[2], points[0]);

        Gizmos.DrawLine(points[4], points[5]);
        Gizmos.DrawLine(points[5], points[7]);
        Gizmos.DrawLine(points[7], points[6]);
        Gizmos.DrawLine(points[6], points[4]);

        Gizmos.DrawLine(points[0], points[4]);
        Gizmos.DrawLine(points[1], points[5]);
        Gizmos.DrawLine(points[3], points[7]);
        Gizmos.DrawLine(points[2], points[6]);
    }

    /// <summary>
    /// Given an AABB (min-max form) returns the array of it 8 vertices with this layout:
    ///  2_______________6  
    ///  |\              \ 
    ///  | \______________\7
    /// 0| 3|          4. |
    ///   \ |             |
    ///    \|_____________|
    ///     1             5
    /// </summary>
    public static Vector3[] aabbToPoints(Aabb aabb)
    {
        Vector3[] points = new Vector3[8];
        Vector3 dimensions = aabb.Max - aabb.Min;
        for (int i = 0; i < 2; i++)
            for (int j = 0; j < 2; j++)
                for (int k = 0; k < 2; k++)
                {
                    Vector3 vertex = new Vector3(i, j, k); //selects which vertex we are considering (e.g. top-left-back)
                    points[i * 4 + j * 2 + k] = aabb.Min + Vector3.Scale(vertex, dimensions);
                }
        return points;
    }

    /// <summary>
    /// Draws the gizmo for a triangle.
    /// </summary>
    public static void drawTriangleGizmo(Triangle triangle, Color color = new Color())
    {
        if (color != new Color()) Gizmos.color = color;
        Gizmos.DrawLine(triangle.V1, triangle.V2);
        Gizmos.DrawLine(triangle.V2, triangle.V3);
        Gizmos.DrawLine(triangle.V3, triangle.V1);

    }
}
