using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using WebSocketSharp;
using Unity.Services.Lobbies;
using Unity.Services.Authentication;

namespace Assets
{
    [Serializable]
    public class ConnectionPayload
    {
        public string playerName;
        public int playerId;
    }

    public class PlayerData
    {
        public string playerName = "Donkey";
        public int characterId = 0;
        public int teamID = 0;
        public bool isReady = false;
    }

    public class MyNetwork : NetworkBehaviour
    {
        static public MyNetwork Singleton { get; internal set; }

        public Dictionary<ulong, PlayerData> playerDatas = new Dictionary<ulong, PlayerData>();
        public NetworkList<FixedString32Bytes> playerNames = new NetworkList<FixedString32Bytes>();
        public NetworkList<int> playerTeams = new NetworkList<int>();
        public int readyCount;
        public int totalCount;
        public NetworkVariable<int> gameMode = new NetworkVariable<int>(0);
        public ConnectionPayload lastConnectionPayload;
        public GameObject[] playerPrefabs;

        private void Awake()
        {
            if(Singleton != null)
            {
                Destroy(this.gameObject);
                return;
            }
            Singleton = this;
            DontDestroyOnLoad(gameObject);
        }
        // Use this for initialization
        void Start()
        {
            NetworkManager.NetworkConfig.ConnectionApproval = true;
            
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
            NetworkManager.Singleton.OnClientStarted += OnClientStarted;
            NetworkManager.Singleton.OnClientStopped += OnClientStopped;
            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
            NetworkManager.Singleton.OnServerStopped += OnServerStopped;
            NetworkManager.Singleton.OnTransportFailure += OnTransportFailure;

            NetworkManager.ConnectionApprovalCallback += ApprovalCheck;
        }

        public void SetPlayerName(string playerName)
        {
            var payload = JsonUtility.ToJson(new ConnectionPayload()
            {
                playerId = 0,
                playerName = playerName,
            });
            var payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);

            NetworkManager.NetworkConfig.ConnectionData = payloadBytes;
        }

        public void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            var connectionData = request.Payload;
            var clientId = request.ClientNetworkId;
            

            if (NetworkManager.ConnectedClients.Count >= 8 || EntrySceneManager.Singleton.currentScreenName == "ChooseCharacterScreen")
            {
                // If connectionData too high, deny immediately to avoid wasting time on the server. This is intended as
                // a bit of light protection against DOS attacks that rely on sending silly big buffers of garbage.
                response.Approved = false;
                return;
            }

