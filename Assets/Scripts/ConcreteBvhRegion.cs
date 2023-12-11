using System.IO;
using UnityEngine;

public abstract class ConcreteBvhRegion : MonoBehaviour
{
    [SerializeField] Color color = Color.white;

    public BvhRegion BvhRegion { get; protected set; }


    /// <summary>
    /// Assumes the influence area is always the parent.
    /// </summary>
    public ConcreteBvh Bvh
    {
        get
        {
            ConcreteBvh bvh = transform.parent.GetComponent<ConcreteInfluenceArea>().Bvh;
            Debug.Assert(bvh != null, "The position in the hierarchy of this ConcreteBvhRegion is wrong. It should be a child of its ConcreteInfluenceArea.");
            return bvh;
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
        drawGizmo(color);
    }
}
