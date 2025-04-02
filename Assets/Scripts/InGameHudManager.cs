using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.Netcode;

namespace Assets
{
    public enum PowerUpType
    {
        SpeedUp,
        Cloud,
        JumpUp,
        Banana,
        Shield,
        Key,
        None
    }
    
    public enum GameState
    {
        PrePlaying,
        Playing,
        Ended        
    }
    
    public class InGameHudManager : MonoBehaviour
    {
        public static InGameHudManager Singleton;
        
        public EndScreenManager endScreen;
        public GameObject menuBoard;
        public GameObject leaderBoard;
        public GameObject teamScoreBoard;
        
        public Button pickUpButton;
        public Button homeButton;

        public List<Image> letterImages;

        public TextMeshProUGUI topTeamScoreLabel;
        public TextMeshProUGUI topScoreLabel;
        public TextMeshProUGUI myTeamScoreLabel;
        public TextMeshProUGUI teamMembersLabel;
        public TextMeshProUGUI myScoreLabel;
        public TextMeshProUGUI mysteryWordLabel;
        public TextMeshProUGUI timeLabel;
        public TextMeshProUGUI countdownText;
        
        public Sprite[] powerUpSprites;
        public Image powerUpIcon;
        public Image powerUpCycle;
        public GameObject powerUpObj;
        public Slider freezeSlider;
        float powerUpTime = 10f;
        float powerUpMaxTime = 10f;
        float freezeTime = 10f;
        float freezeMaxTime = 10f;
        public bool isFreeze = false;
        public bool isPowerUpUsing = false;
        public int myScore = 0, myTeamScore = 0;
        public int topScore = 0, topTeamScore = 0;
        public string topScoreName = "", topTeamScoreName = "";
        public PowerUpType powerUpType = PowerUpType.None;
        public GameState gameState;

        private void Awake()
        {
            Singleton = this;

            pickUpButton.gameObject.SetActive(false);
            homeButton.gameObject.SetActive(false);
            SetLetters(new List<int>());
            SetTopScore(0);
            SetMyScore(0);
            SetTopTeamScore(0);
            SetMyTeamScore(0);
            PickPowerUpItem(PowerUpType.None);
            gameState = GameState.PrePlaying;
            if((EGameMode)MyNetwork.Singleton.gameMode.Value == EGameMode.COOPERATIVE)
            {
                teamScoreBoard.SetActive(true);
            }
            else
            {
                teamScoreBoard.SetActive(false);
            }
        }
        
        public void SetGameState(GameState state)
        {
            gameState = state;
        }
        
        public void PickPowerUpItem(PowerUpType type)
        {
            powerUpType = type;
            if(type != PowerUpType.None)
            {
                // NotificationManager.Singleton.OnNotification("You picked a Power Up item");
                powerUpIcon.sprite = powerUpSprites[(int)type];
                powerUpIcon.GetComponent<Button>().interactable = true;
                powerUpObj.SetActive(true);
            }
            else
            {
                isPowerUpUsing = false;
                powerUpTime = powerUpMaxTime;
                powerUpObj.SetActive(false);
            }
        }
        
