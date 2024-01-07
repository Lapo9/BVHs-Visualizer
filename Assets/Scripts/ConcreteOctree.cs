using UnityEditor;
using UnityEngine;

public class ConcreteOctree : MonoBehaviour
{
    public enum WhatToShow
    {
        NOTHING, LEAVES, WIREFRAME, WIREFRAME_AND_LEAVES
    }

    [SerializeField] private ConcreteOctreeNode concreteOctreeNodePrefab;
    [SerializeField] private WhatToShow visibility;

    private TopLevel TopLevel { get; set; }
    public ConcreteOctreeNode Root { get { return transform.Find("Octree Root").GetComponent<ConcreteOctreeNode>(); } }

    public static ConcreteOctree initialize(TopLevel topLevel, ConcreteOctree prefab)
    {
        ConcreteOctree octree = Instantiate(prefab);
        octree.TopLevel = topLevel;
        octree.name = prefab.name;

        //create the octree to show
        if (topLevel.octree.Count != 0)
        {
            octree.createNode(topLevel.octreeRoot().id);
            octree.showMode(WhatToShow.LEAVES);
        }

        return octree;
    }

    /// <summary>
    /// Given the id of an octree node, it creates it (and all its children) in Unity.
    /// </summary>
    private void createNode(long id, ConcreteOctreeNode parent = null)
    {
        ConcreteOctreeNode node = ConcreteOctreeNode.initialize(TopLevel.findOctreeNode(id), parent, concreteOctreeNodePrefab);
        if (parent == null) node.transform.parent = transform;

        //create children
        if (!node.Node.isLeaf)
        {
            foreach (var child in node.Node.children)
            {
                createNode(child, node);
            }
        }
    }

    /// <summary>
    /// Deletes the octree.
    /// </summary>
    public void delete()
    {
        EditorApplication.delayCall += () => { DestroyImmediate(gameObject); };
    }

    /// <summary>
    /// Sets what to show and what to hide (i.e. objects and gizmos).
    /// </summary>
    public void showMode(WhatToShow visibility, ConcreteOctreeNode parent = null)
    {
        this.visibility = visibility;
        if (parent == null)
        {
            showMode(visibility, Root);
            return;
        }

        parent.showMode(visibility);

        //recurse on children
        foreach (Transform c in parent.transform) showMode(visibility, c.GetComponent<ConcreteOctreeNode>());
    }


    private void OnValidate()
    {
        if (Root != null) showMode(visibility);
    }
}
