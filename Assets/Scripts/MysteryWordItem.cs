using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Assets;
public class MysteryWordItem : MonoBehaviour
{
    public GameObject letterPrefab;
    public Transform letterColntainer;
    public TooTipItem chineseHint;
    public TooTipItem defnitionObj;
    
    public List<HomeUpLetter> letters = new List<HomeUpLetter>();
    
    public bool isSelected = false;
    
    bool isFilled = false;
    
    public bool isCompleted = false;
    
    string mysteryWord = "";
    
    public MysteryWord wordData;
    
    public void OnItemClick()
    {
        if(isCompleted)
            return;
        MysteryHubManager.Singleton.WordClicked(this.gameObject);
    }
    
    public void InitWord(string word)
    {
        mysteryWord = word;
        for(int i = 0; i < word.Length; i++)
        {
            GameObject letterObj = Instantiate(letterPrefab, letterColntainer);
            HomeUpLetter item = letterObj.GetComponent<HomeUpLetter>();
            item.SetEmpty();
            letters.Add(item);
            if(wordData.hintPos.Contains(i))
            {
                item.SetIndex(wordData.eWord[i] - 'a' + 1);
            }
        }
        chineseHint.msg = wordData.cWord;
        defnitionObj.msg = wordData.description;
    }
    
    HomeUpLetter GetFirstEmptyItem()
    {
        foreach (var item in letters)
        {
            if (item.IsEmpty())
                return item;
        }
        return null;
    }
    
    HomeUpLetter GetLastNotEmptyItem()
    {
        for(int i = letters.Count - 1; i >= 0; i--)
        {
            if (!letters[i].IsEmpty() && !wordData.hintPos.Contains(i))
                return letters[i];
        }
        return null;
    }
    
    bool CheckWord()
    {
        bool isCorrect = true;
        for (int i = 0; i < mysteryWord.Length; i++)
        {
            var item = letters[i];
            var c = Common.GetAlphaFromIndex(item.index);
            if (mysteryWord[i] == c)
                item.SetState(ELetterState.GOOD);
            else if (mysteryWord.Contains(c))
            {
                isCorrect = false;
                item.SetState(ELetterState.SPELL);
            }
            else
            {
                isCorrect = false;
                item.SetState(ELetterState.BAD);
            }
        }
        if(!isCorrect && wordData.isChineseHint)
        {
            chineseHint.gameObject.SetActive(true);
        }
        return isCorrect;
    }
    
    void Update()
    {
        if (isSelected && !isCompleted)
        {
            if(!isFilled && Input.inputString.Length > 0)
            {
                // Get the first character of the input string
                char keyPressed = Input.inputString[0];

                // Check if the pressed key is an uppercase letter ('A' to 'Z')
                if(keyPressed >= 'A' && keyPressed <= 'Z')
                    keyPressed = (char)(keyPressed - 'A' + 'a');
                    
                if (keyPressed >= 'a' && keyPressed <= 'z')
                {
                    HomeUpLetter letter = GetFirstEmptyItem();
                    letter.SetIndex(keyPressed - 'a' + 1);
                    if(GetFirstEmptyItem() == null)
                    {
                        if(CheckWord())
                        {
                            if(NetworkManager.Singleton.IsApproved)
                                PlayerController.localInstance?.SetCorrectWordCntServerRpc();
                            else
                                PlayerController.localInstance?.SetCorrectWordCnt();
                            isCompleted = true;
                            PlayerController.localInstance?.AddScore(15);
                            MysteryHubManager.Singleton.CompleteWord();
                            MysteryHubManager.Singleton.DeSelectWords();
                        }
                        else
                        {
                            if(NetworkManager.Singleton.IsApproved)
                            {
                                PlayerController.localInstance?.SetMistakeServerRpc();
                            }
                            else
                            {
                                PlayerController.localInstance?.SetMistake();
                            }
                        }
                        isFilled = true;
                    }
                }
            }
            if(Input.GetKeyDown(KeyCode.Backspace))
            {
                HomeUpLetter letter = GetLastNotEmptyItem();
                if(letter != null)
                {
                    letter.SetEmpty();
                    isFilled = false;
                    foreach(HomeUpLetter letter1 in letters)
                    {
                        letter1.SetState(ELetterState.NONE);
                    }
                }
            }
        }
    }
}
