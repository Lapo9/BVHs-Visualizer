using System.IO;
using UnityEngine;
using UnityEditor;
using static Utilities;
using System.Linq;
using System.Collections.Generic;

[ExecuteAlways]
public class Deserializer : MonoBehaviour
{
    [SerializeField] private string file;
    [SerializeField] private ConcreteNode concreteNodePrefab;
    [SerializeField] private ConcreteTriangle concreteTrianglePrefab;

    [Header("Buttons")]
    [SerializeField] private bool deserializeButton;
    [SerializeField] private bool showOnlyLeavesButton;

    [Header("Global info")]
    [SerializeField] private int maxLevel;
    [SerializeField] private int numberOfLeaves;
    [SerializeField] private int numberOfNodes;
    [SerializeField] private float pah;
    [SerializeField] private float sah;

    private BvhData bvhData;
    private ConcreteNode root;
    private Transform lastSelected;
    private (float opaque, float light) transparency = (0.7f, 0.15f);

    private void OnValidate()
    {
        if (deserializeButton)
        {
            deserializeButton = false;
            deserialize();
        }
        if(root != null) showWhatUserWantsToSee();
    }

    void OnDrawGizmos()
    {
        if(root == null) return;

        updateGlobalInfo();

        Transform selected = Selection.activeTransform; //object selected in the editor

        //if the selected object is a BVH node...
        if (selected?.gameObject.GetComponent<ConcreteNode>() != null)
        {
            ConcreteNode selectedNode = selected.GetComponent<ConcreteNode>(); //retrieve the corresponding BVH node

            //if it is not a leaf, retrieve children and draw them too
            if (!selectedNode.Node.isLeaf())
            {
                ConcreteNode leftChild = selected.GetChild(0).GetComponent<ConcreteNode>();
                ConcreteNode rightChild = selected.GetChild(1).GetComponent<ConcreteNode>();

                drawAabbGizmo(leftChild.Node.core.aabb, Color.red);
                drawAabbGizmo(rightChild.Node.core.aabb, Color.blue);

                //highlight the internal triangles too
                foreach (int i in leftChild.Node.core.triangles) drawTriangleGizmo(bvhData.findTriangle(i), Color.magenta);
                foreach (int i in rightChild.Node.core.triangles) drawTriangleGizmo(bvhData.findTriangle(i), Color.cyan);

                //this way we print info about the selected node and children only once
                if (selected != lastSelected)
                {
                    lastSelected = selected;
                    Debug.Log(logNodeInfo(selectedNode.Node, "Parent", "lime"));
                    Debug.Log(logNodeInfo(leftChild.Node, "Left child", "red"));
                    Debug.Log(logNodeInfo(rightChild.Node, "Right child", "blue"));
                }
            }

            drawAabbGizmo(selectedNode.Node.core.aabb, Color.green); //highlight selected node
            ConcreteNode[] selectedAndChildren = selectedNode.Node.isLeaf() ?
                new ConcreteNode[] { selectedNode } :
                new ConcreteNode[] { selectedNode, selected.GetChild(0).GetComponent<ConcreteNode>(), selected.GetChild(1).GetComponent<ConcreteNode>() };
            
            showOnlySelected(root, selectedAndChildren); //hide everything BUT the selected node (and its children)
        }

        //if the selected object is not a node
        else
        {
            showWhatUserWantsToSee();
        }
    }

    private void deserialize()
    {
        delete(); //delete old BVH

        string json = File.ReadAllText("Assets/Data/" + file);
        bvhData = JsonUtility.FromJson<BvhData>(json);
        Debug.Log("Deserialized");

        //create the BVH to show
        createTriangles();
        createNode(bvhData.root().core.id);
        createInfluenceArea();
    }

    private void createTriangles()
    {
        var parent = GameObject.Find("Triangles").transform;
        foreach(Triangle t in bvhData.triangles)
        {
            ConcreteTriangle.initialize(t, parent, concreteTrianglePrefab);
        }
    }

    private void createInfluenceArea()
    {
        ConcreteInfluenceArea.initialize(bvhData.influenceArea);
    }

