using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Assets;
using TMPro;

public class MysteryHubManager : MonoBehaviour
{
    public static MysteryHubManager Singleton;
    
    public RectTransform selectedWordImg;
    
    public GameObject hintObj;
    
    public TMP_Text hintTxt;
    
    public Transform wordContainer;
    
    public GameObject wordPrefab;
    
    public Text foundsText;
    
    public List<MysteryWordItem> words = new List<MysteryWordItem>();
    
    private void Awake()
    {
        Singleton = this;
    }
    
    void Start()
    {
        InitWords();
    }
    
    void InitWords()
    {
        foreach(MysteryWord word in GlobalData.Singleton.mysteryWords)
        {
            GameObject wordObj = Instantiate(wordPrefab, wordContainer);
            MysteryWordItem wordItem = wordObj.GetComponent<MysteryWordItem>();
            wordItem.wordData = word;
            wordItem.InitWord(word.eWord);
            words.Add(wordItem);
        }
        DeSelectWords();
    }
    
    public void CompleteWord()
    {
        int count = 0;
        foreach(MysteryWordItem wordItem in words)
        {
            if(wordItem.isCompleted)
            {
                count++;
            }
        }
        if(count != 0)
        {
            foundsText.text = "You found " + count + " mystery words!";
        }
        // if(count == GlobalData.Singleton.mysteryWords.Count)
        // {
        //     if(NetworkManager.Singleton.IsApproved)
        //         MyNetwork.Singleton.GameEndServerRpc(GlobalData.Singleton.email, PlayerController.localInstance.teamID.Value);
        //     else
        //         InGameHudManager.Singleton.endScreen.GameEnd(true, 0);
        // }
    }
    
    public void DeSelectWords()
    {
        foreach(MysteryWordItem wordItem in words)
        {
            wordItem.isSelected = false;
        }
        selectedWordImg.gameObject.SetActive(false);
    }
    
    public void WordClicked(GameObject item)
    {
        DeSelectWords();
        selectedWordImg.anchoredPosition = item.GetComponent<RectTransform>().anchoredPosition;
        selectedWordImg.gameObject.SetActive(true);
        item.GetComponent<MysteryWordItem>().isSelected = true;
    }
}
