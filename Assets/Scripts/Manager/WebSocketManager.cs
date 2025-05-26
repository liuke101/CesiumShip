using System;
using System.Collections;
using System.Collections.Concurrent;
using System.IO;
using Best.HTTP.Shared.PlatformSupport.Memory;
using UnityEngine;
using Best.WebSockets;
using Best.WebSockets.Implementations;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class WebSocketManager : MonoSingleton<WebSocketManager>
{
    [Header("WebSocket")] public string URL = "ws://127.0.0.1:6006";
    public WebSocket WebSocket;
    
    private IEnumerator CheckConnection;
    
    public UnityEvent WebSocketOpen;
    public UnityEvent WebSocketClose;
    
    private void Start()
    {
        if (WebSocket == null)
        {
            NewWebSocket();
        }
    }
    
    private void NewWebSocket()
    {
        WebSocket = new WebSocket(new Uri(URL));
        WebSocket.OnOpen += OnWebSocketOpen;
        WebSocket.OnMessage += OnMessageReceived;
        WebSocket.OnBinary += OnBinaryReceived;
        WebSocket.OnClosed += OnWebSocketClosed;
        WebSocket.Open();
        //轮训检查是否连接
        CheckConnection = CheckWebSocket();
        StartCoroutine(CheckConnection);
    }

    private void OnWebSocketOpen(WebSocket webSocket)
    {
        print("WebSocket连接成功");
        webSocket.Send("Client_Connect");
        WebSocketOpen.Invoke();
        //结束检查连接
        StopCoroutine(CheckConnection);
    }
    private void OnWebSocketClosed(WebSocket webSocket, WebSocketStatusCodes code, string message)
    {
        WebSocketClose.Invoke();
    }
    
    private void OnMessageReceived(WebSocket webSocket, string message)
    {
       
    }
    
     private void OnBinaryReceived(WebSocket websocket, BufferSegment buffer)
     {
     }
    
    private void OnDestroy()
    {
        if (WebSocket != null && WebSocket.State == WebSocketStates.Open)
        {
            WebSocket.Close();
        }
    }
    
    private IEnumerator CheckWebSocket()
    {
        while (true)
        {
            if (WebSocket.State == WebSocketStates.Closed)
            {
                NewWebSocket();
            }
            yield return new WaitForSeconds(1);
        }
    }
    
    //开关UI
    public void SwitchState(bool isOpen)
    {
        if (isOpen && WebSocket.State == WebSocketStates.Closed)
        {
            NewWebSocket();
        }
        
        
        if(!isOpen && WebSocket.State == WebSocketStates.Open)
        {
            WebSocket.Close();
        }
    }
}
