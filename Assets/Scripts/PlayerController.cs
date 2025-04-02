using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Assets
{
    public class PlayerController : NetworkBehaviour
    {

        static public PlayerController localInstance = null;
        static public List<PlayerController> players = new List<PlayerController>();
        PlayerAIController aIController;
        public Transform cameraTransform;
        public BoxCollider letterBoxCollider;
        public GameObject homeArrowObj;
        
        public NetworkVariable<int> teamID = new NetworkVariable<int>(0);
        public NetworkVariable<int> positionID = new NetworkVariable<int>(0);
        public NetworkVariable<char> letterCollected = new NetworkVariable<char>('?');
        public NetworkVariable<FixedString32Bytes> playername;
        public NetworkVariable<FixedString32Bytes> studentId;
        public NetworkVariable<int> powerupUsed = new NetworkVariable<int>(0);
        public NetworkVariable<int> correctWordsCnt = new NetworkVariable<int>(0);
        public NetworkVariable<int> mistake = new NetworkVariable<int>(0);
        
        Dictionary<int, int> letterCollectedDic = new Dictionary<int, int>();
        
        int letterLayerMask;
        public bool isAI;

        public NetworkVariable<int> score = new NetworkVariable<int>(0);
        public List<int> letterIndexes = new List<int>();
        public GameObject[] powerUpEffects;

        PitLetterManager lastLetterManager;

        // Use this for initialization
        void Awake()
        {
            aIController = GetComponent<PlayerAIController>();
            // GetComponent<SimpleCharacterControl>().enabled = false;
            // GetComponent<ItemHoldLogic>().enabled = false;
            letterLayerMask = LayerMask.GetMask(new string[] { "Letter" });
            score.OnValueChanged += OnScoreChanged;
            players.Add(this);
        }
        
        public void OnUsePowerUp(PowerUpType powerUpType)
        {
            if(powerUpType != PowerUpType.None && powerUpEffects[(int)powerUpType].activeSelf)
            {
                return;                
            }
            for(int i = 0; i < powerUpEffects.Length; i++)
            {
                if(powerUpEffects[i] != null && powerUpEffects[i].activeSelf)
                    powerUpEffects[i].SetActive(false);
            }
            if(powerUpType != PowerUpType.None)
                powerUpEffects[(int)powerUpType].SetActive(true);
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void SetStudentIdServerRpc(string id, ServerRpcParams serverRpcParams = default)
        {
            studentId.Value = id;
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void SetPowerUpUsedServerRpc(ServerRpcParams serverRpcParams = default)
        {
            SetPowerUpUsed();
        }
        
        public void SetPowerUpUsed()
        {
            powerupUsed.Value ++;
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void SetCorrectWordCntServerRpc(ServerRpcParams serverRpcParams = default)
        {
            SetCorrectWordCnt();
        }
        
        public void SetCorrectWordCnt()
        {
            correctWordsCnt.Value ++;
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void SetMistakeServerRpc(ServerRpcParams serverRpcParams = default)
        {
            SetMistake();
        }
        
        public void SetMistake()
        {
            mistake.Value ++;
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void SetMostCollectedLetterServerRpc(char value, ServerRpcParams serverRpcParams = default)
        {
            SetMostCollectedLetter(value);
        }
        
        public void SetMostCollectedLetter(char value)
        {
            letterCollected.Value  = value;
        }
        

        public override void OnNetworkSpawn()
        {

            //if (IsOwner)
            //{
            //    GetComponent<SimpleCharacterControl>().enabled = IsOwner;
            //    GetComponent<ItemHoldLogic>().enabled = IsOwner;
            //}

            if (IsOwner)
            {
                localInstance = this;
                string _name = GameSettings.playername;
                if(_name == "")
                {
                    _name = "Player" + OwnerClientId;
                }
                SetPlayerNameServerRpc(_name);
                SetStudentIdServerRpc(GlobalData.Singleton.studentId);
                // transform.position = PlayerSpawnManager.Singleton.spawnPoints[teamID.Value].transform.position;
                // Debug.LogError(teamID.Value);
            }
            isAI = false;
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void SetPlayerNameServerRpc(string value, ServerRpcParams serverRpcParams = default)
        {
            playername.Value = value;
        }

        private void Start()
        {
            if (!isAI && (IsOwner || !NetworkManager.Singleton.IsApproved))
            {
                // GetComponent<SimpleCharacterControl>().enabled = true;//IsOwner;
                // GetComponent<ItemHoldLogic>().enabled = true;//IsOwner;

                //cameraTransform.localPosition = new Vector3(0, 1.015f, -0.032f);
                //cameraTransform.localEulerAngles = new Vector3(13.398f, 0, 0);
                if(!NetworkManager.Singleton.IsApproved)
                {
                    playername.Value = GameSettings.playername == "" ? "Player" : GameSettings.playername;
                    studentId.Value = GlobalData.Singleton.studentId;
                }
                var cinemachineVirtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
                cinemachineVirtualCamera.Follow = cameraTransform;
                homeArrowObj.transform.parent.gameObject.SetActive(true);
            }
            else
            {
                if(isAI)
                    playername.Value = "Bot " + positionID.Value;
                homeArrowObj.transform.parent.gameObject.SetActive(false);
            }
            homeArrowObj.transform.parent.gameObject.SetActive(false);
        }

        override public void OnDestroy()
        {
        }

        // Update is called once per frame
        void Update()
        {
            if(isAI || (!IsOwner && NetworkManager.Singleton.IsApproved))
                return;
                
            if(transform.position.y < -10f)
            {
                GameObject[] spawnPointObjs = GameObject.FindGameObjectsWithTag("SpawnPoint");
                transform.position = spawnPointObjs[teamID.Value].transform.position;
            }
                
            if(Input.GetKeyDown(KeyCode.H))
            {
                homeArrowObj.transform.parent.gameObject.SetActive(!homeArrowObj.transform.parent.gameObject.activeSelf);
            }
            
            // PlayerSpawnManager.Singleton.SetHomeIndex(positionID.Value);
            
            Transform target = PlayerSpawnManager.Singleton.homes[positionID.Value].transform;
            if (target != null)
            {
                Vector3 directionToTarget = target.position - transform.position;
                Quaternion rotation = Quaternion.LookRotation(directionToTarget);
                homeArrowObj.transform.rotation = rotation;
            }
            var colliders = Physics.OverlapBox(letterBoxCollider.transform.position, letterBoxCollider.size / 2, letterBoxCollider.transform.rotation, letterLayerMask);

            if (colliders.Length > 0)
            {
                // InGameHudManager.Singleton.pickUpButton.gameObject.SetActive(true);
                lastLetterManager = colliders[0].transform.parent.GetComponent<PitLetterManager>();
                OnPickUp();
            }
            else
            {
                InGameHudManager.Singleton.pickUpButton.gameObject.SetActive(false);
            }
        }

        public void OnHomeEnter(HomeManager homeManager) //home enter triggered
        {
            if(!IsOwner && NetworkManager.Singleton.IsApproved)
                return;
                
            if(isAI)
            {
                aIController.letterCnt = 0;
                aIController.target = null;
                AddScore(3);
                return;
            }
            
            if(homeManager.homeId != positionID.Value)
                return;
            
            InGameHudManager.Singleton.homeButton.gameObject.SetActive(true);
            string msg = "You got ";
            int i = 0;
            if (letterIndexes.Count > 0)
            {
                int addingScore = 0;
                foreach (var index in letterIndexes)
                {
                    i ++;
                    if(i != 1)
                    {
                        msg += ", ";
                    }
                    msg += "'" + Common.GetAlphaFromIndex(index) + "'";
                    
                    if(NetworkManager.Singleton.IsApproved && GameSettings.gameMode == EGameMode.COOPERATIVE)
                    {
                        HomeHudManager.Singleton.AddLetterServerRpc(index, localInstance.teamID.Value, true);
                    }
                    else
                    {
                        HomeHudManager.Singleton.OnAddLetter(index, localInstance.teamID.Value, true);
                    }
                    // addingScore += Common.GetScoreFromIndex(index);
                    addingScore += 1;
                    
                    if(!letterCollectedDic.ContainsKey(index))
                    {
                        letterCollectedDic[index] = 0;
                    }
                    letterCollectedDic[index]++;
                }
                letterIndexes.Clear();
                InGameHudManager.Singleton.SetLetters(letterIndexes);
                AddScore(addingScore);
                
                int max = 0;
                int maxIndex = 0;
                foreach(int wordIndex in letterCollectedDic.Keys)
                {
                    if(max < letterCollectedDic[wordIndex])
                    {
                        max = letterCollectedDic[wordIndex];
                        maxIndex = wordIndex;
                    }
                }
                if(NetworkManager.IsApproved)
                    SetMostCollectedLetterServerRpc(Common.GetAlphaFromIndex(maxIndex));
                else
                    SetMostCollectedLetter(Common.GetAlphaFromIndex(maxIndex));
            }
            
            msg += " letters";
            // if(i != 0)
            //     NotificationManager.Singleton.OnNotification(msg);
        }
        public void AddScore(int value)
        {
            if(NetworkManager.Singleton.IsApproved)
            {
                AddScoreServerRpc(value);
            }
            else
            {
                score.Value += value;                
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void AddScoreServerRpc(int value, ServerRpcParams serverRpcParams = default)
        {
            score.Value += value;
        }
        
        public void OnScoreChanged(int previous, int current)
        {
            InGameHudManager.Singleton.CalculateScores();
        }

        public void OnHomeExit() //home exit triggered
        {
            if(isAI || (!IsOwner && NetworkManager.Singleton.IsApproved))
                return;
                
            InGameHudManager.Singleton.homeButton.gameObject.SetActive(false);
        }


        public void OnPickUp() //pickup button clicked in hud
        {
            if(!IsOwner && NetworkManager.Singleton.IsApproved)
                return;
                
            if(letterIndexes.Count > GameSettings.collectableCount - 1)
                return;
                
            letterIndexes.Add(lastLetterManager.index.Value + 1);
            if (letterIndexes.Count > GameSettings.collectableCount)
                letterIndexes.RemoveAt(0);
            InGameHudManager.Singleton.SetLetters(letterIndexes);
            lastLetterManager.gameObject.SetActive(false);
            lastLetterManager.Despawn();
            
            // NotificationManager.Singleton.OnNotification("You picked a '" + Common.GetAlphaFromIndex(lastLetterManager.index.Value + 1) + "' letter");
        }

        public void OnHome() //home button clicked in hud
        {
            if(isAI || (!IsOwner && NetworkManager.Singleton.IsApproved))
                return;
            
            InGameHudManager.Singleton.gameObject.SetActive(false);
            HomeHudManager.Singleton.obj.SetActive(true);
        }
    }
}