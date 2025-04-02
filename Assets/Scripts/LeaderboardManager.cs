using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets;
using TMPro;

public class LeaderboardManager : MonoBehaviour
{
    public Transform itemContainer;
    public Transform mysteryWordContainer;
    public Transform speltWordContainer;
    public List<LeaderboardItem> itemList = new List<LeaderboardItem>();
    public GameObject exitButton;
    public GameObject leaderboardMysteryWordPrefab;
    public GameObject leaderboardSpeltWordPrefab;
    public GameObject wordsContainer;
    public GameObject teamBoard;
    public TextMeshProUGUI[] teamScores;
    
    public void OnEnable()
    {
        int i = 0;
        PlayerController.players.Sort((p1, p2) => p2.score.Value.CompareTo(p1.score.Value));
        Dictionary<int, int> teamScore = new Dictionary<int, int>();
        for(; i < PlayerController.players.Count; i++)
        {
            if((EGameMode)MyNetwork.Singleton.gameMode.Value == EGameMode.COOPERATIVE)
            {
                itemList[i].username.text = PlayerController.players[i].playername.Value.ToString() + " (Team " + PlayerController.players[i].teamID.Value + ")";
                if(!teamScore.ContainsKey(PlayerController.players[i].teamID.Value))
                {
                    teamScore[PlayerController.players[i].teamID.Value] = 0;
                }
                teamScore[PlayerController.players[i].teamID.Value] += PlayerController.players[i].score.Value;
            }
            else
            {
                itemList[i].username.text = PlayerController.players[i].playername.Value.ToString();
            }
            itemList[i].letterCollected.text = PlayerController.players[i].letterCollected.Value.ToString();
            itemList[i].powerUpUsed.text = PlayerController.players[i].powerupUsed.Value.ToString();
            itemList[i].mysteryWordCount.text = PlayerController.players[i].correctWordsCnt.Value.ToString();
            itemList[i].mistake.text = PlayerController.players[i].mistake.Value.ToString();
            itemList[i].score.text = PlayerController.players[i].score.Value.ToString();
            itemList[i].gameObject.SetActive(true);
            itemList[i].otherBack.SetActive(PlayerController.players[i] != PlayerController.localInstance);
            itemList[i].mineBack.SetActive(PlayerController.players[i] == PlayerController.localInstance);
        }
        for(; i < itemList.Count; i++)
        {
            itemList[i].gameObject.SetActive(false);
        }
        if((EGameMode)MyNetwork.Singleton.gameMode.Value == EGameMode.COOPERATIVE)
        {
            teamBoard.SetActive(true);
            i = 0;
            foreach(int teamID in teamScore.Keys)
            {
                teamScores[i].text = "Team " + teamID + " : " + teamScore[teamID];
                teamScores[i].gameObject.SetActive(true);
                i++;
            }
            for(; i < 4; i++)
            {
                teamScores[i].gameObject.SetActive(false);
            }
        }
        else
        {
            teamBoard.SetActive(false);
        }
    }
    
    void Update()
    {
        if(InGameHudManager.Singleton != null && InGameHudManager.Singleton.gameState == GameState.Ended)
        {
            if(!wordsContainer.activeSelf)
            {
                foreach(MysteryWord mysteryWord in GlobalData.Singleton.mysteryWords)
                {
                    GameObject word = Instantiate(leaderboardMysteryWordPrefab, mysteryWordContainer);
                    word.GetComponent<TMP_Text>().text = mysteryWord.eWord;
                }
                HomeHudManager.Singleton.speltWords.Sort((p1, p2) => Common.GetScoreFromString(p2).CompareTo(Common.GetScoreFromString(p1)));
                for(int i = 0; i < Mathf.Min(3, HomeHudManager.Singleton.speltWords.Count); i++)
                {
                    string speltWord = HomeHudManager.Singleton.speltWords[i];
                    GameObject word = Instantiate(leaderboardSpeltWordPrefab, speltWordContainer);
                    word.GetComponent<TMP_Text>().text = speltWord + "\n(" + (HomeHudManager.Singleton.wordOwners.ContainsKey(speltWord) ? HomeHudManager.Singleton.wordOwners[speltWord] : "") + ")";
                }
            }
            exitButton.SetActive(true);
            wordsContainer.SetActive(true);
        }
        else
        {
            exitButton.SetActive(false);
            wordsContainer.SetActive(false);
        }
    }
}
