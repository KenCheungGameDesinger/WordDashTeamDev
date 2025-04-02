using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

namespace Assets
{
    public class HomeHudManager : NetworkBehaviour
    {
        public static HomeHudManager Singleton;
        public GameObject homeUpLetterPrefab;
        public GameObject homeDownLetterPrefab;
        public Transform homeUpGridTransform;
        public Transform homeDownGridTransform;
        public Transform wordContainer;
        public GameObject wordPrefab;
        public Button tickButton;
        public Button crossButton;
        public Text resultText;
        string mysteryWord = "donkey";
        public List<int> letterIndexes = new List<int> ();
        public List<HomeUpLetter> homeUpLetters = new List<HomeUpLetter>();
        public List<HomeDownLetter> homeDownLetters = new List<HomeDownLetter>();
        bool canFillWord = false;
        public GameObject obj;
        public GameObject wordListContainer;
        public List<string> speltWords = new List<string>();
        public Dictionary<string, string> wordOwners = new Dictionary<string, string>();

        private void Awake()
        {
            Singleton = this;
            canFillWord = true;
        }

        // Use this for initialization
        void Start()
        {
            Common.DestroyChildren(homeUpGridTransform);
            Common.DestroyChildren(homeDownGridTransform);
            int i;
            // for (i = 0; i < mysteryWord.Length; i++)
            // {
            //     var item = Instantiate(homeUpLetterPrefab, homeUpGridTransform).GetComponent<HomeUpLetter>();
            //     homeUpLetters.Add(item);
            // }
            for (i = 0; i <= 26; i++)
            {
                var item = Instantiate(homeDownLetterPrefab, homeDownGridTransform).GetComponent<HomeDownLetter>();
                item.SetIndex(i);
                homeDownLetters.Add(item);
                // if (i > 0 && Random.value < 0.5)
                // {
                //     letterIndexes.Add(i);
                //     if (Random.value < 0.3)
                //         letterIndexes.Add(i);
                // }
            }
            // letterIndexes.Add(4);
            // letterIndexes.Add(15);
            // letterIndexes.Add(14);
            // letterIndexes.Add(11);
            // letterIndexes.Add(5);
            // letterIndexes.Add(25);

            Init();
        }
        
        public void OnClickWordListButton()
        {
            wordListContainer.SetActive(!wordListContainer.activeSelf);
        }

