using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConcreteObbBvhRegion : ConcreteBvhRegion
{
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
    }
}