        public void OnUsePowerUp()
        {
            if(powerUpType == PowerUpType.None)
                return;
                
            isPowerUpUsing = true;
            powerUpIcon.GetComponent<Button>().interactable = false;
            if(NetworkManager.Singleton.IsApproved)
                PlayerController.localInstance?.SetPowerUpUsedServerRpc();
            else
                PlayerController.localInstance?.SetPowerUpUsed();
                
            if(powerUpType == PowerUpType.Banana)
            {
                MyNetwork.Singleton.SetFreezeServerRpc(PlayerController.localInstance.positionID.Value);
            }
        }
        
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.E))
            {
                OnUsePowerUp();
            }
            
            EventSystem.current.SetSelectedGameObject(null);
            if(Input.GetKey(KeyCode.Tab) || gameState == GameState.Ended)
            {
                leaderBoard.SetActive(true);
            }
            else
            {
                leaderBoard.SetActive(false);
            }
            
            if(Input.GetKeyDown(KeyCode.Escape) && !NetworkManager.Singleton.IsConnectedClient)
            {
                menuBoard.SetActive(!menuBoard.activeSelf);
            }
            
            if(isFreeze)
            {
                if(freezeTime < 0)
                {
                    isFreeze = false;
                    freezeTime = freezeMaxTime;
                    freezeSlider.gameObject.SetActive(false);
                }
                else
                {
                    freezeSlider.gameObject.SetActive(true);
                    freezeSlider.value = freezeTime / freezeMaxTime;
                }
                freezeTime -= Time.deltaTime;
                PlayerController.localInstance?.OnUsePowerUp(PowerUpType.Banana);
            }
            else if(isPowerUpUsing)
            {
                if(powerUpTime < 0)
                {
                    PickPowerUpItem(PowerUpType.None);
                    powerUpCycle.fillAmount = 0;
                }
                else
                {
                    powerUpCycle.fillAmount = powerUpTime / powerUpMaxTime;
                }
                powerUpTime -= Time.deltaTime;
                PlayerController.localInstance?.OnUsePowerUp(powerUpType);
            }
            else
            {
                PlayerController.localInstance?.OnUsePowerUp(PowerUpType.None);
            }
        }

        public void OnPickUpButton()
        {
            PlayerController.localInstance?.OnPickUp();
        }
        
        public void OnMysteryButton()
        {
            MysteryHubManager.Singleton.gameObject.SetActive(true);
        }

        public void OnHomeButton()
        {
            PlayerController.localInstance?.OnHome();
        }
        
        public void DiscardLetter(int index)
        {
            PlayerController.localInstance?.letterIndexes.Remove(PlayerController.localInstance.letterIndexes[index]);
            SetLetters(PlayerController.localInstance?.letterIndexes);
        }

        public void SetLetters(List<int> data)
        {
            for (int i = 0; i < GameSettings.collectableCount; i++)
            {
                if (i < data.Count)
                {
                    letterImages[i].gameObject.SetActive(true);
                    letterImages[i].sprite = ArenaHudManager.Singleton.letterSprites[data[i]];
                }
                else 
                {
                    letterImages[i].gameObject.SetActive(false);
                }
            }
        }
        
        public void SetTopScore(int score, string topName = "")
        {
            topScoreLabel.text = "Top Score: " + score + (topName == "" ? "" : "\n(" + topName + ")");
            if(PlayerController.localInstance != null && score == PlayerController.localInstance.score.Value && topScoreName != PlayerController.localInstance.playername.Value)
            {
                AlertManager.singleton.OnAlert("YOU ARE THE TOP SCORER");
            }
            topScore = score;
            topScoreName = topName;
        }

        public void SetMyTeamScore(int score)
        {
            if((EGameMode)MyNetwork.Singleton.gameMode.Value == EGameMode.COOPERATIVE)
            {
                myTeamScoreLabel.text = "Team Score: " + score;
                myTeamScore = score;
            }
        }

        public void SetMyScore(int score)
        {
            myScore = score;
            if((EGameMode)MyNetwork.Singleton.gameMode.Value == EGameMode.COOPERATIVE)
            {
                myScoreLabel.text = "My Score: " + score;
            }
            else
            {
                topTeamScoreLabel.text = "My Score: " + score;
            }
        }

        public void SetTopTeamScore(int score, string topName = "")
        {
            if((EGameMode)MyNetwork.Singleton.gameMode.Value == EGameMode.COOPERATIVE)
            {
                if(PlayerController.localInstance != null && topName == "Team " + PlayerController.localInstance.teamID.Value && topTeamScoreName != "Team " + PlayerController.localInstance.teamID.Value)
                {
                    AlertManager.singleton.OnAlert("YOUR TEAM IS IN THE LEAD");
                }
                topTeamScore = score;
                topTeamScoreName = topName;
                topTeamScoreLabel.text = "Winning Team: " + topName + "\n" + score + " points";
            }
        }
        
        public void CalculateScores()
        {
            int myTeamScore = 0;
            int topScore = -1, topTeamScore = -1;
            string topName = "", topTeamName = "", teamMembers = "";
            int myScore = PlayerController.localInstance.score.Value;
            if ((EGameMode)MyNetwork.Singleton.gameMode.Value == EGameMode.COOPERATIVE)
            {
                Dictionary<int, int> teamScore = new Dictionary<int, int>();
                int myTeamId = PlayerController.localInstance.teamID.Value;
                foreach(PlayerController player in PlayerController.players)
                {
                    if(player != null)
                    {
                        if(topScore < player.score.Value)
                        {
                            topScore = player.score.Value;
                            topName = player.playername.Value.ToString();
                        }
                        if(player.teamID.Value == myTeamId)
                        {
                            teamMembers = teamMembers + (teamMembers == "" ? "" : "\n") + player.playername.Value;
                            myTeamScore += player.score.Value;
                        }
                        if(!teamScore.ContainsKey(player.teamID.Value))
                        {
                            teamScore[player.teamID.Value] = 0;
                        }
                        teamScore[player.teamID.Value] += player.score.Value;
                    }
                }
                foreach(int teamID in teamScore.Keys)
                {
                    if(topTeamScore < teamScore[teamID])
                    {
                        topTeamScore = teamScore[teamID];
                        topTeamName = "Team " + teamID;
                    }
                }
                SetMyScore(myScore);
                SetTopScore(topScore, topName);
                SetTopTeamScore(topTeamScore, topTeamName);
                SetMyTeamScore(myTeamScore);
                teamMembersLabel.text = teamMembers;
            }
            else
            {
                foreach(PlayerController player in PlayerController.players)
                {
                    if(player != null)
                    {
                        if(topScore < player.score.Value)
                        {
                            topScore = player.score.Value;
                            topName = player.playername.Value.ToString();
                        }
                    }
                }
                SetMyScore(myScore);
                SetTopScore(topScore, topName);
            }
        }
        
        public void SetCountDownText(string text)
        {
            countdownText.text = text;
        }

        public void SetGameTime(float t)
        {
            int v = Mathf.CeilToInt(t);
            
            timeLabel.text = "Time: " + String.Format("{0}:{1:00}", v / 60, v % 60);
            
            if(v < 0 && gameState != GameState.Ended)
            {
                if((EGameMode)MyNetwork.Singleton.gameMode.Value == EGameMode.COOPERATIVE)
                {
                    endScreen.GameEnd(myTeamScore == topTeamScore);
                }
                else
                {
                    endScreen.GameEnd(myScore == topScore);                    
                }
            }
        }

        public void SetMysteryWord(int cnt)
        {
            string str = "_";
            for (int i = 1; i < cnt; i++)
                str += "  _";
            mysteryWordLabel.text = str;
        }

    }
}