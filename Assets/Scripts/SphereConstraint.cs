using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class SphereConstraint : MonoBehaviour
{
    [SerializeField] SphereCollider sphere;

    void OnDrawGizmos()
    {
        Vector3 distance = transform.position - sphere.center;
        Vector3 pos = sphere.center + distance.normalized * sphere.radius;
        transform.position = pos;
        Gizmos.color = GetComponent<Renderer>().material.color;
        Gizmos.DrawSphere(pos, GetComponent<SphereCollider>().radius * transform.lossyScale.x);
    }
}
