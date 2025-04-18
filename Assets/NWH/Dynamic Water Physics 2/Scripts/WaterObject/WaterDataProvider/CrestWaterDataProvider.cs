#if DWP_CREST

using System.Collections.Generic;
using UnityEngine;
using Crest;
using NWH.DWP2.WaterObjects;

namespace NWH.DWP2.WaterData
{
    public class CrestWaterDataProvider : WaterDataProvider
    {
        private OceanRenderer _oceanRenderer;

        public override bool SupportsWaterHeightQueries()
        {
            return true;
        }


        public override bool SupportsWaterNormalQueries()
        {
            return true;
        }


        public override bool SupportsWaterFlowQueries()
        {
            return true;
        }


        public override void Awake()
        {
            base.Awake();

            _oceanRenderer = GetComponent<OceanRenderer>();
            if (_oceanRenderer == null)
            {
                Debug.LogError($"{typeof(OceanRenderer)} not found. " +
                               $"{GetType()} needs to be attached to an object containing {typeof(OceanRenderer)}.");
            }
        }


        public override void GetWaterHeights(WaterObject waterObject, ref Vector3[] points, ref float[] waterHeights)
        {
            _oceanRenderer.CollisionProvider.Query(waterObject.instanceID, 0, points, waterHeights, null, null);
        }


        public override void GetWaterNormals(WaterObject waterObject, ref Vector3[] points, ref Vector3[] waterNormals)
        {
            // Flip the instance sign to not get overlapping queries for same provider. InstanceID is always negative so no duplicates should ever happen.
            _oceanRenderer.CollisionProvider.Query(-waterObject.instanceID, 0, points, (float[])null, waterNormals, null);
        }


        public override void GetWaterFlows(WaterObject waterObject, ref Vector3[] points, ref Vector3[] waterFlows)
        {
            _oceanRenderer.FlowProvider.Query(waterObject.instanceID, 0, points, waterFlows);
        }


        public override float GetWaterHeightSingle(WaterObject waterObject, Vector3 point)
        {
            _singlePointArray[0] = point;
            _oceanRenderer.CollisionProvider.Query(_oceanRenderer.GetHashCode(), 0,
                _singlePointArray, _singleHeightArray, null, null);
            return _singleHeightArray[0];
        }
    }
}

#endif