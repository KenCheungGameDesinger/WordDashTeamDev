using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Singleton;
    
    [SerializeField] Transform notificationParent;
    [SerializeField] GameObject notificationPrefab;
    public List<NotificationItem> notificationItems = new();
    
    void Awake()
    {
        Singleton = this;
    }
    
    public void OnNotification(string msg)
    {
        if(notificationItems.Count > 4)
        {
            notificationItems.Remove(notificationItems[0]);
        }
        GameObject obj = Instantiate(notificationPrefab, notificationParent);
        NotificationItem item = obj.GetComponent<NotificationItem>();
        item.SetMessage(msg);
        notificationItems.Add(item);
    }
}