        public void Init()
        {
            foreach (var item in homeUpLetters)
            {
                item.SetEmpty();
            }
            foreach (var item in homeDownLetters)
            {
                item.SetCount(0);
            }
            foreach (var index in letterIndexes)
            {
                homeDownLetters[index].Inc();
            }

            RefreshButtons();
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void AddLetterServerRpc(int index, int teamID, bool isAdd, ServerRpcParams serverRpcParams = default)
        {
            AddLetterClientRpc(index, teamID, isAdd);     
        }
        
        [ClientRpc]
        public void AddLetterClientRpc(int index, int teamID, bool isAdd, ClientRpcParams clientRpcParams = default)
        {
            OnAddLetter(index, teamID, isAdd);
        }
        
        public void OnAddLetter(int index, int teamID, bool isAdd)
        {
            if(teamID != PlayerController.localInstance.teamID.Value)
                return;
            if(isAdd)
                homeDownLetters[index].Inc();
            else
                homeDownLetters[index].Dec();

            RefreshButtons();
        }

        public void OnUpItemClick(HomeUpLetter upItem)
        {
            if(NetworkManager.Singleton.IsApproved)
            {
                AddLetterServerRpc(upItem.index, PlayerController.localInstance.teamID.Value, true);
            }
            else
            {
                OnAddLetter(upItem.index, PlayerController.localInstance.teamID.Value, true);
            }
            upItem.SetEmpty();
            homeUpLetters.Remove(upItem);
            GameObject.Destroy(upItem.gameObject);

            RefreshButtons();
        }
        public void OnDownItemClick(HomeDownLetter downItem)
        {
            if(!canFillWord)
                return;
            // HomeUpLetter upItem = GetFirstEmptyItem();
            // if (!upItem)
            //     return;

            // upItem.SetIndex(downItem.index);
            var item = Instantiate(homeUpLetterPrefab, homeUpGridTransform).GetComponent<HomeUpLetter>();
            homeUpLetters.Add(item);
            item.SetIndex(downItem.index);
            // downItem.Dec();
            
            if(NetworkManager.Singleton.IsApproved)
            {
                AddLetterServerRpc(downItem.index, PlayerController.localInstance.teamID.Value, false);
            }
            else
            {
                OnAddLetter(downItem.index, PlayerController.localInstance.teamID.Value, false);
            }

            RefreshButtons();            
        }


        public void OnBackClick()
        {
            InGameHudManager.Singleton.gameObject.SetActive(true);
            HomeHudManager.Singleton.obj.SetActive(false);
        }

        public void OnTickClick()
        {
            Check();
        }

        public void OnCrossClick(bool isAdd = true)
        {
            foreach (var item in homeUpLetters)
            {
                if(!item.IsEmpty() && isAdd)
                {
                    if(NetworkManager.Singleton.IsApproved)
                    {
                        AddLetterServerRpc(item.index, PlayerController.localInstance.teamID.Value, true);
                    }
                    else
                    {
                        OnAddLetter(item.index, PlayerController.localInstance.teamID.Value, true);
                    }
                }
                item.SetEmpty();
                Destroy(item.gameObject);
            }
            homeUpLetters.Clear();
            RefreshButtons();
        }


        HomeUpLetter GetFirstEmptyItem()
        {
            foreach (var item in homeUpLetters)
            {
                if (item.IsEmpty())
                    return item;
            }
            return null;
        }
        
        string checkingWrod;

        void Check()
        {
            canFillWord = false;
            foreach (var item in homeUpLetters)
            {
                if (item.IsEmpty())
                    return;
            }
            
            checkingWrod = "";
            foreach(var item in homeUpLetters)
            {
                checkingWrod += Common.GetAlphaFromIndex(item.index);
            }
            StartCoroutine(WebServices.CheckWord(checkingWrod, OnCheckWord));

            // for (int i = 0; i < mysteryWord.Length; i++)
            // {
            //     var item = homeUpLetters[i];
            //     var c = Common.GetAlphaFromIndex(item.index);
            //     if (mysteryWord[i] == c)
            //         item.SetState(ELetterState.GOOD);
            //     else if (mysteryWord.Contains(c))
            //         item.SetState(ELetterState.SPELL);
            //     else
            //         item.SetState(ELetterState.BAD);
            // }
        }
        
        void SetVisibleButtons(bool isVisible)
        {
            tickButton.gameObject.SetActive(isVisible);
            crossButton.gameObject.SetActive(isVisible);            
        }
        
        public void OnCheckWord(bool isCorrect)
        {
            SetVisibleButtons(false);
            int score = 0;
            foreach(var item in homeUpLetters)
            {
                item.SetState(isCorrect ? ELetterState.GOOD : ELetterState.BAD);
                score += Common.GetScoreFromIndex(item.index);
            }
            if(isCorrect)
            {
                if(speltWords.Contains(checkingWrod) && wordOwners[checkingWrod] == PlayerController.localInstance.playername.Value)
                {
                    score = 0;
                    resultText.text = "You already spelt this word!";
                    isCorrect = false;
                }
                else
                {
                    resultText.text = "You made a correct word!\nYou got " + score + " points!";
                    if(NetworkManager.IsApproved)
                        PlayerController.localInstance?.SetCorrectWordCntServerRpc();
                    else
                        PlayerController.localInstance?.SetCorrectWordCnt();
                    PlayerController.localInstance?.AddScore(score);
                }
            }
            else
            {
                resultText.text = "Wrong! Please try again!";
                if(NetworkManager.IsApproved)
                {
                    PlayerController.localInstance?.SetMistakeServerRpc();
                }
                else
                {
                    PlayerController.localInstance?.SetMistake();
                }
            }
            resultText.gameObject.SetActive(true);
            StartCoroutine(OnCheckWordComplete(isCorrect));
        }
        
        IEnumerator OnCheckWordComplete(bool isCorrect)
        {
            yield return new WaitForSeconds(3f);
            canFillWord = true;
            SetVisibleButtons(true);
            resultText.gameObject.SetActive(false);
            if(isCorrect)
            {
                GameObject obj = Instantiate(wordPrefab, wordContainer);
                obj.GetComponent<TMPro.TMP_Text>().text = checkingWrod;
                OnCrossClick(false);
                if(NetworkManager.Singleton.IsApproved)
                    AddSpeltWordServerRpc(checkingWrod, PlayerController.localInstance.playername.Value.ToString());
                else
                {
                    speltWords.Add(checkingWrod);
                    wordOwners[checkingWrod] = PlayerController.localInstance.playername.Value.ToString();
                }
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void AddSpeltWordServerRpc(string word, string playerName, ServerRpcParams serverRpcParams = default)
        {
            AddSpeltWordClientRpc(word, playerName);
        }
        
        [ClientRpc]
        public void AddSpeltWordClientRpc(string word, string playerName, ClientRpcParams clientRpcParams = default)
        {
            if(speltWords.Contains(word))
                return;
            speltWords.Add(word);
            wordOwners[word] = playerName;
        }

        void RefreshButtons()
        {
            int cnt = 0;
            foreach (var item in homeUpLetters)
            {
                if (!item.IsEmpty())
                    cnt++;
            }
            tickButton.interactable = cnt > 0;
            crossButton.interactable = cnt > 0;
        }
    }
}