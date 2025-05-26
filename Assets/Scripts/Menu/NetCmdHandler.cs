using System;
using System.IO;
using UnityEngine;
using Battlehub.UIControls.MenuControl;
using Best.WebSockets.Implementations;
using System.Text;
using Unity.Collections.LowLevel.Unsafe;

public class NetCmdHandler : MonoBehaviour
{
    public GameObject NetCommunication;
    
    public void ShowSetting()
    {
        NetCommunication.SetActive(!NetCommunication.activeSelf);
    }
    
    public void Connect()
    {
        WebSocketManager.Instance.SwitchState(true);
    }
    
    public void Disconnect()
    {
        WebSocketManager.Instance.SwitchState(false);
    }
}