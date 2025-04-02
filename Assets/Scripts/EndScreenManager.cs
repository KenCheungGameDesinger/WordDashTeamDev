using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using Assets;
using MiniJSON;

public class EndScreenManager : NetworkBehaviour
{
    public GameObject winObj;
    public GameObject loseObj;

    public void GameEnd(bool isWin)
    {
        if (InGameHudManager.Singleton.gameState == GameState.Ended)
            return;
        gameObject.SetActive(true);
        winObj.SetActive(isWin);
        loseObj.SetActive(!isWin);
        InGameHudManager.Singleton.gameState = GameState.Ended;
        
        if (NetworkManager.Singleton.IsConnectedClient && NetworkManager.Singleton.IsHost)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param["class_id"] = GlobalData.Singleton.classId;
            param["match_id"] = Common.GenerateUniqueID();

            List<Dictionary<string, string>> studentsData = new List<Dictionary<string, string>>();
            foreach (PlayerController player in PlayerController.players)
            {
                Dictionary<string, string> dicData = new Dictionary<string, string>();
                dicData["most_letter"] = player.letterCollected.Value.ToString();
                dicData["powerup_used"] = player.powerupUsed.Value.ToString();
                dicData["correct_words"] = player.correctWordsCnt.Value.ToString();
                dicData["mistake"] = player.mistake.Value.ToString();
                dicData["score"] = player.score.Value.ToString();
                dicData["student_id"] = player.studentId.Value.ToString();
                studentsData.Add(dicData);
            }

            param["data"] = Json.Serialize(studentsData);

            StartCoroutine(WebServices.PostTopWord(param, null));
        }

        Invoke("SetGameEndState", 3f);
    }

    void SetGameEndState()
    {
        MysteryHubManager.Singleton.gameObject.SetActive(false);
        HomeHudManager.Singleton.gameObject.SetActive(false);
        InGameHudManager.Singleton.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }

    public void LoadScene()
    {
        if (NetworkManager.Singleton.IsApproved)
        {
            SceneManager.LoadScene("EntryScene");
        }
        else
        {
            SceneManager.LoadScene("EntryScene");
        }
    }
}
