using System.IO;
using UnityEngine;
using UnityEditor;
using static Utilities;
using System.Linq;
using System.Collections.Generic;
using Unity.VisualScripting;
using System;

[ExecuteAlways]
public class Deserializer : MonoBehaviour
{
    [SerializeField] private string file;
    [SerializeField] private ConcreteBvh concreteBvhPrefab;
    [SerializeField] private ConcreteTriangle concreteTrianglePrefab;

    [Header("Buttons")]
    [SerializeField] private bool deserializeButton;

    private TopLevel topLevel;
    private List<ConcreteBvh> Bvhs
    {
        get
        {
            List<ConcreteBvh> res = new List<ConcreteBvh>();
            foreach (Transform bvh in transform)
            {
                if (bvh.GetComponent<ConcreteBvh>() != null) res.Add(bvh.GetComponent<ConcreteBvh>());
            }
            return res;
        }
    }
    private List<ConcreteTriangle> Triangles
    {
        get
        {
            List<ConcreteTriangle> triangles = new List<ConcreteTriangle>();
            foreach (Transform t in GameObject.Find("Triangles").transform)
            {
                triangles.Add(t.GetComponent<ConcreteTriangle>());
            }
            return triangles;
        }
    }
    private Transform lastSelected;
    private (float opaque, float light) transparency = (0.7f, 0.15f);

    private void OnValidate()
    {
        if (deserializeButton)
        {
            deserializeButton = false;
            deserialize();
        }
    }

    void OnDrawGizmos()
    {
        Transform selected = Selection.activeTransform; //object selected in the editor

        //returns true if the selected object is part of a BVH (e.g. ConcreteBvh, ConcreteInfluenceArea, ...)
        Func<Transform, (bool isPart, ConcreteBvh bvh)> partOfBvh = o =>
        {
            if (o == null) return (false, null);
            if (o.GetComponent<ConcreteBvh>() != null) return (true, o.GetComponent<ConcreteBvh>());
            if (o.GetComponent<ConcreteNode>() != null) return (true, o.GetComponent<ConcreteNode>().Bvh);
            if (o.GetComponent<ConcreteInfluenceArea>() != null) return (true, o.GetComponent<ConcreteInfluenceArea>().Bvh);
            if (o.GetComponent<ConcreteBvhRegion>() != null) return (true, o.GetComponent<ConcreteBvhRegion>().Bvh);
            return (false, null);
        };

        //if the selected object is a BVH node...
        if (selected?.gameObject.GetComponent<ConcreteNode>() != null)
        {
            bvhSelected(selected.gameObject.GetComponent<ConcreteNode>().Bvh);
            nodeSelected(selected.GetComponent<ConcreteNode>());
        }

        //if the selected object is a BVH...
        else if (partOfBvh(selected) is (true, _) result) bvhSelected(result.bvh);

        //if the selected object is not a node or a bvh, but the last one was
        else if (partOfBvh(lastSelected).isPart) showRestore();

        lastSelected = selected;
    }

    private void deserialize()
    {
        delete(); //delete old BVH

        string json = File.ReadAllText("Assets/Data/" + file);
        topLevel = JsonUtility.FromJson<TopLevel>(json);
        Debug.Log("Deserialized");

        //create the BVH to show
        createTriangles();
        foreach (var bvh in topLevel.bvhs)
        {
            var concreteBvh = ConcreteBvh.initialize(bvh, concreteBvhPrefab);
            concreteBvh.transform.parent = transform;
        }
    }

    private void createTriangles()
    {
        var parent = GameObject.Find("Triangles").transform;
        foreach (Triangle t in topLevel.triangles)
        {
            ConcreteTriangle.initialize(t, parent, concreteTrianglePrefab);
        }
    }

    /// <summary>
    /// Deletes the BVH and the triangles.
    /// </summary>
    private void delete()
    {
        //delete triangles
        var triangles = GameObject.Find("Triangles").transform;
        foreach (Transform t in triangles)
        {
            EditorApplication.delayCall += () => { DestroyImmediate(t.gameObject); };
        }

        //delete BVHs
        var bvhs = Bvhs;
        foreach (var bvh in bvhs)
        {
            bvh.delete();
        }
    }

    /// <summary>
    /// Finds a triangle in the Triangles game object with the required id, else throws.
    /// Only the first occurrence is returned.
    /// </summary>
    private ConcreteTriangle findConcreteTriangle(int id)
    {
        var triangles = GameObject.Find("Triangles").transform;
        foreach (Transform t in triangles)
        {
            var triangle = t.GetComponent<ConcreteTriangle>();
            if (triangle.Triangle.id == id) return triangle;
        }

        throw new KeyNotFoundException("No triangle with id = " + id + " found");
    }

