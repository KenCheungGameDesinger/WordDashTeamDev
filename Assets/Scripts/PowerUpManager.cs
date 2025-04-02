using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;

public class PowerUpManager : NetworkBehaviour
{
    public static PowerUpManager Singleton;
    
    public GameObject[] powerUpItems;
    
    public static int InitialCount = 30;
    public static int MaxCount = 30;
    public static int SpawnTime = 30;

    float remainTime = 100;
    List<GameObject> spawnedList = new List<GameObject>();

    public GameObject centerObj;
    // Use this for initialization
    void Start()
    {
        Singleton = this;
        remainTime = SpawnTime;
        StartCoroutine(SpawnInitPowerUps());
    }
    
    public IEnumerator SpawnInitPowerUps()
    {
        yield return new WaitForSeconds(10f);
        for (int i = 0; i < InitialCount; i++)
        {
            yield return new WaitForSeconds(0.3f);
            SpawnItem();
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (spawnedList.Count(item => item != null) < MaxCount)
        {
            remainTime -= Time.fixedDeltaTime;
            if (remainTime < 0)
                SpawnItem();
        }

    }

    void SpawnItem()
    {
        if(!IsHost && NetworkManager.Singleton.IsApproved)
            return;
        var obj = GameObject.Instantiate(powerUpItems[Random.Range(0, powerUpItems.Length)]);
        obj.transform.position = centerObj.transform.position + new Vector3((Random.value - 0.5f) * 90, 10f, (Random.value-0.5f) * 90) / 2;
        spawnedList.Add(obj);
        if(NetworkManager.Singleton.IsApproved)
            obj.GetComponent<NetworkObject>().Spawn();

        remainTime = SpawnTime;
    }
    public void DespawnItem(GameObject obj)
    {
        spawnedList.Remove(obj);
        GameObject.Destroy(obj);
    }
}
