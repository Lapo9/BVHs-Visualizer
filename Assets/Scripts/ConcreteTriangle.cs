using UnityEngine;

public class ConcreteTriangle : MonoBehaviour
{
    [SerializeField] long id;
    [SerializeField] Vector3 v1;
    [SerializeField] Vector3 v2;
    [SerializeField] Vector3 v3;

    public Triangle Triangle { get; private set; }


    void OnDrawGizmos()
    {
        id = Triangle.id;
        v1 = Triangle.V0;
        v2 = Triangle.V1;
        v3 = Triangle.V2;
    }

    public static ConcreteTriangle initialize(Triangle triangle, Transform parent, ConcreteTriangle prefab)
    {
        //create the object
        ConcreteTriangle concreteTriangle = Instantiate(prefab);
        var mesh = new Mesh();

        //create the mesh
        mesh.vertices = new Vector3[] { triangle.V0, triangle.V1, triangle.V2 };
        mesh.triangles = new int[] { 0, 1, 2 };
        concreteTriangle.transform.GetComponent<MeshFilter>().sharedMesh = mesh;

        //set random color
        var triangleMaterial = new Material(concreteTriangle.GetComponent<MeshRenderer>().sharedMaterial);
        triangleMaterial.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 0.8f);
        concreteTriangle.GetComponent<MeshRenderer>().sharedMaterial = triangleMaterial;

        //set properties
        concreteTriangle.transform.parent = parent;
        concreteTriangle.name = triangle.id.ToString();

        //set data
        concreteTriangle.Triangle = triangle;

        return concreteTriangle;
    }
}
