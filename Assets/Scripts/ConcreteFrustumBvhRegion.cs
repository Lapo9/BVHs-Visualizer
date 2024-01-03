using UnityEngine;

public class ConcreteFrustumBvhRegion : ConcreteBvhRegion
{
    [SerializeField] Vector3[] vertices; //to show the vertices in the inspector

    /// <summary>
    /// Initializes the object.
    /// </summary>
    public static new ConcreteFrustumBvhRegion initialize(BvhRegion bvhRegion)
    {
        var gameObject = new GameObject("BvhRegion");
        var region = gameObject.AddComponent<ConcreteFrustumBvhRegion>();

        //set position
        region.transform.position = (bvhRegion.frustum.Vertices[1] + bvhRegion.frustum.Vertices[6]) / 2f; //we compute the mid point of one diagonal of the near face of the frustum
        Vector3 right = bvhRegion.frustum.Vertices[5] - bvhRegion.frustum.Vertices[1];
        Vector3 up = bvhRegion.frustum.Vertices[3] - bvhRegion.frustum.Vertices[1];
        region.transform.forward = Vector3.Cross(right, up); //the forward direction is perpendicular to the right-up plane

        return region;
    }

    /// <summary>
    /// Draws the gizmo for a Frustum region.
    /// </summary>
    protected override void drawGizmo(Color color)
    {
        vertices = BvhRegion.frustum.Vertices; //update vertices in inspector
        Utilities.drawFrustumGizmo(BvhRegion.frustum, color);
    }
}