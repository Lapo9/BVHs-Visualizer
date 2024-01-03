using System;
using UnityEngine;

public class ConcretePointInfluenceArea : ConcreteInfluenceArea
{
    public static new ConcretePointInfluenceArea initialize(InfluenceArea influenceArea)
    {
        var gameObject = new GameObject("InfluenceArea");
        var area = gameObject.AddComponent<ConcretePointInfluenceArea>();

        //set position
        area.transform.position = influenceArea.pointInfluenceArea.pov.Position;
        area.transform.forward = influenceArea.pointInfluenceArea.pov.Direction;

        //set camera
        var cam = gameObject.AddComponent<Camera>();
        cam.orthographic = false;
        cam.farClipPlane = influenceArea.pointInfluenceArea.bvhRegion.frustum.parameters.f;
        cam.nearClipPlane = influenceArea.pointInfluenceArea.bvhRegion.frustum.parameters.n;
        cam.aspect = influenceArea.pointInfluenceArea.bvhRegion.frustum.parameters.ratio;
        cam.fieldOfView = Mathf.Rad2Deg * influenceArea.pointInfluenceArea.bvhRegion.frustum.parameters.fovX;

        //create region
        var region = ConcreteBvhRegion.initialize(influenceArea.pointInfluenceArea.bvhRegion);
        region.transform.parent = gameObject.transform;

        return area;
    }

    /// <summary>
    /// Generates random rays.
    /// </summary>
    protected override void generateRays()
    {
        throw new NotImplementedException(); //TODO
    }
}