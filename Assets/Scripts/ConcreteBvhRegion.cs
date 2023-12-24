using System.IO;
using UnityEngine;

public abstract class ConcreteBvhRegion : MonoBehaviour
{
    public BvhRegion BvhRegion { get; protected set; }


    /// <summary>
    /// Assumes the influence area is always the parent.
    /// </summary>
    public ConcreteBvh Bvh
    {
        get
        {
            return InfluenceArea.Bvh;
        }
    }
    public ConcreteInfluenceArea InfluenceArea
    {
        get
        {
            ConcreteInfluenceArea influenceArea = transform.parent.GetComponent<ConcreteInfluenceArea>();
            Debug.Assert(influenceArea != null, "The position in the hierarchy of this ConcreteBvhRegion is wrong. It should be a child of its ConcreteInfluenceArea.");
            return influenceArea;
        }
    }

    /// <summary>
    /// Initializes the object.
    /// </summary>
    public static ConcreteBvhRegion initialize(BvhRegion bvhRegion)
    {
        //create the right BVH region based on type
        ConcreteBvhRegion region;
        if (bvhRegion.type == "obb")
        {
            region = ConcreteObbBvhRegion.initialize(bvhRegion);
        }
        else
        {
            throw new InvalidDataException(bvhRegion.type + " is not a known type of BVH region");
        }

        //set fields
        region.BvhRegion = bvhRegion;

        return region;
    }

    /// <summary>
    /// Draws a visual hint for this region.
    /// Each type of region will have a different visual hint.
    /// </summary>
    protected abstract void drawGizmo(Color color);

    void OnDrawGizmos()
    {
        if (Bvh.Visibility == ConcreteBvh.WhatToShow.NOTHING) return; //if the BVH is hidden, don't draw the gizmos
        drawGizmo(InfluenceArea.color); //the region gizmo must have the same color as its influence area
    }
}
