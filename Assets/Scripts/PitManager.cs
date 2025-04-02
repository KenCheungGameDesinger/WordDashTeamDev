using System.Collections;
using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;

namespace Assets
{
    public class PitManager : NetworkBehaviour
    {
        public static PitManager Singleton;
        
        public static int InitialCount = 200;
        public static int MaxCount = 240;
        public static float SpawnTime = 0.3f;
        public bool isSpawning = false;

        float remainTime = 100;
        public List<GameObject> spawnedList = new List<GameObject>();

        public GameObject centerObj;
        public GameObject letterPrefab;
        // Use this for initialization
        void Start()
        {
            Singleton = this;
            remainTime = SpawnTime;
            StartCoroutine(SpawnInitLetters());
        }
        
        public IEnumerator SpawnInitLetters()
        {
            yield return new WaitForSeconds(10f);
            isSpawning = true;
            for (int i = 0; i < InitialCount; i++)
            {
                yield return new WaitForSeconds(0.3f);
                SpawnLetter();                
            }
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (isSpawning && spawnedList.Count(item => item != null) < MaxCount)
            {
                remainTime -= Time.fixedDeltaTime;
                if (remainTime < 0)
                    SpawnLetter();
            }
        }

        void SpawnLetter()
        {
            if(!IsHost && NetworkManager.Singleton.IsApproved)
                return;            
                
            var obj = Instantiate(letterPrefab);
            SpawnLetterPoint spawnpoint = GetSpawnPoint();
            obj.transform.position = spawnpoint == null ? (centerObj.transform.position + new Vector3((Random.value - 0.5f) * 90, 10f, (Random.value-0.5f) * 90) / 2) : spawnpoint.transform.position + new Vector3(0, 10f, 0);
            obj.GetComponent<PitLetterManager>().SetPitManager(this);
            spawnedList.Add(obj);
            if(spawnpoint != null)
                spawnpoint.letterObj = obj;
            if(NetworkManager.Singleton.IsApproved)
                obj.GetComponent<NetworkObject>().Spawn();

            remainTime = SpawnTime;
        }
        
        SpawnLetterPoint GetSpawnPoint()
        {
            GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnLetterPoint");
            List<Transform> spawnObjs = new List<Transform>();
            for(int i = 0; i < spawnPoints.Length; i++)
            {
                if(spawnPoints[i].GetComponent<SpawnLetterPoint>().letterObj == null)
                    spawnObjs.Add(spawnPoints[i].transform);                
            }
            if(spawnObjs.Count == 0)
                return null;
            return spawnObjs[Random.Range(0, spawnObjs.Count)].GetComponent<SpawnLetterPoint>();
        }
        
        public void DespawnLetter(GameObject obj)
        {
            spawnedList.Remove(obj);
            GameObject.Destroy(obj);
        }
    }
}