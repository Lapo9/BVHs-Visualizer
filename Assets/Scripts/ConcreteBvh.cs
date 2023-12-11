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

    [SerializeField] private ConcreteNode concreteNodePrefab;

    [Header("Buttons")]
    [SerializeField] private WhatToShow visibility;

    [Header("Global info")]
    [SerializeField] private int maxLevel;
    [SerializeField] private int numberOfLeaves;
    [SerializeField] private int numberOfNodes;
    [SerializeField] private float pah;
    [SerializeField] private float sah;

    public BvhData BvhData { get; private set; }
    public ConcreteNode Root { get { return transform.Find("Root").GetComponent<ConcreteNode>(); } }
    private (float opaque, float light) transparency = (0.7f, 0.15f);

    public WhatToShow Visibility
    {
        get { return visibility; }
        set
        {
            visibility = value;
            switch (value)
            {
                case ConcreteBvh.WhatToShow.ALL:
                    showAllNodes();
                    break;
                case ConcreteBvh.WhatToShow.NOTHING:
                    hideAllNodes();
                    break;
                case ConcreteBvh.WhatToShow.LEAVES:
                    showOnlyLeaves();
                    break;

            }
        }
    }


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
        if (Root != null) Visibility = visibility;
    }

    private void OnDrawGizmos()
    {
        updateGlobalInfo();
    }

    private void createInfluenceArea()
    {
        var influenceArea = ConcreteInfluenceArea.initialize(BvhData.influenceArea);
        influenceArea.transform.parent = transform;
    }

    private void createNode(int id, ConcreteNode parent = null)
    {
        ConcreteNode node = ConcreteNode.initialize(BvhData.findNode(id), parent, concreteNodePrefab);
        if (parent == null) node.transform.parent = transform;

        //create children
        if (!node.Node.isLeaf())
        {
            createNode(node.Node.core.leftChild, node);
            createNode(node.Node.core.rightChild, node);
        }
    }

    /// <summary>
    /// Deletes the BVH and the triangles.
    /// </summary>
    public void delete()
    {
        EditorApplication.delayCall += () => { DestroyImmediate(gameObject); };
    }

    /// <summary>
    /// Finds a node in the BVH with the required id, else throws.
    /// Only the leftmost node is returned.
    /// </summary>
    public ConcreteNode findConcreteNode(int id)
    {
        var found = findConcreteNodeRecursive(Root, id);
        if (found != null) return found;

        throw new KeyNotFoundException("No node with id = " + id + " found");
    }

    /// <summary>
    /// Recursively looks for a node with the required id.
    /// The lookup is performed depth first, left to right.
    /// </summary>
    private ConcreteNode findConcreteNodeRecursive(ConcreteNode node, int id)
    {
        //is this the node?
        if (node.Node.core.id == id) return node;

        //if it is a leaf, return null
        if (node.Node.isLeaf()) return null;

        //look for it in the left subtree
        var foundInLeftChildren = findConcreteNodeRecursive(node.transform.GetChild(0).GetComponent<ConcreteNode>(), id);
        if (foundInLeftChildren != null) return foundInLeftChildren;

        //look for it in the right subtree
        var foundInRightChildren = findConcreteNodeRecursive(node.transform.GetChild(1).GetComponent<ConcreteNode>(), id);
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
    /// Hides all the non-leaf nodesMeshes of the BVH.
    /// </summary>
    private void showOnlyLeaves(ConcreteNode parent = null)
    {
        if (parent == null)
        {
            showOnlyLeaves(Root);
            return;
        }

        //set node
        var meshRenderer = parent.gameObject.GetComponent<MeshRenderer>();
        //if not leaf, hide it
        if (!parent.Node.isLeaf()) meshRenderer.enabled = false;
        //make leaves more opaque
        else
        {
            meshRenderer.enabled = true;
            meshRenderer.sharedMaterial.color = new Color(meshRenderer.sharedMaterial.color.r, meshRenderer.sharedMaterial.color.g, meshRenderer.sharedMaterial.color.b, transparency.opaque);
        }

        //recurse on children
        foreach (Transform c in parent.transform) showOnlyLeaves(c.GetComponent<ConcreteNode>());
    }

    /// <summary>
    /// Shows all the nodesMeshes of the BVH.
    /// </summary>
    private void showAllNodes(ConcreteNode parent = null)
    {
        if (parent == null)
        {
            showAllNodes(Root);
            return;
        }

        //show node
        var meshRenderer = parent.gameObject.GetComponent<MeshRenderer>();
        meshRenderer.sharedMaterial.color = new Color(meshRenderer.sharedMaterial.color.r, meshRenderer.sharedMaterial.color.g, meshRenderer.sharedMaterial.color.b, transparency.light);
        meshRenderer.enabled = true;

        //recurse on children
        foreach (Transform c in parent.transform) showAllNodes(c.GetComponent<ConcreteNode>());
    }

    /// <summary>
    /// Hides all the nodesMeshes of the BVH.
    /// </summary>
    private void hideAllNodes(ConcreteNode parent = null)
    {
        if (parent == null)
        {
            hideAllNodes(Root);
            return;
        }

        //make node invisible
        var meshRenderer = parent.gameObject.GetComponent<MeshRenderer>();
        meshRenderer.enabled = false;

        //recurse on children
        foreach (Transform c in parent.transform) hideAllNodes(c.GetComponent<ConcreteNode>());
    }
}
