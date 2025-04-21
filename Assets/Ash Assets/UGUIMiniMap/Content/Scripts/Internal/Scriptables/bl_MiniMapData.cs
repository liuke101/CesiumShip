using Lovatto.MiniMap;
using System;
using UnityEngine;

public class bl_MiniMapData : ScriptableObject
{
    public RenderPipeline renderPipeline = RenderPipeline.BuiltIn;

    [Header("References")]
    public GameObject IconPrefab;
    public bl_MiniMapInputBase inputHandler;
    public bl_MiniMapIconData emptyIconData;
    public GameObject ScreenShotPrefab;
    public bl_MiniMapPathNav pathNavPrefab;
    public bl_MiniMapPlaneBase[] mapPlanes;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static bl_MiniMapPlaneBase GetMapPlanePrefab()
    {
        var renderPipeline = Instance.renderPipeline;
        if (renderPipeline == RenderPipeline.URP)
        {
            return Instance.mapPlanes[1];
        }
        else if (renderPipeline == RenderPipeline.HDRP)
        {
            return Instance.mapPlanes[2];
        }
        return Instance.mapPlanes[0];
    }

    [Serializable]
    public enum RenderPipeline
    {
        BuiltIn,
        URP,
        HDRP,
    }

    public static bl_MiniMapData _instance;
    public static bl_MiniMapData Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<bl_MiniMapData>("MiniMapData") as bl_MiniMapData;
            }
            return _instance;
        }
    }
}