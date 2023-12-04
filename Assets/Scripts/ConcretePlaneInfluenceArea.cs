using UnityEngine;

public class ConcretePlaneInfluenceArea : ConcreteInfluenceArea
{
    public static new ConcretePlaneInfluenceArea initialize(InfluenceArea influenceArea)
    {
        var gameObject = new GameObject("InfluenceArea");
        var area = gameObject.AddComponent<ConcretePlaneInfluenceArea>();

        //set position
        area.transform.position = influenceArea.plane.Point;
        area.transform.forward = influenceArea.plane.Normal;

        //set camera
        var cam = gameObject.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 20f;
        cam.targetDisplay = 0;

        return area;
    }

    /// <summary>
    /// Draws the gizmo for a plane ray caster.
    /// </summary>
    protected override void drawGizmo(Color color)
    {
        Gizmos.color = color;

        //transform the extents in world space
        var extent = InfluenceArea.Size;
        Vector3 topRight = (Vector3)(transform.localToWorldMatrix * new Vector3(extent.x, extent.y)) + transform.position;
        Vector3 topLeft = (Vector3)(transform.localToWorldMatrix * new Vector3(-extent.x, extent.y)) + transform.position;
        Vector3 bottomRight = (Vector3)(transform.localToWorldMatrix * new Vector3(extent.x, -extent.y)) + transform.position;
        Vector3 bottomLeft = (Vector3)(transform.localToWorldMatrix * new Vector3(-extent.x, -extent.y)) + transform.position;

        //draws the rectangle (in world space)
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);
        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
    }

    /// <summary>
    /// Generates random rays with the same directionas the PoV and origin inside the specified rectangle (in world space).
    /// </summary>
    protected override void generateRays()
    {
        Random.InitState(seed); //initialize randomness
        rays = new Ray[raysAmount];
        var extent = InfluenceArea.Size;

        for (int i = 0; i < raysAmount; i++)
        {
            Vector3 rand = new Vector3(Random.Range(-extent.x, extent.x), Random.Range(-extent.y, extent.y)); //get a random position on the plane in local space
            Vector3 pos = (Vector3)(transform.localToWorldMatrix * rand) + InfluenceArea.plane.Point; //transform the position of the ray in world space
            rays[i] = new Ray(pos, InfluenceArea.plane.Normal);
        }

        intersectionInfo = collectStats();
    }
}
