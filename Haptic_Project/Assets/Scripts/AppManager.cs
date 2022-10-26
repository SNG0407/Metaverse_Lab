using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppManager : MonoBehaviour
{
    private static AppManager _instance;
    public static AppManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<AppManager>();
            if (_instance == null)
            {
                GameObject container = new GameObject("Singleton");
                _instance = container.AddComponent<AppManager>();
            }
            return _instance;
        }
    }

    public HandController handController;

    private void Awake()
    {
        
    }
}