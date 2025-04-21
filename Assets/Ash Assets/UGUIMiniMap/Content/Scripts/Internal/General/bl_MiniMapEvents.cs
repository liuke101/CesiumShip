using System;
using UnityEngine;

public static class bl_MiniMapEvents 
{
    /// <summary>
    /// Invoked when the minimap change to fullscreen or vise versa
    /// bool = true = full screen, false = mini map
    /// </summary>
    public static Action<bool> onSizeChanged;

    /// <summary>
    /// Invoked when the active minimap changes
    /// </summary>
    public static Action<bl_MiniMap> onActiveMiniMapChanged;

    /// <summary>
    /// Invoke when the player click in a minimap position
    /// The Vector3 is the World Space position that the player selected.
    /// </summary>
    public static Action<Vector3> onSelectPosition;
}