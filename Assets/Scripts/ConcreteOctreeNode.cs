using UnityEngine;
using Color = UnityEngine.Color;

public class ConcreteOctreeNode : MonoBehaviour
{
    [SerializeField] int id;
    [SerializeField] int[] children;
    [SerializeField] int[] bvhs;
    [Header("AABB")]
    [SerializeField] Vector3 max;
    [SerializeField] Vector3 min;

    bool showWireframe = true;

    public OctreeNode Node { get; private set; }

    void OnDrawGizmos()
    {
        //update infos to show
        id = Node.id;
        children = Node.children.ToArray();
        bvhs = Node.bvhs.ToArray();
        max = Node.aabb.Max;
        min = Node.aabb.Min;

        //draw gizmo for the box
        if(showWireframe)
        {
            Utilities.drawAabbGizmo(Node.aabb, Color.white);
        }
    }

    public static ConcreteOctreeNode initialize(OctreeNode node, ConcreteOctreeNode parent, ConcreteOctreeNode prefab)
    {
        //create the game object and position it
        var aabb = node.aabb;
        ConcreteOctreeNode cube = Instantiate(prefab);
        cube.transform.position = (aabb.Max + aabb.Min) / 2.0f;
        cube.transform.localScale = aabb.Max - aabb.Min;

        //give name
        cube.name = "Octree Node";
        if (parent == null) cube.name = "Octree Root";
        else if (node.isLeaf) cube.name = "Octree Leaf";

        //set color (if leaf with at least one BVH, else it is invisible)
        if (node.isLeaf && node.bvhs.Count != 0)
        {
            Vector4 color = Vector3.zero;
            foreach (var bvh in node.bvhs)
            {
                Vector4 c4 = colors[Mathf.Abs(bvh.GetHashCode()) % colors.Length]; //nodes containing the same BVHs must have the same color
                color += c4;
            }
            color /= (float)node.bvhs.Count;

            var nodeMaterial = new Material(cube.GetComponent<MeshRenderer>().sharedMaterial);
            nodeMaterial.color = new Color(color.x, color.y, color.z);
            cube.GetComponent<MeshRenderer>().sharedMaterial = nodeMaterial;
        }
        else
        {
            cube.GetComponent<MeshRenderer>().enabled = false;
        }

        //set the data
        cube.Node = node;
        if (parent != null) cube.transform.parent = parent.transform;
        return cube;
    }

    /// <summary>
    /// Sets what to show of this node (i.e. cube and gizmos).
    /// </summary>
    public void showMode(ConcreteOctree.WhatToShow visibility)
    {
        switch (visibility)
        {
            case ConcreteOctree.WhatToShow.NOTHING:
                showWireframe = false;
                GetComponent<MeshRenderer>().enabled = false;
                break;
            case ConcreteOctree.WhatToShow.LEAVES:
                showWireframe = false;
                GetComponent<MeshRenderer>().enabled = Node.isLeaf && Node.bvhs.Count != 0;
                break;
            case ConcreteOctree.WhatToShow.WIREFRAME:
                showWireframe = true;
                GetComponent<MeshRenderer>().enabled = false;
                break;
            case ConcreteOctree.WhatToShow.WIREFRAME_AND_LEAVES:
                showWireframe = true;
                GetComponent<MeshRenderer>().enabled = Node.isLeaf && Node.bvhs.Count != 0;
                break;
        }
    }


    //an array containing possible colors for the nodes of the octree.
    private static Color[] colors =
    {
        new Color(0, 0, 0),
        new Color(1, 0, 0),
        new Color(0, 1, 0),
        new Color(0, 0, 1),
        new Color(1, 1, 0),
        new Color(1, 0, 1),
        new Color(0, 1, 1),
        new Color(1, 1, 1),
        new Color(0.5f, 0, 1),
        new Color(1, 0.5f, 0),
        new Color(0, 0.5f, 0),
        new Color(0.5f, 0.25f, 0),
        new Color(0.5f, 0, 0.5f)
    };
}
