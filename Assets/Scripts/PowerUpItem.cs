using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Assets;

public class PowerUpItem : NetworkBehaviour
{
    public PowerUpType type;
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            PlayerController controller = other.transform.GetComponent<PlayerController>();
            if(controller == null)
            {
                controller = other.transform.parent.GetComponent<PlayerController>();
            }
            if(controller.isAI || (!controller.IsOwner && NetworkManager.Singleton.IsApproved))
                return;
                
            if(InGameHudManager.Singleton.powerUpType == PowerUpType.None)
            {
                InGameHudManager.Singleton.PickPowerUpItem(type);
                if(IsClient)
                {
                    DespawnObjectServerRpc();
                }
                else
                {
                    Destroy(this.gameObject);
                }
            }
        }
    }
        
    [ServerRpc(RequireOwnership = false)]
    public void DespawnObjectServerRpc(ServerRpcParams serverRpcParams = default)
    {
        NetworkObject.Despawn(true);
    }
}
