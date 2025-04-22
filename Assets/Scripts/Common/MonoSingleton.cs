using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 单例模板类，需要挂到GameObject上
/// </summary>
/// <typeparam name="T">模板类</typeparam>
public class MonoSingleton<T> : MonoBehaviour where T:MonoBehaviour
{
    private static T _instance;

    public static T Instance => _instance;

    protected virtual void Awake() 
    {
        if (_instance != null)
        {
            Destroy(gameObject);
        }       
        else
        {
            _instance = this as T;
        }
    }
}