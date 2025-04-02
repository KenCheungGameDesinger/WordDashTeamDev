using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Unity.Netcode;

public class Drop : MonoBehaviour, IDropHandler
{
    private TeamManager teamManager;
    // Use this for initialization
    void Start()
    {
        teamManager = GetComponent<TeamManager>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnDrop(PointerEventData eventData)
    {
        if(!NetworkManager.Singleton.IsHost)
        {
            return;
        }
        var playerManager = eventData.pointerDrag.GetComponent<PlayerManager>();
        if (playerManager)
        {
            teamManager.AddPlayer(playerManager);
        }
    }
}
