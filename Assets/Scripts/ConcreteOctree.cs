using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ConcreteOctree : MonoBehaviour
{
    public enum WhatToShow
    {
        NOTHING, LEAVES, WIREFRAME, WIREFRAME_AND_LEAVES
    }

    [SerializeField] private ConcreteOctreeNode concreteOctreeNodePrefab;
    [SerializeField] private WhatToShow visibility;

    public TopLevel TopLevel { get; private set; }
    public ConcreteOctreeNode Root { get { return transform.Find("Octree Root").GetComponent<ConcreteOctreeNode>(); } }

    public static ConcreteOctree initialize(TopLevel topLevel, ConcreteOctree prefab)
    {
        ConcreteOctree octree = Instantiate(prefab);
        octree.TopLevel = topLevel;
        octree.name = prefab.name;

        //create the octree to show
        octree.createNode(topLevel.octreeRoot().id);

        return octree;
    }

    private void createNode(int id, ConcreteOctreeNode parent = null)
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

    public void showMode(WhatToShow visibility, ConcreteOctreeNode parent = null)
    {
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
