using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ConcreteBvhRegion : MonoBehaviour
{
    public BvhRegion BvhRegion { get; protected set; }

    /// <summary>
    /// Initializes the object.
    /// </summary>
    public static ConcreteBvhRegion initialize(BvhRegion bvhRegion)
    {
        //create the right influence area based on type
        ConcreteBvhRegion region;
        if (bvhRegion.type == "obb")
        {
            //TODO region = ConcreteObbBvhRegion.initialize(bvhRegion);
        }
        else
        {
            throw new InvalidDataException(bvhRegion.type + " is not a known type of BVH region");
        }

        //set fields
        region.BvhRegion = bvhRegion;

        return region;
    }
}
