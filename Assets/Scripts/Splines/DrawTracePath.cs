using System.Collections.Generic;
using OmniVehicleAi;
using UnityEngine;

public class DrawTracePath : MonoBehaviour
{
    public PathProgressTracker pathProgressTracker;
    private LineRenderer lineRenderer;
    private float time = 0;
    public float offsetSpeed = 2.0f; // 控制纹理移动速度

    public int smoothness = 20; // 控制曲线的平滑度
    private static readonly int s_BaseMap = Shader.PropertyToID("_BaseMap");

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        if (lineRenderer && pathProgressTracker != null)
        {
            // 原始点
            Vector3[] controlPoints = new Vector3[]
            {
                transform.position,
                pathProgressTracker.A,
                pathProgressTracker.B,
                pathProgressTracker.C,
                pathProgressTracker.D
            };

            // 生成平滑点
            List<Vector3> smoothPoints = GenerateSmoothCurve(controlPoints, smoothness);

            // 设置 LineRenderer 的点
            lineRenderer.positionCount = smoothPoints.Count;
            lineRenderer.SetPositions(smoothPoints.ToArray());
            
            //材质
            time -= Time.deltaTime;
            lineRenderer.material.SetTextureOffset(s_BaseMap, new Vector2(time * offsetSpeed,0));
        }
    }

    private List<Vector3> GenerateSmoothCurve(Vector3[] controlPoints, int smoothness)
    {
        List<Vector3> smoothPoints = new List<Vector3>();

        for (int i = 0; i < controlPoints.Length - 1; i++)
        {
            Vector3 p0 = controlPoints[Mathf.Max(i - 1, 0)];
            Vector3 p1 = controlPoints[i];
            Vector3 p2 = controlPoints[i + 1];
            Vector3 p3 = controlPoints[Mathf.Min(i + 2, controlPoints.Length - 1)];

            for (int j = 0; j < smoothness; j++)
            {
                float t = j / (float)smoothness;
                Vector3 point = CatmullRom(p0, p1, p2, p3, t);
                smoothPoints.Add(point);
            }
        }

        // 添加最后一个点
        smoothPoints.Add(controlPoints[controlPoints.Length - 1]);

        return smoothPoints;
    }

    private Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f * (
            (2 * p1) +
            (-p0 + p2) * t +
            (2 * p0 - 5 * p1 + 4 * p2 - p3) * t2 +
            (-p0 + 3 * p1 - 3 * p2 + p3) * t3
        );
    }
}