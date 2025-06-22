using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

public class SplineSpawner : MonoBehaviour
{
    public SplineContainer spline;
    public GameObject prefab;
    public float spawnsPerHour = 350f;
    public float movementSpeed = 5f;
    public float rotationSmoothness = 40f;
    public float minSpawnInterval = 0.5f; // 新增：最小生成间隔(秒)

    private float averageInterval;
    private float nextSpawnTime;

    void Start()
    {
        averageInterval = 3600f / spawnsPerHour;
        nextSpawnTime = Time.time + GetRandomInterval();
    }

    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            StartCoroutine(SpawnObject());
            nextSpawnTime = Time.time + GetRandomInterval();
        }
    }

    private float GetRandomInterval()
    {
        // 在平均间隔±50%范围内随机，但不少于最小间隔
        float randomFactor = Random.Range(0.5f, 1.5f);
        return Mathf.Max(averageInterval * randomFactor, minSpawnInterval);
    }

    IEnumerator SpawnObject()
    {
        GameObject obj = Instantiate(prefab, spline.EvaluatePosition(0), Quaternion.identity);
        float progress = 0f;
        Vector3 previousPosition = obj.transform.position;

        while (progress < 1f)
        {
            progress += Time.deltaTime * movementSpeed / spline.CalculateLength();
            Vector3 newPosition = spline.EvaluatePosition(progress);
            
            // 计算移动方向并旋转物体
            Vector3 moveDirection = (newPosition - previousPosition).normalized;
            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                obj.transform.rotation = Quaternion.Slerp(
                    obj.transform.rotation, 
                    targetRotation, 
                    rotationSmoothness * Time.deltaTime
                );
            }
            
            obj.transform.position = newPosition;
            previousPosition = newPosition;
            yield return null;
        }

        Destroy(obj);
    }
}