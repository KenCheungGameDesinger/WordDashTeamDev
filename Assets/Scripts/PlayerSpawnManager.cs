using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Assets;

public class PlayerSpawnManager : NetworkBehaviour
{
    public static PlayerSpawnManager Singleton;
    public GameObject[] spawnPoints;
    public GameObject[] homes;
    
    void Awake()
    {
        Singleton = this;
    }
    
    void Start()
    {
        NetworkObject[] objs = GameObject.FindObjectsByType<NetworkObject>(FindObjectsSortMode.None);
        for(int i = 0; i < objs.Length; i++)
        {
            if(!objs[i].IsSpawned && NetworkManager.IsHost)
                objs[i].Spawn();
        }
        
        if(!NetworkManager.Singleton.IsApproved)
        {
            for(int i = 0; i < MyNetwork.Singleton.totalCount; i++)
            {
                GameObject[] spawnPointObjs = GameObject.FindGameObjectsWithTag("SpawnPoint");
                var obj = Instantiate(MyNetwork.Singleton.playerPrefabs[GameSettings.characterId]);
                obj.transform.position = spawnPointObjs[i].transform.position;
                obj.transform.localEulerAngles = new Vector3(0, UnityEngine.Random.Range(0, 360f), 0);
                obj.GetComponent<PlayerController>().positionID.Value = i;
                obj.GetComponent<PlayerController>().isAI = i != 0;
                if(i == 0)
                {
                    PlayerController.localInstance = obj.GetComponent<PlayerController>();
                }
            }
        }
        // if(NetworkManager.IsHost && (GameSettings.gameMode == EGameMode.COOPERATIVE || GameSettings.gameMode == EGameMode.COMPETITIVE))
        // {
        //     GameObject[] spawnPointObjs = GameObject.FindGameObjectsWithTag("SpawnPoint");
        //     int posID = 0;
        //     foreach (var item in MyNetwork.Singleton.playerDatas)
        //     {
        //         var obj = Instantiate(MyNetwork.Singleton.playerPrefabs[item.Value.characterId]);
        //         obj.transform.localEulerAngles = new Vector3(0, UnityEngine.Random.Range(0, 360f), 0);
        //         obj.GetComponent<NetworkObject>().SpawnWithOwnership(item.Key);
        //         if(GameSettings.gameMode == EGameMode.COOPERATIVE)
        //         {
        //             obj.transform.position = spawnPointObjs[item.Value.teamID].transform.position;
        //             obj.GetComponent<PlayerController>().teamID.Value = item.Value.teamID;
        //             obj.GetComponent<PlayerController>().positionID.Value = item.Value.teamID;
        //         }
        //         else
        //         {
        //             obj.transform.position = spawnPointObjs[posID].transform.position;
        //             obj.GetComponent<PlayerController>().positionID.Value = posID;
        //         }
        //         posID ++;
        //     }
        // }
    }
    
    public void SetHomeIndex(int index)
    {
        if(homes[index].activeSelf)
            return;
            
        for(int i = 0; i < homes.Length; i++)
        {
            homes[i].SetActive(false);
        }
        homes[index].SetActive(true);
    }
}
