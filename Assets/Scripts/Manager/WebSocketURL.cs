using System;
using TMPro;
using UnityEngine;

public class WebSocketURL : MonoBehaviour
{
    private TMP_InputField inputField;

    //写死URL,可以删掉
    private void Awake()
    {
        inputField = GetComponent<TMP_InputField>();
        if (inputField)
        {
            inputField.text = "ws://127.0.0.1:6006";
        }
    }

    private void Start()
    {
        UpdateURL(inputField.text);
        inputField.onEndEdit.AddListener(UpdateURL);
    }

    private void UpdateURL(string url)
    {
        WebSocketManager.Instance.URL = url;
    }
}