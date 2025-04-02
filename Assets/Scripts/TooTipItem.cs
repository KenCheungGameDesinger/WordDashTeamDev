using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TooTipItem : MonoBehaviour
{
    [SerializeField] public GameObject toolTipPrefab;
    public string msg;
    public ToolTipMsg toolTip;
    
    public void OnPointEnter()
    {
        if(toolTip == null)
            toolTip = Instantiate(toolTipPrefab, MysteryHubManager.Singleton.transform).GetComponent<ToolTipMsg>();
        toolTip.transform.position = transform.position + new Vector3(0, 100, 0);
        toolTip.msg.text = msg;
        toolTip.gameObject.SetActive(true);
    }
    
    public void OnPointExit()
    {
        toolTip.gameObject.SetActive(false);
    }
}
