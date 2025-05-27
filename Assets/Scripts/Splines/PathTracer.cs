using System.Collections;
using System.Collections.Generic;
using ShipAI;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineContainer), typeof(LineRenderer))]
public class PathTracer : MonoBehaviour
{
    public AIShipController AIShipController;
    private SplineContainer spline;
    
    
    [Range(0f, 3f)]
    public float delay = 2f;  // 目标替换前的时间（以秒计）
    private float TraceTimer = 0f; // 追踪延迟计时器
    private float interval = 0f; // spline evaluation 插值
    [Range(0f, 0.1f)]
    public float intervalAddSpeed = 0.02f; // 插值增加速度
    
    //lineRenderer
    private LineRenderer lineRenderer;
    public float offsetSpeed = 2.0f; // 控制纹理移动速度

    public int smoothness = 20; // 控制曲线的平滑度
    private static readonly int s_BaseMap = Shader.PropertyToID("_BaseMap");
    private float RenderTimer = 0f; // 渲染计时器
    
    
    void Awake()
    {
        spline = GetComponent<SplineContainer>();
        lineRenderer = GetComponent<LineRenderer>();
    }
    
    void Start()
    {
        if(AIShipController)
        {
            AIShipController.target.position = spline.EvaluatePosition(0f);
        }
        
        InitLineRender();
    }

    void Update()
    {
        UpdateTarget();
        UpdateLineMaterial();
    }
    
    /// <summary>
    /// 更新目标位置
    /// </summary>
    private void UpdateTarget()
    {
        if (AIShipController)
        {
            Vector3 targetPosition = AIShipController.target.position;
            Vector3 vehiclePosition = AIShipController.vehicleTransform.position;
            float distance = (targetPosition - vehiclePosition).magnitude;

            if (distance < 5f)
            {
                TraceTimer += Time.deltaTime;
                if(TraceTimer >= delay)
                {
                    AIShipController.target.position = spline.EvaluatePosition(interval);
                    interval += intervalAddSpeed;
                    
                    //到达终点
                    if(Mathf.Approximately(interval, 1.0f))
                    {
                        interval = 0f; //测试：重置为0
                    }
                    
                    TraceTimer = 0f;
                }
            }
            else
            {
                TraceTimer = 0f;
            }
        }
    }

    /// <summary>
    /// 初始化LineRender
    /// </summary>
    private void InitLineRender()
    {
        if (spline == null || lineRenderer == null)
        {
            return;
        }

        // 获取样条曲线上的点
        List<Vector3> points = new List<Vector3>();
        float i = 0f;
        while (i < 1.0f)
        {
            //points添加元素
            points.Add(spline.EvaluatePosition(i));
            i += 0.01f;
        }

        // 设置 LineRenderer 的点
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }

    /// <summary>
    /// 更新LineRenderer材质
    /// </summary>
    private void UpdateLineMaterial()
    {
        //材质
        RenderTimer -= Time.deltaTime;
        lineRenderer.material.SetTextureOffset(s_BaseMap, new Vector2(RenderTimer * offsetSpeed,0));
    }
    
}
