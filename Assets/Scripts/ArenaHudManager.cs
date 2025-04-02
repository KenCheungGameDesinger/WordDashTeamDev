using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

namespace Assets
{
    public class ArenaHudManager : MonoBehaviour
    {
        public static ArenaHudManager Singleton;
        
        public GameObject instructionObj;

        public List<Sprite> letterSprites;
        
        public GameObject loadingObj;

        private void Awake()
        {
            Singleton = this;
            InGameHudManager.Singleton = transform.Find("InGameHud").GetComponent<InGameHudManager>();
            HomeHudManager.Singleton = transform.Find("HomeHud").GetComponent<HomeHudManager>();
            MysteryHubManager.Singleton = transform.Find("MysteryHub").GetComponent<MysteryHubManager>();
        }
        // Use this for initialization
        void Start()
        {
            InGameHudManager.Singleton.gameObject.SetActive(true);
            HomeHudManager.Singleton.obj.SetActive(false);
            MysteryHubManager.Singleton.gameObject.SetActive(false);
            instructionObj.SetActive(!NetworkManager.Singleton.IsConnectedClient && PlayerPrefs.GetInt("istutorial", 1) == 1);
        }
    }
}