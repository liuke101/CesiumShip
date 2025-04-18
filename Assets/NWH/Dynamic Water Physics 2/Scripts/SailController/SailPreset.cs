using System;
using UnityEngine;

#if UNITY_EDITOR
using NWH.DWP2.NUI;
using UnityEditor;
#endif

namespace NWH.DWP2.SailController
{
    [CreateAssetMenu(fileName = "SailPreset", menuName = "NWH/DWP2/SailPreset", order = 1)]
    public class SailPreset : ScriptableObject
    {
        public string         description;
        public float          liftScale                 = 1f;
        public AnimationCurve liftCoefficientVsAoACurve = new AnimationCurve();
        public float          dragScale                 = 1f;
        public AnimationCurve dragCoefficientVsAoACurve = new AnimationCurve();


        private void Reset()
        {
            liftCoefficientVsAoACurve = GetDefaultLiftCurve();
            dragCoefficientVsAoACurve = GetDefaultDragCurve();
        }


        private AnimationCurve GetDefaultDragCurve()
        {
            AnimationCurve dragCurve = new AnimationCurve();

            for (float angle = -180f; angle <= 180f; angle += 20f)
            {
                float angleRadians     = angle * Mathf.Deg2Rad;
                float forceCoefficient = Mathf.Sin(angleRadians);
                dragCurve.AddKey(angle, forceCoefficient);
            }

            return dragCurve;
        }


        private AnimationCurve GetDefaultLiftCurve()
        {
            AnimationCurve liftCurve = new AnimationCurve();

            for (float angle = -180f; angle <= 180f; angle += 20f)
            {
                float angleRadians     = angle * Mathf.Deg2Rad;
                float forceCoefficient = Mathf.Cos(angleRadians * 2f);
                liftCurve.AddKey(angle, forceCoefficient);
            }

            return liftCurve;
        }
    }
}


#if UNITY_EDITOR
namespace NWH.DWP2.SailController
{
    [CustomEditor(typeof(SailPreset))]
    [CanEditMultipleObjects]
    public class SailPresetEditor : DWP_NUIEditor
    {
        public override bool OnInspectorNUI()
        {
            if (!base.OnInspectorNUI())
            {
                return false;
            }

            SailPreset sailPreset = (SailPreset)target;

            EditorGUILayout.Space(30f);
            EditorGUILayout.LabelField("Description:");
            sailPreset.description = EditorGUILayout.TextArea(sailPreset.description, GUILayout.Height(60f));
            drawer.Space(100f);

            drawer.BeginSubsection("Drag");
            drawer.Field("dragScale",                 true, "x100%");
            drawer.Field("dragCoefficientVsAoACurve", true, null, "Drag Coeff. vs AoA");
            drawer.EndSubsection();

            drawer.BeginSubsection("Lift");
            drawer.Field("liftScale",                 true, "x100%");
            drawer.Field("liftCoefficientVsAoACurve", true, null, "Lift Coeff. vs AoA");
            drawer.EndSubsection();

            drawer.EndEditor(this);
            return true;
        }
    }
}
#endif