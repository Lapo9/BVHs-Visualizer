using UnityEngine;

[ExecuteAlways]
public class ConcreteNode : MonoBehaviour
{
    [SerializeField] int id;
    [SerializeField] int leftChild;
    [SerializeField] int rightChild;
    [SerializeField] int[] triangles;
    [Header("AABB")]
    [SerializeField] Vector3 max;
    [SerializeField] Vector3 min;

    public Node Node { get; private set; }

    void OnDrawGizmos()
    {
        id = Node.core.id;
        leftChild = Node.core.leftChild;
        rightChild = Node.core.rightChild;
        triangles = Node.core.triangles.ToArray();
        max = Node.core.aabb.Max;
        min = Node.core.aabb.Min;
    }

    public static ConcreteNode initialize(Node node, ConcreteNode parent, ConcreteNode prefab)
    {
        //create the game object and position it
        var aabb = node.core.aabb;
        ConcreteNode cube = Instantiate(prefab);
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
}
