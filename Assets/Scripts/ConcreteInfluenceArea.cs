using System.Collections.Generic;
using System.IO;
using UnityEngine;

public abstract class ConcreteInfluenceArea : MonoBehaviour
{
    #region Related classes
    public class Ray
    {
        public Vector3 Origin { get; set; }

        Vector3 direction;
        public Vector3 Direction
        {
            get { return direction; }
            set { direction = value.normalized; }
        }

        public Ray(Vector3 origin, Vector3 direction)
        {
            this.Origin = origin;
            this.direction = direction.normalized;
        }
    }

    public class IntersectionInfo
    {
        public int Count { get; set; }
        public float Cost { get; set; }
        public int Rays { get; private set; }
        public float CountPerRay { get { return (float)Count / Rays; } }
        public float CostPerRay { get { return Cost / Rays; } }

        public IntersectionInfo()
        {
            Count = 0;
            Cost = 0;
            Rays = 1;
        }
        public IntersectionInfo(int count, float cost, int rays = 1)
        {
            Rays = rays;
            Count = count;
            Cost = cost;
        }

        public static IntersectionInfo operator +(IntersectionInfo lhs, IntersectionInfo rhs)
        {
            return new IntersectionInfo(lhs.Count + rhs.Count, lhs.Cost + rhs.Cost, lhs.Rays + rhs.Rays);
        }

        public static IntersectionInfo operator ++(IntersectionInfo info)
        {
            info.Rays++;
            return info;
        }
    }
    #endregion


    [Range(1, 500)] [SerializeField] protected int raysAmount = 10; //how many rays to cast
    [Range(1f, 100f)] [SerializeField] protected float length = 100f; //length of each ray
    [Range(0, 20)][SerializeField] protected int seed = 0; //randomness
    [SerializeField] private Color rayColor = Color.white; //color of the rays (and gizmo)

    [Header("Buttons")]
    [SerializeField] private bool generateRaysButton = false;
    [SerializeField] private bool deleteRaysButton = false;

    [Header("Info")]
    protected IntersectionInfo intersectionInfo; //how many intersections
    [SerializeField] protected int intersectionsCountDisplay;
    [SerializeField] protected float intersectionsCountPerRayDisplay;
    [SerializeField] protected float intersectionsCostDisplay;
    [SerializeField] protected float intersectionsCostPerRayDisplay;

    protected (float internalNode, float leafNode) costs;
    protected ConcreteNode root;
    protected Ray[] rays; //where to store the rays

    public InfluenceArea InfluenceArea { get; protected set; }

    /// <summary>
    /// Initializes the object.
    /// </summary>
    public static ConcreteInfluenceArea initialize(InfluenceArea influenceArea)
    {
        //create the right influence area based on type
        ConcreteInfluenceArea area;
        if(influenceArea.type == "plane")
        {
            area = ConcretePlaneInfluenceArea.initialize(influenceArea);
        }
        else
        {
            throw new InvalidDataException(influenceArea.type + " is not a known type of inflluence area");
        }

        //set fields
        area.InfluenceArea = influenceArea;
        area.intersectionInfo = new IntersectionInfo();

        //set camera
        area.GetComponent<Camera>().clearFlags = CameraClearFlags.Nothing; //black background

        return area;
    }

    void OnValidate()
    {
        if (generateRaysButton)
        {
            generateRaysButton = false;
            generateRays();
        }

        if (deleteRaysButton)
        {
            deleteRaysButton = false;
            deleteRays();
        }
    }

    void OnDrawGizmos()
    {
        //update shown info
        if (intersectionInfo != null)
        {
            intersectionsCountDisplay = intersectionInfo.Count;
            intersectionsCountPerRayDisplay = intersectionInfo.CountPerRay;
            intersectionsCostDisplay = intersectionInfo.Cost;
            intersectionsCostPerRayDisplay = intersectionInfo.CostPerRay;
        }

        Random.InitState(0); //used to make the color of the gizmos for the projected AABBs stable
        drawGizmo(rayColor); //draw the gizmo for this object

        //draw the gizmos for the rays
        if (rays == null) return;
        Gizmos.color = rayColor;
        foreach (Ray r in rays) Gizmos.DrawLine(r.Origin, r.Origin + r.Direction * length);
    }


    /// <summary>
    /// Draws a visual hint for this caster.
    /// Each type of caster will have a different visual hint.
    /// </summary>
    protected abstract void drawGizmo(Color color);

    /// <summary>
    /// Generates the rays.
    /// Each type of caster will have a different visual hint.
    /// </summary>
    protected abstract void generateRays();

    /// <summary>
    /// Erases all the rays
    /// </summary>
    protected void deleteRays()
    {
        rays = new Ray[0];
    }

    /// <summary>
    /// Computes the stats of the interaction between the rays and the BVH.
    /// </summary>
    protected IntersectionInfo collectStats()
    {
        IntersectionInfo info = new IntersectionInfo();
        float cost = 0f;
        foreach (var ray in rays)
        {
            info += countIntersections(ray);
        }
        return info;
    }

    /// <summary>
    /// Counts how many intersections there are between a ray and the BVH.
    /// </summary>
    private IntersectionInfo countIntersections(Ray ray)
    {
        IntersectionInfo info = new IntersectionInfo();
        Queue<ConcreteNode> nodes = new Queue<ConcreteNode>(); //intersected nodes still to evaluate
        nodes.Enqueue(root);

        while (nodes.Count > 0)
        {
            ConcreteNode node = nodes.Dequeue(); //get the node
            BoxCollider nodeBox = node.GetComponent<BoxCollider>(); //get the BoxCollider from the node
            bool collision = nodeBox.Raycast(new UnityEngine.Ray(ray.Origin, ray.Direction), out _, length); //intersect ray with BoxCollider
            if (collision)
            {
                info.Count++;
                if (!node.Node.isLeaf())
                {
                    info.Cost += costs.internalNode * 2f;
                    //we must check children
                    nodes.Enqueue(node.transform.GetChild(0).GetComponent<ConcreteNode>());
                    nodes.Enqueue(node.transform.GetChild(1).GetComponent<ConcreteNode>());
                }
                else
                {
                    info.Cost += costs.leafNode * node.Node.core.triangles.Count;
                }
            }
        }
        if (info.Count <= 0) return new IntersectionInfo(0, 0, 0); //if the ray doesn't intersect at least the root, we won't consider it in the final stats
        return info;
    }
}
