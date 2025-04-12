using UnityEngine;

public class SplineSegment : MonoBehaviour
{
    public SplineHandle H1;
    public SplineHandle H2;

    public bool Changed()
    {
        var result = transform.hasChanged || H1.transform.hasChanged || H2.transform.hasChanged;
        transform.hasChanged = false;
        H1.transform.hasChanged = false;
        H2.transform.hasChanged = false;

        return result;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, 0.02f);
    }
}