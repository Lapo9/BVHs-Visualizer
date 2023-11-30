using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[Serializable]
public struct BvhData
{
    public GlobalInfo globalInfo;
    public List<Triangle> triangles;
    public List<Node> nodes;


    public Node findNode(int id)
    {
        return nodes.First(n => n.core.id == id);
    }

    public Triangle findTriangle(int id)
    {
        return triangles.First(t => t.id == id);
    }

    public Node root()
    {
        return nodes[0];
    }
}

[Serializable]
public struct GlobalInfo
{
    public int maxLevel;
    public int numberOfLeaves;
    public int numberOfNodes;
    public float pahCost;
    public float sahCost;
}

[Serializable]
public struct Triangle
{
    public int id;
    public List<float> v1;
    public List<float> v2;
    public List<float> v3;

    public Vector3 V1 { get { return new Vector3(v1[0], v1[1], v1[2]); } }
    public Vector3 V2 { get { return new Vector3(v2[0], v2[1], v2[2]); } }
    public Vector3 V3 { get { return new Vector3(v3[0], v3[1], v3[2]); } }
}

[Serializable]
public struct Node
{
    public Core core;
    public Metrics metrics;

    public bool isLeaf()
    {
        return core.leftChild == 0 && core.rightChild == 0;
    }
}

[Serializable]
public struct Core
{
    public Aabb aabb;
    public int id;
    public int leftChild;
    public int rightChild;
    public List<int> triangles;
}

[Serializable]
public struct Aabb
{
    public List<float> max;
    public List<float> min;
    public Vector3 Max { get { return new Vector3(max[0], max[1], max[2]); } } 
    public Vector3 Min { get { return new Vector3(min[0], min[1], min[2]); } }
}

[Serializable]
public struct Metrics
{
    public float pa;
    public float pah;
    public float sa;
    public float sah;
}