using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class HillSpawnManager : MonoBehaviour
{
    public GameObject[] hills;
    
    // IEnumerator Start()
    // {
    //     yield return new WaitForSeconds(5f);
    //     // if(NetworkManager.Singleton.IsHost || !NetworkManager.Singleton.IsApproved)
    //     {
    //         float x, y;
    //         for(x = -40; x <= 40; x += 10)
    //         {
    //             for(y = -40; y <= 40; y += 10)
    //             {
    //                 // if(Random.Range(0, 1f) < 0.5f)
    //                 //     continue;
    //                 var obj = Instantiate(GetRandHill(), new Vector3(x, 2, y), Quaternion.identity);
    //                 // if(NetworkManager.Singleton.IsApproved)
    //                     // obj.GetComponent<NetworkObject>().Spawn();
    //                 yield return new WaitForSeconds(0.2f);
    //             }
    //         }
    //     }
    // }
    
    // public GameObject GetRandHill()
    // {
    //     return hills[0];
    //     return hills[Random.Range(0, hills.Length)];
    // }
}
