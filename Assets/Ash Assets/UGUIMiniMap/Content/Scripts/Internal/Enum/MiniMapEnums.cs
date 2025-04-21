using System;

namespace Lovatto.MiniMap
{
    public enum ItemEffect
    {
        Pulsing,
        Fade,
        None,
    }

    [Serializable]
    public enum MiniMapFullScreenMode
    {
        /// <summary>
        /// No fullscreen mode allowed
        /// </summary>
        NoFullScreen,

        /// <summary>
        /// Resize to a defined area in the screen
        /// </summary>
        ScreenArea,

        /// <summary>
        /// Auto scale to cover the whole screen
        /// </summary>
        ScaleToCoverScreen,

        /// <summary>
        /// Auto scale to fit in the screen
        /// </summary>
        ScaleToFitScreen,
    }

    public enum MiniMapCameraUpdateMode
    {
        /// <summary>
        /// Update the camera every frame
        /// </summary>
        EveryFrame,
        /// <summary>
        /// Update the camera only when the target moves
        /// </summary>
        RateLimited,
    }

    public enum MiniMapRTSize
    {
        _256,
        _512,
        _1024,
    }

    [Serializable]
    public enum MiniMapRenderType
    {
        RealTime,
        Picture,
    }

    [Serializable]
    public enum MiniMapRenderMode
    {
        Mode2D,
        Mode3D,
    }

    [Serializable]
    public enum MiniMapMapType
    {
        Local,
        Global,
    }

    [Serializable]
    public enum MiniMapMapShape
    {
        Rectangle,
        Circle
    }
}