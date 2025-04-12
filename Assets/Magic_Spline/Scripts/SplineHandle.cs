using UnityEngine;

public class SplineHandle : MonoBehaviour
{
    public GameObject Origin;
    public GameObject Opposite;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.02f);
    }

    public void SnapOppositeToAxis()
    {
        var dist = Vector3.Distance(Opposite.transform.position, Origin.transform.position);
        var dir = -(transform.position - Origin.transform.position).normalized;
        Opposite.transform.position = Origin.transform.position +  dir * dist;
    }
}