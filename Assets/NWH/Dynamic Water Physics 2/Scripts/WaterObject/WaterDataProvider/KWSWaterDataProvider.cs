#if DWP_KWS

using System;
using KWS;
using NWH.DWP2.WaterObjects;
using UnityEngine;


namespace NWH.DWP2.WaterData
{
    [DefaultExecutionOrder(-50)]
    public class KWSWaterDataProvider : WaterDataProvider
    {
        public override bool SupportsWaterHeightQueries()
        {
            return true;
        }

        public override bool SupportsWaterNormalQueries()
        {
            return false;
        }

        public override bool SupportsWaterFlowQueries()
        {
            return false;
        }

        public override void GetWaterHeights(WaterObject waterObject, ref Vector3[] points, ref float[] waterHeights)
        {
            for (int i = 0; i < points.Length; i++)
            {
                var waterSurfaceData = WaterSystem.GetWaterSurfaceData(points[i]);
                if (waterSurfaceData.IsActualDataReady)
                {
                    waterHeights[i] = waterSurfaceData.Position.y;
                }
            }
        }
    }
}

#endif