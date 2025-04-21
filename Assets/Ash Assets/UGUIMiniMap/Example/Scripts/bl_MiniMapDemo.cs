using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class bl_MiniMapDemo : MonoBehaviour
{

    public int MapID = 2;
    public const string MMName = "MMManagerExample";

    public bl_MiniMap[] Maps;
    public Dropdown mapsDropdown;
    private bool Rotation = true;
    private bl_MiniMap CurrentMiniMap;

    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        MapID = PlayerPrefs.GetInt("MMExampleMapID", 0);

        mapsDropdown.ClearOptions();
        var op = new List<Dropdown.OptionData>();
        foreach (var item in Maps)
        {
            op.Add(new Dropdown.OptionData()
            {
                text = item.name.ToUpper()
            });
        }
        mapsDropdown.AddOptions(op);
        ApplyMap();
    }

    /// <summary>
    /// 
    /// </summary>
    void ApplyMap()
    {
        for (int i = 0; i < Maps.Length; i++)
        {
            Maps[i].SetActive(false);
        }

        Maps[MapID].SetActive(true);
        CurrentMiniMap = Maps[MapID];
        mapsDropdown.value = MapID;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ChangeRotation();
        }
        
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;       
        }
        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void ChangeRotation()
    {
        Rotation = !Rotation;
        Maps[MapID].GetComponentInChildren<bl_MiniMap>().SetMapRotationMode(Rotation);

    }

    public void SetIconMulti(float v)
    {
        CurrentMiniMap.IconMultiplier = v;
    }

    public void SetGridSize(float v)
    {
        CurrentMiniMap.SetGridSize(v);
    }

    public void SetGrid(bool v)
    {
        CurrentMiniMap.SetActiveGrid(v);
    }

    public void SetDynamicRot(bool v)
    {
        CurrentMiniMap.SetMapRotationMode(v);
    }

    public void SetFadeOnFullscreen(bool fadeOn)
    {
        CurrentMiniMap.FadeOnFullScreen = fadeOn;
    }
    
    public void ChangeMap(int i)
    {
        PlayerPrefs.SetInt("MMExampleMapID", i);
        Maps[i].SetAsActiveMiniMap();
        mapsDropdown.OnDeselect(null);
        MapID = i;
        CurrentMiniMap = Maps[MapID];
    }

    public bl_MiniMap GetActiveMiniMap
    {
        get
        {
            bl_MiniMap m = Maps[MapID].GetComponentInChildren<bl_MiniMap>();
            return m;
        }
    }
}