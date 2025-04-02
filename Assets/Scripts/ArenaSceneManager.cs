using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets
{
    public class ArenaSceneManager : NetworkBehaviour
    {
        static public ArenaSceneManager Singleton { get; internal set; }

        public float remainTime = GameSettings.gameTime;
        public NetworkVariable<int> remainNetworkTime = new NetworkVariable<int>(0);
        public string mysteryWord = "donkey";

        // Use this for initialization
        void Awake()
        {
            Singleton = this;
            StartCoroutine("CalcCountDown");
        }
        
        IEnumerator CalcCountDown()
        {
            yield return new WaitForSeconds(5f);
            InGameHudManager.Singleton?.SetCountDownText("5");
            yield return new WaitForSeconds(1f);
            InGameHudManager.Singleton?.SetCountDownText("4");
            yield return new WaitForSeconds(1f);
            InGameHudManager.Singleton?.SetCountDownText("3");
            yield return new WaitForSeconds(1f);
            InGameHudManager.Singleton?.SetCountDownText("2");
            yield return new WaitForSeconds(1f);
            InGameHudManager.Singleton?.SetCountDownText("1");
            yield return new WaitForSeconds(1f);
            InGameHudManager.Singleton?.SetCountDownText("GO!");
            yield return new WaitForSeconds(1f);
            InGameHudManager.Singleton?.SetCountDownText("");
            InGameHudManager.Singleton?.SetGameState(GameState.Playing);
        }

        private void FixedUpdate()
        {
            if(InGameHudManager.Singleton.gameState != GameState.Playing || ArenaHudManager.Singleton.instructionObj.activeSelf)
                return;
                
            if(NetworkManager.IsApproved && !NetworkManager.IsHost)
                return;
                
            remainTime -= Time.fixedDeltaTime;
            if(remainNetworkTime.Value != (int)remainTime && remainNetworkTime != null)
            {
                remainNetworkTime.Value = (int)remainTime;
            }
            // InGameHudManager.Singleton?.SetGameTime(remainTime);
        }

        private void Start()
        {
            remainTime = GameSettings.gameTime;
            if(!NetworkManager.IsApproved || NetworkManager.IsHost)
                remainNetworkTime.Value = GameSettings.gameTime;
            remainNetworkTime.OnValueChanged = OnRemainTimeChanged;
            // remainTime = 30;
            InGameHudManager.Singleton?.SetGameTime(remainTime);
            InGameHudManager.Singleton.SetMysteryWord(mysteryWord.Length);
        }
        
        void OnRemainTimeChanged(int previous, int current)
        {
            InGameHudManager.Singleton?.SetGameTime(current);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (!IsHost)
                return;
        }

    }
}