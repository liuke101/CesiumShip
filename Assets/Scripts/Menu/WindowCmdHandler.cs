using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Battlehub.UIControls.MenuControl;
using Best.WebSockets.Implementations;
using System.Text;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Serialization;

public class WindowCmdHandler : MonoBehaviour
{
    public GameObject SmallScreen;
    public GameObject ShipMessage;
    public GameObject QuantEval;
    
    public void ShowSmallScreenBar()
    {
        SmallScreen.SetActive(!SmallScreen.activeSelf);
    }

    public void ShowShipMessage()
    {
        ShipMessage.SetActive(!ShipMessage.activeSelf);
    }

    public void ShowQuantEval()
    {
        QuantEval.SetActive(!QuantEval.activeSelf);
    }
}