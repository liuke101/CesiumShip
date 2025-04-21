using UnityEngine;
using UnityEngine.AI;

namespace Lovatto.MiniMap
{
    public class bl_MiniMapPathNav : MonoBehaviour
    {
        public bool checkSurface = true;
        [SerializeField] private LineRenderer lineRenderer;
        private NavMeshPath navMeshPath;

        [SerializeField] private float pathUpdateInterval = 0.2f; // Update every 0.2s to optimize performance
        private float nextUpdateTime;
        private float baseWidth = 2.5f;
        private Transform target;
        private Vector3 endPos;
        private bool isTracking;

        void Awake()
        {
            navMeshPath = new NavMeshPath();

            // Configure Line Renderer
            lineRenderer.positionCount = 0;
            lineRenderer.useWorldSpace = true;
            baseWidth = lineRenderer.widthMultiplier;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newTarget"></param>
        public void TrackTarget(Transform newTarget, Vector3 destination, bl_MiniMap minimap)
        {
            target = newTarget;
            endPos = destination;

            if (target == null) return;

            if (!UpdatePath())
            {
                isTracking = false;
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);
            isTracking = true;
            UpdateSize(minimap);
        }

        /// <summary>
        /// 
        /// </summary>
        private void Update()
        {
            CheckDistance();

            if (!isTracking || Time.time < nextUpdateTime) return; // Rate limiting for performance
            nextUpdateTime = Time.time + pathUpdateInterval;

            if (target == null)
            {
                isTracking = false;
                gameObject.SetActive(false);
                return;
            }

            UpdatePath();
        }

        /// <summary>
        /// Updates the path from player position to the target position.
        /// </summary>
        /// <param name="startPos">Player's position</param>
        /// <param name="endPos">Target position</param>
        private bool UpdatePath()
        {
            Vector3 originPos = target.position;
            if (checkSurface)
            {
                // throw a raycast to the ground to get the correct position
                RaycastHit hit;
                if (Physics.Raycast(originPos, Vector3.down, out hit, 1000, NavMesh.AllAreas))
                {
                    originPos = hit.point;
                }
            }

            if (NavMesh.CalculatePath(originPos, endPos, NavMesh.AllAreas, navMeshPath))
            {
                DrawPath(navMeshPath);
                return true;
            }
            else
            {
                return false;
            }
        }

        private void CheckDistance()
        {
            if (target == null) return;

            Vector3 endPosRelative = new Vector3(endPos.x, target.position.y, endPos.z);
            if (Vector3.Distance(target.position, endPosRelative) < 4)
            {
                isTracking = false;
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Draws the path using the Line Renderer.
        /// </summary>
        /// <param name="path">The calculated NavMeshPath</param>
        private void DrawPath(NavMeshPath path)
        {
            if (path.corners.Length < 2) return;

            lineRenderer.positionCount = path.corners.Length;
            lineRenderer.SetPositions(path.corners);
        }

        public void UpdateSize(bl_MiniMap miniMap)
        {
            lineRenderer.widthMultiplier = baseWidth / miniMap.GetViewportRatio();
        }

        public void SetColor(Color pathColor)
        {
            var mat = lineRenderer.material;
            mat.color = pathColor;
        }

        public void SetWidth(float width)
        {
            lineRenderer.widthMultiplier = width;
            baseWidth = width;
        }
    }
}