using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets
{
    public class SettingsScreenManager : MonoBehaviour
    {
        public TMP_InputField playerNameInput;
        public Toggle tutorialToggle;
        // Use this for initialization
        void Start()
        {
            var str = PlayerPrefs.GetString("PlayerName", "");
            playerNameInput.text = str;
            MyNetwork.Singleton.SetPlayerName(str);
        }
        
        void OnEnable()
        {
            playerNameInput.text = GameSettings.playername;
            tutorialToggle.isOn = PlayerPrefs.GetInt("istutorial", 1) == 1;
        }

        public void OnPlayerNameChanged()
        {
            PlayerPrefs.SetString("PlayerName", playerNameInput.text);
            MyNetwork.Singleton.SetPlayerName(playerNameInput.text);
            GameSettings.playername = playerNameInput.text;
        }
        
        public void OnTutorialChanged(bool isTrue)
        {
            PlayerPrefs.SetInt("istutorial", isTrue ? 1 : 0);
        }
    }
}