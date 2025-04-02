using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets
{
    public class SinglePlayerScreenManager : MonoBehaviour
    {
        public static SinglePlayerScreenManager singleton;
        public GameObject playerPrefab;
        public Transform playersTransform;
        public List<PlayerManager> players = new List<PlayerManager>();
        public int playerCount = 0;
        
        // Use this for initialization
        void Awake()
        {
            singleton = this;
        }
        
        void Start()
        {
            Common.DestroyChildren(playersTransform);
            players.Clear();

            for (int i = 0; i <= 7; i++)
            {
                var playerManager = GameObject.Instantiate(playerPrefab, playersTransform).GetComponent<PlayerManager>();
                playerManager.SetName("Player " + i);
                if (i == 0)
                {
                    playerManager.SetName("You");
                }
                else
                {
                    playerManager.SetName("Bot " + i);
                    playerManager.nameLabel.color = new Color32(209, 13, 0, 255);
                }
                players.Add(playerManager);
            }

            Init(1);
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void Init(int botCount)
        {
            playerCount = botCount + 1;
            for (int i = 1; i <= 7; i++)
            {
                players[i].gameObject.SetActive(i <= botCount);
            }
        }

        public void OnChangeBotCount(int index)
        {
            Init(index + 1);
        }
    }
}