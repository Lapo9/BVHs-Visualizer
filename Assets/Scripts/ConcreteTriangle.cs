using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConcreteTriangle : MonoBehaviour
{
    public Triangle Triangle { get; private set; }

    public static ConcreteTriangle initialize(Triangle triangle, Transform parent, ConcreteTriangle prefab)
    {
        //create the object
        ConcreteTriangle concreteTriangle = Instantiate(prefab);
        var mesh = new Mesh();

        //create the mesh
        mesh.vertices = new Vector3[] { triangle.V1, triangle.V2, triangle.V3 };
        mesh.triangles = new int[] { 0, 1, 2 };
        concreteTriangle.transform.GetComponent<MeshFilter>().sharedMesh = mesh;

        //set random color
        var triangleMaterial = new Material(concreteTriangle.GetComponent<MeshRenderer>().sharedMaterial);
        triangleMaterial.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 0.8f);
        concreteTriangle.GetComponent<MeshRenderer>().sharedMaterial = triangleMaterial;

        //set properties
        concreteTriangle.transform.parent = parent;
        concreteTriangle.name = "Triangle";

        return concreteTriangle;
    }
}
