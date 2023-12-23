using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public struct TopLevel
{
    public List<BvhData> bvhs;
    public List<Triangle> triangles;
    public List<OctreeNode> octree;

    public Triangle findTriangle(int id)
    {
        return triangles.First(t => t.id == id);
    }

    public OctreeNode findOctreeNode(int id)
    {
        return octree.First(n => n.id == id);
    }

    public OctreeNode octreeRoot()
    {
        return octree[0];
    }
}

[Serializable]
public struct BvhData
{
    public GlobalInfo globalInfo;
    public List<Triangle> triangles;
    public List<BvhNode> nodes;
    public InfluenceArea influenceArea;


    public BvhNode findNode(int id)
    {
        return nodes.First(n => n.core.id == id);
    }

    public BvhNode root()
    {
        return nodes[0];
    }
}

[Serializable]
public struct InfluenceArea
{
    public string type;
    public Plane plane;
    public List<float> size;
    public float density;
    public BvhRegion bvhRegion;

    public Vector2 Size { get { return new Vector2(size[0], size[1]); } }
}

[Serializable]
public struct BvhRegion
{
    public string type;
    public Obb obb;
}

[Serializable]
public struct Obb
{
    public List<float> center;
    public List<float> halfSize;
    public List<float> forward;

    public Vector3 Center { get { return new Vector3(center[0], center[1], center[2]); } }
    public Vector3 HalfSize { get { return new Vector3(halfSize[0], halfSize[1], halfSize[2]); } }
    public Vector3 Forward { get { return new Vector3(forward[0], forward[1], forward[2]); } }
    public Vector3 Right { get { return Vector3.Cross(Vector3.up, Forward); } }
    public Vector3 Up { get { return Vector3.Cross(Forward, Right); } }
}

[Serializable]
public struct Plane
{
    public List<float> point;
    public List<float> normal;

    public Vector3 Point { get { return new Vector3(point[0], point[1], point[2]); } }
    public Vector3 Normal { get { return new Vector3(normal[0], normal[1], normal[2]); } }
}

[Serializable]
public struct GlobalInfo
{
    public int maxLevel;
    public int numberOfLeaves;
    public int numberOfNodes;
    public float pahCost;
    public float sahCost;
    public float internalNodeCost;
    public float leafNodeCost;
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

    public static bool operator==(Triangle a, Triangle b)
    {
        return a.id == b.id;
    }

    public static bool operator!=(Triangle a, Triangle b)
    {
        return !(a == b);
    }
}

[Serializable]
public struct BvhNode
{
    public Core core;
    public Metrics metrics;

    public bool isLeaf()
    {
        return core.leftChild == 0 && core.rightChild == 0;
    }
}

[Serializable]
public struct OctreeNode
{
    public int id;
    public Aabb aabb;
    public List<int> bvhs;
    public List<int> children;
    public bool isLeaf;
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