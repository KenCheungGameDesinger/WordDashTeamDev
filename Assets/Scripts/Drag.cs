using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Unity.Netcode;

public class Drag : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public Transform parentTransform;
    private Vector3 prevPosition;
    private Graphic mainGraphic;
    public Transform tempTransform;
    public bool dropped;
    // Use this for initialization
    void Start()
    {
        mainGraphic = GetComponent<Graphic>();
    }
    
    void OnEnable()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if(!NetworkManager.Singleton.IsHost)
        {
            return;
        }
        mainGraphic.raycastTarget = false;
        prevPosition = eventData.pointerCurrentRaycast.worldPosition;
        parentTransform = transform.parent;
        transform.SetParent(tempTransform);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(!NetworkManager.Singleton.IsHost)
        {
            return;
        }
        var curPosition = eventData.pointerCurrentRaycast.worldPosition;
        transform.position += curPosition - prevPosition;
        prevPosition = curPosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if(!NetworkManager.Singleton.IsHost)
        {
            return;
        }
        mainGraphic.raycastTarget = true;
        transform.SetParent(parentTransform);
    }

}
