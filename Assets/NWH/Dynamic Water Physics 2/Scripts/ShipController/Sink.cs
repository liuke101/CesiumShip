using System.Collections;
using NWH.Common.CoM;
using UnityEngine;

namespace NWH.DWP2.ShipController
{
    public class Sink : MonoBehaviour, IMassAffector
    {
        [Tooltip("Percentage of initial mass that will be added each second to imitate water ingress")]
        public float addedMassPercentPerSecond = 0.1f;

        [Tooltip("Maximum added mass after water ingress. 1f = 100% of orginal mass, 2f = 200% of original mass, etc.")]
        public float maxAdditionalMass = 100000f;

        private float _mass;
        private VariableCenterOfMass _variableCenterOfMass;
        private Coroutine _sinkCoroutine;
        
        private void Start()
        {
            _variableCenterOfMass = GetComponentInParent<VariableCenterOfMass>();
            _variableCenterOfMass.useMassAffectors = true;
            _variableCenterOfMass.useDefaultMass = false;
            _variableCenterOfMass.useDefaultCenterOfMass = false;
            _mass = 0f;
        }

        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, 0.2f);
        }

        
        public IEnumerator SinkCoroutine()
        {
            while (true)
            {
                _mass += maxAdditionalMass * addedMassPercentPerSecond;
                _mass = Mathf.Clamp(_mass, 0f, maxAdditionalMass);
                yield return new WaitForSeconds(1f);
            }
        }


        public void StartSinking()
        {
            if (_sinkCoroutine == null)
            {
                _sinkCoroutine = StartCoroutine(SinkCoroutine());
            }
        }


        public void StopSinking()
        {
            if (_sinkCoroutine != null)
            {
                StopCoroutine(SinkCoroutine());
            }
        }


        public void ResetMass()
        {
            _mass = 0f;
        }


        public float GetMass()
        {
            return _mass;
        }


        public Vector3 GetWorldCenterOfMass()
        {
            return transform.position;
        }


        public Transform GetTransform()
        {
            return transform;
        }
    }
}