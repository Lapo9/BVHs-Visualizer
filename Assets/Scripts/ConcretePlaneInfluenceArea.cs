using UnityEngine;

public class ConcretePlaneInfluenceArea : ConcreteInfluenceArea
{
    public static new ConcretePlaneInfluenceArea initialize(InfluenceArea influenceArea)
    {
        var gameObject = new GameObject("InfluenceArea");
        var area = gameObject.AddComponent<ConcretePlaneInfluenceArea>();

        //set position
        area.transform.position = influenceArea.planeInfluenceArea.plane.Point;
        area.transform.forward = influenceArea.planeInfluenceArea.plane.Normal;

        //set camera
        var cam = gameObject.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 2f;
        cam.targetDisplay = 0;

        //create region
        var region = ConcreteBvhRegion.initialize(influenceArea.planeInfluenceArea.bvhRegion);
        region.transform.parent = gameObject.transform;

        return area;
    }

    /// <summary>
    /// Generates random rays with the same directionas the PoV and origin inside the specified rectangle (in world space).
    /// </summary>
    protected override void generateRays()
    {
        Random.InitState(seed); //initialize randomness
        rays = new Ray[raysAmount];
        var extent = InfluenceArea.planeInfluenceArea.Size;

        for (int i = 0; i < raysAmount; i++)
        {
            Vector3 rand = new Vector3(Random.Range(-extent.x, extent.x), Random.Range(-extent.y, extent.y)); //get a random position on the plane in local space
            Vector3 pos = (Vector3)(transform.localToWorldMatrix * rand) + InfluenceArea.planeInfluenceArea.plane.Point; //transform the position of the ray in world space
            rays[i] = new Ray(pos, InfluenceArea.planeInfluenceArea.plane.Normal);
        }

        intersectionInfo = collectStats();
    }
}
