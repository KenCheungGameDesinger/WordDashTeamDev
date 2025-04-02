using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Netcode;

namespace Assets
{
    public class CompetitiveScreenManager : MonoBehaviour
    {
        static public CompetitiveScreenManager Singleton { get; internal set; }
        public GameObject playerPrefab;
        public Transform playersTransform;
        public Dictionary<ulong, PlayerManager> playerList = new Dictionary<ulong, PlayerManager>();
        // Use this for initialization
        void Awake()
        {
            Singleton = this;
        }

        private void OnDestroy()
        {
            Singleton = null;
        }

        private void OnEnable()
        {
            ResetUI();
            if (NetworkManager.Singleton.IsClient)
                StartCoroutine("UpdateClientLobby");
        }
        
        void ResetUI()
        {
            Common.DestroyChildren(playersTransform);
            playerList.Clear();
        }

        IEnumerator UpdateClientLobby()
        {
            while (true)
            {
                ResetUI();
                for (int i = 0; i < MyNetwork.Singleton.playerNames.Count; i++)
                {
                    var playerManager = GameObject.Instantiate(playerPrefab, playersTransform).GetComponent<PlayerManager>();
                    playerManager.SetName(MyNetwork.Singleton.playerNames[i].ToString());
                }
                yield return new WaitForSeconds(3f);
            }
        }
        
        public void AddPlayer(ulong clientId, PlayerData playerData)
        {
            var playerManager = GameObject.Instantiate(playerPrefab, playersTransform).GetComponent<PlayerManager>();
            playerManager.SetName(playerData.playerName);
            playerList.Add(clientId, playerManager);
            playerManager.playerData = playerData;
        }

        public void RemovePlayer(ulong clientId)
        {
            GameObject.Destroy(playerList[clientId].gameObject);
            playerList.Remove(clientId);
        }


        // Update is called once per frame
        void Update()
        {

        }
    }
}