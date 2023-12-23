using UnityEngine;

[ExecuteAlways]
public class ConcreteBvhNode : MonoBehaviour
{
    [SerializeField] int id;
    [SerializeField] int leftChild;
    [SerializeField] int rightChild;
    [SerializeField] int[] triangles;
    [Header("AABB")]
    [SerializeField] Vector3 max;
    [SerializeField] Vector3 min;

    private (float opaque, float light) transparency = (0.7f, 0.15f);

    public ConcreteBvh Bvh
    {
        get
        {
            Transform current = transform;
            do
            {
                current = current.parent;
            } while (current.gameObject.GetComponent<ConcreteBvh>() == null);
            return current.GetComponent<ConcreteBvh>();
        }
    }

    public BvhNode Node { get; private set; }


    void OnDrawGizmos()
    {
        id = Node.core.id;
        leftChild = Node.core.leftChild;
        rightChild = Node.core.rightChild;
        triangles = Node.core.triangles.ToArray();
        max = Node.core.aabb.Max;
        min = Node.core.aabb.Min;
    }

    public static ConcreteBvhNode initialize(BvhNode node, ConcreteBvhNode parent, ConcreteBvhNode prefab)
    {
        //create the game object and position it
        var aabb = node.core.aabb;
        ConcreteBvhNode cube = Instantiate(prefab);
        cube.transform.position = (aabb.Max + aabb.Min) / 2.0f;
        cube.transform.localScale = aabb.Max - aabb.Min;

        //give name
        cube.name = "Node";
        if (parent == null) cube.name = "Root";
        else if (node.isLeaf()) cube.name = "Leaf";

        //set random color
        var nodeMaterial = new Material(cube.GetComponent<MeshRenderer>().sharedMaterial);
        nodeMaterial.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 0.1f);
        cube.GetComponent<MeshRenderer>().sharedMaterial = nodeMaterial;

        //set the data
        cube.Node = node;
        if (parent != null) cube.transform.parent = parent.transform;
        return cube;
    }

    public void showMode(ConcreteBvh.WhatToShow visibility)
    {
        switch(visibility)
        {
            case ConcreteBvh.WhatToShow.ALL:
                var meshRenderer = GetComponent<MeshRenderer>();
                meshRenderer.sharedMaterial.color = new Color(meshRenderer.sharedMaterial.color.r, meshRenderer.sharedMaterial.color.g, meshRenderer.sharedMaterial.color.b, transparency.light);
                meshRenderer.enabled = true;
                break;
            case ConcreteBvh.WhatToShow.NOTHING:
                //make node invisible
                meshRenderer = GetComponent<MeshRenderer>();
                meshRenderer.enabled = false;
                break;
            case ConcreteBvh.WhatToShow.LEAVES:
                meshRenderer = GetComponent<MeshRenderer>();
                //if not leaf, hide it
                if (!Node.isLeaf()) meshRenderer.enabled = false;
                //make leaves more opaque
                else
                {
                    meshRenderer.enabled = true;
                    meshRenderer.sharedMaterial.color = new Color(meshRenderer.sharedMaterial.color.r, meshRenderer.sharedMaterial.color.g, meshRenderer.sharedMaterial.color.b, transparency.opaque);
                }
                break;
        }
    }
}
