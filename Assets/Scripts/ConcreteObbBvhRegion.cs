using UnityEngine;

public class ConcreteObbBvhRegion : ConcreteBvhRegion
{
    [SerializeField] Vector3[] vertices; //to show the vertices in the inspector

    /// <summary>
    /// Initializes the object.
    /// </summary>
    public static new ConcreteObbBvhRegion initialize(BvhRegion bvhRegion)
    {
        var gameObject = new GameObject("BvhRegion");
        var region = gameObject.AddComponent<ConcreteObbBvhRegion>();

        //set position
        region.transform.position = bvhRegion.aabbForObb.obb.Center;
        region.transform.forward = bvhRegion.aabbForObb.obb.Forward;

        return region;
    }

    /// <summary>
    /// Draws the gizmo for an OBB region.
    /// </summary>
    protected override void drawGizmo(Color color)
    {
        vertices = Utilities.obbToPoints(BvhRegion.aabbForObb.obb); //update vertices in inspector
        Utilities.drawObbGizmo(BvhRegion.aabbForObb.obb, color);
    }
}
