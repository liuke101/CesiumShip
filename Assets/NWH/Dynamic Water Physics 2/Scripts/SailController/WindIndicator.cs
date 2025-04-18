using UnityEngine;

#if UNITY_EDITOR
using NWH.DWP2.NUI;
using NWH.DWP2.SailController;
using UnityEditor;
#endif

namespace NWH.DWP2.SailController
{
    /// <summary>
    /// Shows apparent wind if placed as a child of the SailController,
    /// or true wind otherwise.
    /// </summary>
    public class WindIndicator : MonoBehaviour
    {
        private SailController _sailController;


        private void Awake()
        {
            _sailController = GetComponentInParent<SailController>();
        }
        

        private void Update()
        {
            if (_sailController != null)
            {
                // Show apparent wind.
                transform.rotation = Quaternion.LookRotation(_sailController.ApparentWind.normalized, 
                    transform.parent.up);
            }
            else if (WindGenerator.Instance != null)
            {
                // Show true wind.
                transform.rotation = Quaternion.LookRotation(WindGenerator.Instance.CurrentWind.normalized, 
                    transform.parent.up);
            }
        }
    }
}


#if UNITY_EDITOR
namespace NWH.DWP2.SailController
{
    [CustomEditor(typeof(WindIndicator))]
    [CanEditMultipleObjects]
    public class WindIndicatorEditor : DWP_NUIEditor
    {
        public override bool OnInspectorNUI()
        {
            if (!base.OnInspectorNUI())
            {
                return false;
            }
            
            drawer.Info("Shows apparent wind if placed as a child of the SailController, " +
                        "or true wind otherwise.");

            drawer.EndEditor(this);
            return true;
        }
    }
}
#endif