    /// <summary>
    /// Finds a node in the BVH with the required id, else throws.
    /// Only the leftmost node is returned.
    /// </summary>
    private ConcreteNode findConcreteNode(int id)
    {
        foreach (var bvh in Bvhs)
        {
            var found = bvh.findConcreteNode(id);
            if (found != null) return found;
        }

        throw new KeyNotFoundException("No node with id = " + id + " found");
    }

    /// <summary>
    /// Returns a string containing info about the node.
    /// </summary>
    /// <returns>
    /// - Surface area
    /// - Surface area heuristic
    /// - Projected area
    /// - Projected area heuristic
    /// - Number of primitives inside
    /// </returns>
    public static string logNodeInfo(Node node, string nodeName, string cssColor)
    {
        string log = (nodeName.Bold() +
                        "\t\tSA = " + node.metrics.sa.ToString("0.00") +
                        "\tSAH = " + node.metrics.sah.ToString("0.00") +
                        "\tPA = " + node.metrics.pa.ToString("0.00") +
                        "\tPAH = " + node.metrics.pah.ToString("0.00") +
                        "\tPrimitives = " + node.core.triangles.Count) + "\n";

        return log.Color(cssColor);
    }

    /// <summary>
    /// Hides all the nodes not in the selected array.
    /// </summary>
    private void showOnlySelected(ConcreteNode[] selected)
    {
        //hide all
        foreach (var bvh in Bvhs)
        {
            bvh.Visibility = ConcreteBvh.WhatToShow.NOTHING;
        }

        //make visible only selected nodes
        foreach (var node in selected)
        {
            var meshRenderer = node.gameObject.GetComponent<MeshRenderer>();
            meshRenderer.enabled = true;
            meshRenderer.sharedMaterial.color = new Color(meshRenderer.sharedMaterial.color.r, meshRenderer.sharedMaterial.color.g, meshRenderer.sharedMaterial.color.b, selected[0] == node ? 0f : transparency.opaque);
        }
    }

    /// <summary>
    /// Restore the default, basically shows everything.
    /// </summary>
    private void showRestore()
    {
        //show all
        foreach (var bvh in Bvhs)
        {
            bvh.Visibility = ConcreteBvh.WhatToShow.ALL;
        }

        foreach (var t in Triangles)
        {
            t.GetComponent<MeshRenderer>().enabled = true;
        }
    }

    /// <summary>
    /// Routine to execute when the selected transform is a ConcreteNode.
    /// </summary>
    private void nodeSelected(ConcreteNode selectedNode)
    {
        Transform selected = selectedNode.transform;

        //if it is not a leaf, retrieve children and draw them too
        if (!selectedNode.Node.isLeaf())
        {
            ConcreteNode leftChild = selected.GetChild(0).GetComponent<ConcreteNode>();
            ConcreteNode rightChild = selected.GetChild(1).GetComponent<ConcreteNode>();

            drawAabbGizmo(leftChild.Node.core.aabb, Color.red);
            drawAabbGizmo(rightChild.Node.core.aabb, Color.blue);

            //highlight the internal triangles too
            foreach (int i in leftChild.Node.core.triangles) drawTriangleGizmo(topLevel.findTriangle(i), Color.magenta);
            foreach (int i in rightChild.Node.core.triangles) drawTriangleGizmo(topLevel.findTriangle(i), Color.cyan);

            //this way we print info about the selected node and children only once
            if (selected != lastSelected)
            {
                Debug.Log(logNodeInfo(selectedNode.Node, "Parent", "lime"));
                Debug.Log(logNodeInfo(leftChild.Node, "Left child", "red"));
                Debug.Log(logNodeInfo(rightChild.Node, "Right child", "blue"));
            }
        }

        drawAabbGizmo(selectedNode.Node.core.aabb, Color.green); //highlight selected node
        ConcreteNode[] selectedAndChildren = selectedNode.Node.isLeaf() ?
            new ConcreteNode[] { selectedNode } :
            new ConcreteNode[] { selectedNode, selected.GetChild(0).GetComponent<ConcreteNode>(), selected.GetChild(1).GetComponent<ConcreteNode>() };

        showOnlySelected(selectedAndChildren); //hide everything BUT the selected node (and its children)
    }

    /// <summary>
    /// Routine to execute when the selected transform is a ConcreteBvh.
    /// </summary>
    private void bvhSelected(ConcreteBvh selectedBvh)
    {
        foreach (var bvh in Bvhs)
        {
            if (bvh != selectedBvh) bvh.Visibility = ConcreteBvh.WhatToShow.NOTHING;
            else bvh.Visibility = ConcreteBvh.WhatToShow.ALL;
        }

        //show only the triangles of the selected BVH
        foreach (var t in Triangles)
        {
            if (selectedBvh.BvhData.triangles.Where(t1 => t1.id == t.Triangle.id).Count() == 0)
            {
                t.GetComponent<MeshRenderer>().enabled = false;
            }
        }
    }
}