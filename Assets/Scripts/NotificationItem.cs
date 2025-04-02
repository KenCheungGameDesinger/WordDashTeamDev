using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NotificationItem : MonoBehaviour
{
    [SerializeField] TMP_Text msgTxt;
    
    public void SetMessage(string msg)
    {
        msgTxt.text = msg;
    }
    
    void Start()
    {
        Invoke("RemoveItem", 5f);
    }
    
    void RemoveItem()
    {
        NotificationManager.Singleton.notificationItems.Remove(this);
        Destroy(gameObject);
    }
}
