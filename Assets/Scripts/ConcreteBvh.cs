using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class ConcreteBvh : MonoBehaviour
{
    public enum WhatToShow
    {
        ALL, NOTHING, LEAVES
    }

    [SerializeField] private ConcreteBvhNode concreteNodePrefab;

    [Header("Buttons")]
    [SerializeField] private WhatToShow visibility;

    [Header("Global info")]
    [SerializeField] private int maxLevel;
    [SerializeField] private int numberOfLeaves;
    [SerializeField] private int numberOfNodes;
    [SerializeField] private float pah;
    [SerializeField] private float sah;

    public BvhData BvhData { get; private set; }
    public ConcreteBvhNode Root { get { return transform.Find("Root").GetComponent<ConcreteBvhNode>(); } }
    public WhatToShow Visibility { get { return visibility; } private set { visibility = value; } }

    public static ConcreteBvh initialize(BvhData bvhData, ConcreteBvh prefab)
    {
        ConcreteBvh bvh = Instantiate(prefab);
        bvh.BvhData = bvhData;
        bvh.name = prefab.name;

        //create the BVH to show
        bvh.createNode(bvhData.root().core.id);
        bvh.createInfluenceArea();

        return bvh;
    }

    private void OnValidate()
    {
        if (Root != null) showMode(visibility);
    }

    private void OnDrawGizmos()
    {
        updateGlobalInfo();
    }

    /// <summary>
    /// Initializes the influence area for this BVH.
    /// </summary>
    private void createInfluenceArea()
    {
        var influenceArea = ConcreteInfluenceArea.initialize(BvhData.influenceArea);
        influenceArea.transform.parent = transform;
    }

    /// <summary>
    /// Given the id of the data of a node, creates it in Unity (an object).
    /// </summary>
    private void createNode(int id, ConcreteBvhNode parent = null)
    {
        ConcreteBvhNode node = ConcreteBvhNode.initialize(BvhData.findNode(id), parent, concreteNodePrefab);
        if (parent == null) node.transform.parent = transform;

        //create children
        if (!node.Node.isLeaf())
        {
            createNode(node.Node.core.leftChild, node);
            createNode(node.Node.core.rightChild, node);
        }
    }

    /// <summary>
    /// Deletes the BVH.
    /// </summary>
    public void delete()
    {
        EditorApplication.delayCall += () => { DestroyImmediate(gameObject); };
    }

    /// <summary>
    /// Finds a node in the BVH with the required id, else throws.
    /// Only the leftmost node is returned.
    /// </summary>
    public ConcreteBvhNode findConcreteNode(int id)
    {
        var found = findConcreteNodeRecursive(Root, id);
        if (found != null) return found;

        throw new KeyNotFoundException("No node with id = " + id + " found");
    }

    /// <summary>
    /// Recursively looks for a node with the required id.
    /// The lookup is performed depth first, left to right.
    /// </summary>
    private ConcreteBvhNode findConcreteNodeRecursive(ConcreteBvhNode node, int id)
    {
        //is this the node?
        if (node.Node.core.id == id) return node;

        //if it is a leaf, return null
        if (node.Node.isLeaf()) return null;

        //look for it in the left subtree
        var foundInLeftChildren = findConcreteNodeRecursive(node.transform.GetChild(0).GetComponent<ConcreteBvhNode>(), id);
        if (foundInLeftChildren != null) return foundInLeftChildren;

        //look for it in the right subtree
        var foundInRightChildren = findConcreteNodeRecursive(node.transform.GetChild(1).GetComponent<ConcreteBvhNode>(), id);
        if (foundInRightChildren != null) return foundInRightChildren;

        //we found nothing
        return null;
    }

    /// <summary>
    /// Updates the variables used to show info in the inspector.
    /// </summary>
    private void updateGlobalInfo()
    {
        maxLevel = BvhData.globalInfo.maxLevel;
        numberOfLeaves = BvhData.globalInfo.numberOfLeaves;
        numberOfNodes = BvhData.globalInfo.numberOfNodes;
        pah = BvhData.globalInfo.pahCost;
        sah = BvhData.globalInfo.sahCost;
    }

    /// <summary>
    /// Sets what to show in Unity (i.e. nodes and gizmos) and what to hide.
    /// </summary>
    public void showMode(WhatToShow visibility, ConcreteBvhNode parent = null)
    {
        Visibility = visibility;
        if (parent == null)
        {
            showMode(visibility, Root);
            return;
        }

        //set the node visibility
        parent.showMode(visibility);

        //recurse on children
        foreach (Transform c in parent.transform) showMode(visibility, c.GetComponent<ConcreteBvhNode>());
    }
}
