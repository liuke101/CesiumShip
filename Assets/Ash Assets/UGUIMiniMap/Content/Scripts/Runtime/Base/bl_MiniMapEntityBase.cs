using Lovatto.MiniMap;
using System;
using UnityEngine;

public abstract class bl_MiniMapEntityBase : MonoBehaviour
{

    [Serializable]
    public enum InteracableAction
    {
        OnHover,
        OnTouch,
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract Transform GetTarget
    {
        get;
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract bool IsInteractable
    {
        get;
        set;
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract InteracableAction InteractAction
    {
        get;
        set;
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract void OnUpdateItem();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="iconSettings"></param>
    public abstract void SetIconSettings(MiniMapIconSettings iconSettings);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newTarget"></param>
    public abstract void SetTarget(Transform newTarget);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newSprite"></param>
    public abstract void SetIcon(Sprite newSprite);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newIconColor"></param>
    public abstract void SetIconColor(Color newIconColor);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="text"></param>
    public abstract void SetIconText(string text);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="radius"></param>
    /// <param name="AreaColor"></param>
    public abstract void SetCircleArea(float radius, Color AreaColor);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public abstract void SetActiveIcon(bool active);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public abstract void SetActiveCircleArea(bool active);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="inmediate"></param>
    public abstract void DestroyIcon(bool inmediate = false);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newOwner"></param>
    public abstract void ChangeMiniMapOwner(bl_MiniMap newOwner);

    /// <summary>
    /// Called when the minimap zoom or fullscreen mode is changed
    /// </summary>
    /// <param name="viewPortRatio">the size ratio difference compared to the default values, this can be used to scale sizes of icons to keep consistency in UI.</param>
    public virtual void OnViewportChanged(float viewPortRatio)
    {
    }

    /// <summary>
    /// Event called when the minimap icon entity is clicked
    /// </summary>
    /// <returns>false if there is NOT custom event to execute.</returns>
    public virtual bool InvokeOnClickEvent()
    {
        return false;
    }
}