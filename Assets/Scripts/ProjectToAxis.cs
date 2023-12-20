using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteAlways]
public class ProjectToAxis : MonoBehaviour
{
    [SerializeField] Vector3 axis;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(Vector3.zero, axis * 100);

        var originalPoints = getPoints();
        var (points, minIndex, maxIndex) = projectPoints();

        for(int i = 0; i < points.Count; i++)
        {
            if(i == minIndex) Gizmos.color = Color.green;
            else if(i == maxIndex) Gizmos.color = Color.red;
            else Gizmos.color = Color.blue;

            Gizmos.DrawSphere(originalPoints[i], 0.05f);
            Gizmos.DrawSphere(points[i], 0.05f);
            Gizmos.DrawLine(originalPoints[i], points[i]);
        }
    }

    List<Vector3> getPoints()
    {
        var res = new List<Vector3>();
        for (int i = -1; i <= 1; i += 2)
            for (int j = -1; j <= 1; j += 2)
                for (int k = -1; k <= 1; k += 2)
                    res.Add(transform.position + Vector3.Scale(transform.lossyScale / 2f, new Vector3(i, j, k)));
        return res;
    }

    (List<Vector3> projected, int minIndex, int maxIndex) projectPoints()
    {
        var points = getPoints();
        var res = new List<Vector3>();
        (float min, int minIndex) = (float.MaxValue, -1); 
        (float max, int maxIndex) = (-float.MaxValue, -1);

        for (int i = 0; i < points.Count; i++)
        {
            float pointDistance = Vector3.Dot(points[i], axis.normalized);
            if (pointDistance <= min) (min, minIndex) = (pointDistance, i);
            if (pointDistance >= max) (max, maxIndex) = (pointDistance, i);
            res.Add(axis.normalized * pointDistance);
        }

        return (res, minIndex, maxIndex);
    }
}