    private void createNode(int id, ConcreteNode parent = null)
    {
        ConcreteNode node = ConcreteNode.initialize(bvhData.findNode(id), parent, concreteNodePrefab);
        if (parent == null) root = node;

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
    private void delete()
    {
        //delete BVH
        var oldRoot = GameObject.Find("Root");
        if (oldRoot!= null) { 
            EditorApplication.delayCall += () => { DestroyImmediate(oldRoot); };
        }

        //delete triangles
        var triangles = GameObject.Find("Triangles").transform;
        foreach (Transform t in triangles)
        {
            EditorApplication.delayCall += () => { DestroyImmediate(t.gameObject); };
        }

        //delete influence area
        var influenceArea = GameObject.Find("InfluenceArea");
        if (oldRoot != null)
        {
            EditorApplication.delayCall += () => { DestroyImmediate(influenceArea); };
        }
    }

    /// <summary>
    /// Finds a triangle in the Triangles game object with the required id, else throws.
    /// Only the first occurrence is returned.
    /// </summary>
    private ConcreteTriangle findConcreteTriangle(int id)
    {
        var triangles = GameObject.Find("Triangles").transform;
        foreach(Transform t in triangles)
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
        var found = findConcreteNodeRecursive(root, id);
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
        if(node.Node.core.id == id) return node;

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

    public void updateGlobalInfo()
    {
        maxLevel = bvhData.globalInfo.maxLevel;
        numberOfLeaves = bvhData.globalInfo.numberOfLeaves;
        numberOfNodes = bvhData.globalInfo.numberOfNodes;
        pah = bvhData.globalInfo.pahCost;
        sah = bvhData.globalInfo.sahCost;
    }

    /// <summary>
    /// Calls the right functions to show only what the user decided he wants to see.
    /// </summary>
    private void showWhatUserWantsToSee()
    {
        if (showOnlyLeavesButton) showOnlyLeaves(root); 
        else showAllNodes(root);
    }
    
    /// <summary>
    /// Hides all the nodes not in the selected array.
    /// </summary>
    private void showOnlySelected(ConcreteNode parent, ConcreteNode[] selected)
    {
        foreach (Transform c in parent.transform)
        {
            ConcreteNode child = c.GetComponent<ConcreteNode>();

            var meshRenderer = child.gameObject.GetComponent<MeshRenderer>();
            //make it more opaque (if selected)
            if (selected.Contains(child))
            {
                meshRenderer.enabled = true;
                meshRenderer.sharedMaterial.color = new Color(meshRenderer.sharedMaterial.color.r, meshRenderer.sharedMaterial.color.g, meshRenderer.sharedMaterial.color.b, selected[0] == child ? 0f : transparency.opaque);
            }
            //hide it (if NOT selecetd)
            else
            {
                meshRenderer.enabled = false;
            }

            if (!child.GetComponent<ConcreteNode>().Node.isLeaf()) showOnlySelected(child, selected); //recurse
        }
    }

    /// <summary>
    /// Hides all the non-leaf nodesMeshes of the BVH.
    /// </summary>
    private void showOnlyLeaves(ConcreteNode parent)
    {
        foreach (Transform c in parent.transform)
        {
            ConcreteNode child = c.GetComponent<ConcreteNode>();

            var meshRenderer = child.gameObject.GetComponent<MeshRenderer>();
            //if not leaf, hide it
            if (!child.Node.isLeaf())
            {
                meshRenderer.enabled = false;
                showOnlyLeaves(child);
            }
            //make leaves more opaque
            else
            {
                meshRenderer.enabled = true;
                meshRenderer.sharedMaterial.color = new Color(meshRenderer.sharedMaterial.color.r, meshRenderer.sharedMaterial.color.g, meshRenderer.sharedMaterial.color.b, transparency.opaque);
            }
        }
    }

    /// <summary>
    /// Shows all the nodesMeshes of the BVH.
    /// </summary>
    private void showAllNodes(ConcreteNode parent)
    {
        //make all nodes visible and lower opacity
        foreach (Transform c in parent.transform)
        {
            ConcreteNode child = c.GetComponent<ConcreteNode>();
            var meshRenderer = child.gameObject.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial.color = new Color(meshRenderer.sharedMaterial.color.r, meshRenderer.sharedMaterial.color.g, meshRenderer.sharedMaterial.color.b, transparency.light);
            meshRenderer.enabled = true;
            if (!child.Node.isLeaf()) showAllNodes(child);
        }
    }
}