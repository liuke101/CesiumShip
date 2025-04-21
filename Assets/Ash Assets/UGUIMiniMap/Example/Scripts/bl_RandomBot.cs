using UnityEngine;

namespace UGUIMiniMap
{
    public class bl_RandomBot : MonoBehaviour
    {

        [SerializeField] private float Radius = 50;

        void FixedUpdate()
        {
            if (!Agent.hasPath)
            {
                RandomBot();
            }
        }

        void RandomBot()
        {
            Vector3 randomDirection = Random.insideUnitSphere * Radius;
            randomDirection += transform.position;
            UnityEngine.AI.NavMeshHit hit;
            UnityEngine.AI.NavMesh.SamplePosition(randomDirection, out hit, 75, 1);
            Vector3 finalPosition = hit.position;
            Agent.SetDestination(finalPosition);
        }

        private UnityEngine.AI.NavMeshAgent m_Agent;
        private UnityEngine.AI.NavMeshAgent Agent
        {
            get
            {
                if (m_Agent == null)
                {
                    m_Agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
                }
                return m_Agent;
            }
        }
    }
}