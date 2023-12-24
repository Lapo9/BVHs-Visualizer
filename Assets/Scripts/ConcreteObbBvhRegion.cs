using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConcreteObbBvhRegion : ConcreteBvhRegion
{
    [SerializeField] Vector3[] vertices; //to show the vertices

    /// <summary>
    /// Initializes the object.
    /// </summary>
    public static new ConcreteObbBvhRegion initialize(BvhRegion bvhRegion)
    {
        var gameObject = new GameObject("BvhRegion");
        var region = gameObject.AddComponent<ConcreteObbBvhRegion>();

        //set position
        region.transform.position = bvhRegion.obb.Center;
        region.transform.forward = bvhRegion.obb.Forward;

        return region;
    }

    /// <summary>
    /// Draws the gizmo for an OBB region.
    /// </summary>
    protected override void drawGizmo(Color color)
    {
        Utilities.drawObbGizmo(BvhRegion.obb, color);
        vertices = Utilities.obbToPoints(BvhRegion.obb); //update vertices in inspector
    }
}