            var payload = System.Text.Encoding.UTF8.GetString(connectionData);
            var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);

            lastConnectionPayload = connectionPayload;

            response.Approved = true;
            response.CreatePlayerObject = false;//true;
            response.Position = Vector3.zero;
            response.Rotation = Quaternion.identity;
        }
        
        public void ChangePlayerTeam(string playerName, int teamID)
        {
            int index = playerNames.IndexOf(playerName);
            playerTeams[index] = teamID;
        }
       
        private void OnClientConnectedCallback(ulong clientId)
        {
            UnityEngine.Debug.Log("OnClientConnectedCallback: " + clientId);
            
            if (NetworkManager.Singleton.IsHost)
            {
                var playerData = new PlayerData();
                if (lastConnectionPayload == null || lastConnectionPayload.playerName.IsNullOrEmpty())
                    playerData.playerName = "Player" + clientId;
                else
                    playerData.playerName = lastConnectionPayload.playerName;
                playerDatas.Add(clientId, playerData);
                playerNames.Add(playerData.playerName);
                playerTeams.Add(0);

                if (GameSettings.gameMode == EGameMode.COMPETITIVE)
                    CompetitiveScreenManager.Singleton.AddPlayer(clientId, playerData);
                else if (GameSettings.gameMode == EGameMode.COOPERATIVE)
                    CooperativeScreenManager.Singleton.AddPlayer(clientId, playerData);
            }
            else
            {
                if(gameMode.Value == (int)EGameMode.COMPETITIVE)
                    EntrySceneManager.Singleton.SetScreen("CompetitiveScreen");
                if(gameMode.Value == (int)EGameMode.COOPERATIVE)
                    EntrySceneManager.Singleton.SetScreen("CooperativeScreen");
            }

        }
        
        private void OnClientDisconnectCallback(ulong clientId)
        {
            UnityEngine.Debug.Log("OnClientDisconnectCallback: " + clientId);

            if(NetworkManager.IsHost)
            {
                if(playerDatas.ContainsKey(clientId))
                {
                    int nameIndex = playerNames.IndexOf(playerDatas[clientId].playerName);
                    playerNames.Remove(playerNames[nameIndex]);
                    playerTeams.Remove(playerTeams[nameIndex]);
                    playerDatas.Remove(clientId);
                }
                if (GameSettings.gameMode == EGameMode.COMPETITIVE)
                {
                    CompetitiveScreenManager.Singleton?.RemovePlayer(clientId);
                }
                else if (GameSettings.gameMode == EGameMode.COOPERATIVE)
                {
                    CooperativeScreenManager.Singleton?.RemovePlayer(clientId);
                }
            }
        }

        private void OnClientStarted()
        {
            UnityEngine.Debug.Log("OnClientStarted");
            NetworkManager.Singleton.SceneManager.OnSceneEvent += OnSceneEvent;
        }

        private void OnServerStarted()
        {
            gameMode.Value = (int)GameSettings.gameMode;
            playerNames.Clear();
            playerTeams.Clear();
            UnityEngine.Debug.Log("OnServerStarted");
        }

        private void OnServerStopped(bool obj)
        {
            UnityEngine.Debug.Log("OnServerStopped" + obj);
            playerDatas.Clear();
            // await Lobbies.Instance.RemovePlayerAsync(GameSettings.lobbyId, AuthenticationService.Instance.PlayerId);
        }

        private void OnClientStopped(bool obj)
        {
            UnityEngine.Debug.Log("OnClientStopped_" + obj);
            if(InGameHudManager.Singleton != null)
            {
                if((EGameMode)MyNetwork.Singleton.gameMode.Value == EGameMode.COOPERATIVE)
                {
                    InGameHudManager.Singleton?.endScreen.GameEnd(InGameHudManager.Singleton.myTeamScore == InGameHudManager.Singleton.topTeamScore);
                }
                else
                {
                    InGameHudManager.Singleton?.endScreen.GameEnd(InGameHudManager.Singleton.myScore == InGameHudManager.Singleton.topScore);
                }
            }
            if(EntrySceneManager.Singleton != null)
                EntrySceneManager.Singleton?.SetScreen("OpenScreen");
        }

        private void OnTransportFailure()
        {
            UnityEngine.Debug.Log("OnTransportFailure");
        }

        private void OnSceneEvent(SceneEvent sceneEvent)
        {
            if (!IsHost)
                return;
            switch (sceneEvent.SceneEventType)
            {
                case SceneEventType.LoadEventCompleted:
                    InitGameScene();
                    break;
            }
        }

        [Obsolete]
        void InitGameScene()
        {
            int i = 0;
            GameObject[] spawnPointObjs = GameObject.FindGameObjectsWithTag("SpawnPoint");
            int posID = 0;
            foreach (var item in playerDatas)
            {
                if(GameSettings.gameMode == EGameMode.COOPERATIVE)
                {
                    Vector3 offset = new(UnityEngine.Random.RandomRange(-3f, 3f), 0, UnityEngine.Random.RandomRange(-5f, 5f));
                    var obj = Instantiate(playerPrefabs[item.Value.characterId], spawnPointObjs[item.Value.teamID].transform.position + offset, Quaternion.identity);
                    obj.GetComponent<NetworkObject>().SpawnWithOwnership(item.Key);
                    obj.GetComponent<PlayerController>().teamID.Value = item.Value.teamID;
                    obj.GetComponent<PlayerController>().positionID.Value = item.Value.teamID;
                }
                else
                {
                    Vector3 offset = new(UnityEngine.Random.RandomRange(-3f, 3f), 0, UnityEngine.Random.RandomRange(-5f, 5f));
                    var obj = Instantiate(playerPrefabs[item.Value.characterId], spawnPointObjs[posID].transform.position + offset, Quaternion.identity);
                    obj.GetComponent<NetworkObject>().SpawnWithOwnership(item.Key);
                    obj.GetComponent<PlayerController>().positionID.Value = posID;
                }
                posID ++;
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void SetFreezeServerRpc(int id, ServerRpcParams serverRpcParams = default)
        {
            SetFreezeClientRpc(id);
        }
        
        [ClientRpc]
        public void SetFreezeClientRpc(int id, ClientRpcParams clientRpcParams = default)
        {
            foreach(PlayerController player in PlayerController.players)
            {
                if(player.IsOwner && id != player.positionID.Value
                 && (!InGameHudManager.Singleton.isPowerUpUsing || InGameHudManager.Singleton.powerUpType != PowerUpType.Shield))
                {
                    InGameHudManager.Singleton.isFreeze = true;                
                }
            }
        }


        [ClientRpc]
        public void GoChooseCharacterScreenClientRpc(int pTotalCount, ClientRpcParams clientRpcParams = default)
        {
            totalCount = pTotalCount;
            readyCount = 0;
            EntrySceneManager.Singleton.SetScreen("ChooseCharacterScreen");
        }

        [ClientRpc]
        private void IncReadyClientRpc(ClientRpcParams clientRpcParams = default)
        {
            readyCount++;
            ChooseCharacterScreenManager.Singleton.RefreshReadyLabel();
            if (readyCount == totalCount && NetworkManager.IsHost)
            {
                NetworkManager.Singleton.SceneManager.LoadScene("GameScene" + GameSettings.locationId, UnityEngine.SceneManagement.LoadSceneMode.Single);
                // Debug.LogError(status);
                // Debug.LogError(NetworkManager.SceneManager.ClientSynchronizationMode);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetReadyServerRpc(int characterId, ServerRpcParams serverRpcParams = default)
        {
            var clientId = serverRpcParams.Receive.SenderClientId;
            if (!playerDatas.ContainsKey(clientId) || playerDatas[clientId].isReady)
                return;

            playerDatas[clientId].isReady = true;
            playerDatas[clientId].characterId = characterId;

            IncReadyClientRpc();
        }

        // [ServerRpc(RequireOwnership = false)]
        // public void GameEndServerRpc(string email, int teamID, ServerRpcParams serverRpcParams = default)
        // {
        //     GameEndClientRpc(email, teamID);
        // }

        // [ClientRpc]
        // private void GameEndClientRpc(string email, int teamID, ClientRpcParams clientRpcParams = default)
        // {
        //     InGameHudManager.Singleton.endScreen.GameEnd(GlobalData.Singleton.email == email, teamID);
        // }
    }
}