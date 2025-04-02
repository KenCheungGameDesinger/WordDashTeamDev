using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

namespace Assets
{
    public class PitLetterManager : NetworkBehaviour
    {
        PitManager pitManager;
        public List<GameObject> prefabList;
        public Transform alphaParentTransform;

        [HideInInspector]
        public NetworkVariable<int> index = new NetworkVariable<int>();

        // Use this for initialization
        void Start()
        {
            if(!NetworkManager.Singleton.IsApproved)
            {
                index.Value = GetChildIndex();
                CreateChild();
            }
        }
        
        int GetChildIndex()
        {
            int[] vowelIndexes = {0, 4, 8, 14, 20, 24};
            float rand = Random.Range(0, 1f);
            if(rand < .3f)
            {
                return vowelIndexes[Random.Range(0, vowelIndexes.Length)];
            }
            else
            {
                return Random.Range(0, prefabList.Count);                
            }
        }
        
        public override void OnNetworkSpawn()
        {
            if (IsHost)
            {
                index.Value = GetChildIndex();
            }
            CreateChild();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SetPitManager(PitManager pm)
        {
            pitManager = pm;
        }

        public void Despawn()
        {
            if (pitManager)
                pitManager.DespawnLetter(gameObject);
            else
            {
                if(IsClient)
                    DespawnObjectServerRpc();
                else
                {
                    if(NetworkManager.Singleton.IsApproved)
                        NetworkObject.Despawn(true);
                    else
                        Destroy(gameObject);
                }
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void DespawnObjectServerRpc(ServerRpcParams serverRpcParams = default)
        {
            NetworkObject.Despawn(true);
        }

        void CreateChild()
        {
            var obj = GameObject.Instantiate(prefabList[index.Value], alphaParentTransform);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = Vector3.one * 80;
            obj.transform.localRotation = Quaternion.Euler(0f, Random.Range(0,360f), 0f);
        }
    }
}