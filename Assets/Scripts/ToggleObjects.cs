using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleObjects : MonoBehaviour
{
    public Toggle mainToggle;
    public List<GameObject> objs = new List<GameObject>();
    // Use this for initialization
    void Start()
    {
        if(mainToggle)
            InvertObjects(mainToggle.isOn);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void InvertObjects(Boolean b)
    {
        foreach (var obj in objs)
        {
            obj.SetActive(b);
        }
    }

    public void ShowObjects()
    {
        foreach (var obj in objs)
        {
            obj.SetActive(true);
        }
    }
    public void HideObjects()
    {
        foreach (var obj in objs)
        {
            obj.SetActive(false);
        }
    }
}
