using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Utilities;


[Serializable]
public struct TopLevel
{
    public List<BvhData> bvhs;
    public List<Triangle> triangles;
    public List<OctreeNode> octree;

    public Triangle findTriangle(long id)
    {
        return triangles.First(t => t.id == id);
    }

    public OctreeNode findOctreeNode(long id)
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


    public BvhNode findNode(long id)
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
    public PlaneInfluenceArea planeInfluenceArea;
    public PointInfluenceArea pointInfluenceArea;
}

[Serializable]
public struct PlaneInfluenceArea
{
    public BvhRegion bvhRegion;
    public float density;
    public Plane plane;
    public List<float> size;

    public Vector2 Size { get { return listToVector2(size); } }
}

[Serializable]
public struct PointInfluenceArea
{
    public BvhRegion bvhRegion;
    public float density;
    public Pov pov;
}

[Serializable]
public struct BvhRegion
{
    public string type;
    public Obb obb;
    public AabbForObb aabbForObb;
    public Frustum frustum;
}

[Serializable]
public struct Obb
{
    public List<float> center;
    public List<float> halfSize;
    public List<float> forward;

    public Vector3 Center { get { return listToVector3(center); } }
    public Vector3 HalfSize { get { return listToVector3(halfSize); } }
    public Vector3 Forward { get { return listToVector3(forward); } }
    public Vector3 Right { get { return Vector3.Cross(Vector3.up, Forward); } }
    public Vector3 Up { get { return Vector3.Cross(Forward, Right); } }
}

[Serializable]
public struct AabbForObb
{
    public Aabb aabb;
    public Obb obb;
}

[Serializable]
public struct Frustum
{
    public List<List<float>> matrix;
    public List<List<float>> vertices;
    public ProjectionMatrixParameters parameters;

    public Matrix4x4 Matrix { get { return listToMatrix4x4(matrix); } }
    public Vector3[] Vertices { get { return vertices.Select(list => listToVector3(list)).ToArray(); } }
}

[Serializable]
public struct Plane
{
    public List<float> point;
    public List<float> normal;

    public Vector3 Point { get { return listToVector3(point); } }
    public Vector3 Normal { get { return listToVector3(normal); } }
}

[Serializable]
public struct Pov
{
    public List<float> position;
    public List<float> direction;
    public List<float> up;

    public Vector3 Position { get { return listToVector3(position); } }
    public Vector3 Direction { get { return listToVector3(direction); } }
    public Vector3 Up { get { return listToVector3(up); } }
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
    public long id;
    public List<float> v1;
    public List<float> v2;
    public List<float> v3;

    public Vector3 V1 { get { return listToVector3(v1); } }
    public Vector3 V2 { get { return listToVector3(v2); } }
    public Vector3 V3 { get { return listToVector3(v3); } }

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
    public long id;
    public Aabb aabb;
    public List<long> bvhs;
    public List<long> children;
    public bool isLeaf;
}

[Serializable]
public struct Core
{
    public Aabb aabb;
    public long id;
    public long leftChild;
    public long rightChild;
    public List<long> triangles;
}

[Serializable]
public struct Aabb
{
    public List<float> max;
    public List<float> min;
    public Vector3 Max { get { return listToVector3(max); } } 
    public Vector3 Min { get { return listToVector3(min); } }
}

[Serializable]
public struct Metrics
{
    public float pa;
    public float pah;
    public float sa;
    public float sah;
}

[Serializable]
public struct ProjectionMatrixParameters
{
    public float n, f, b, t, l, r, fovX, fovY, ratio;
}