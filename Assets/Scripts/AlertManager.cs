using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AlertManager : MonoBehaviour
{
    public static AlertManager singleton;
    public Animator animator;
    public TextMeshProUGUI alertLabel;
    public float showTime = 0;
    
    void Awake()
    {
        singleton = this;
    }
    
    void Update()
    {
        if(Time.time > showTime + 3f)
        {
            alertLabel.gameObject.SetActive(false);
        }
    }
    
    public void OnAlert(string msg)
    {
        showTime = Time.time;
        alertLabel.text = msg;
        alertLabel.gameObject.SetActive(true);
        animator.SetBool("isShow", true);
        Invoke("OnAnimationFinished", 0.1f);
    }
    
    public void OnAnimationFinished()
    {
        animator.SetBool("isShow", false);
    }
}
