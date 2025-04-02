using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Assets
{
    public class ChooseCharacterScreenManager : MonoBehaviour
    {
        static public ChooseCharacterScreenManager Singleton { get; internal set; }
        public GameObject readyObj;
        public TextMeshProUGUI readyCountLabel;
        public TextMeshProUGUI selectedNameLabel;
        public CharacterItem selectedItem;
        public Transform leftTransform;
        public Transform rightTrasform;
        public GameObject characterItemPrefab;
        public Transform showTransform;
        public Button readyButton;
        public bool frost = false;
        
        // Use this for initialization
        void Start()
        {
            Singleton = this;
            var item = leftTransform.GetComponentInChildren<CharacterItem>();
            OnSelectItem(item);
        }

        private void OnDestroy()
        {
            Singleton = null;
        }

        private void OnEnable()
        {
            
            frost = false;
            readyButton.interactable = true;
            RefreshReadyLabel();
        }

        public void RefreshReadyLabel()
        {
            if (!NetworkManager.Singleton)
                return;

            if (!NetworkManager.Singleton.IsClient)
                readyCountLabel.text = "";
            else
                readyCountLabel.text = MyNetwork.Singleton.readyCount + "/" + MyNetwork.Singleton.totalCount;
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButton(0))
            {
                float rotX = Input.GetAxis("Mouse X");
                float rotY = 0f;//Input.GetAxis("Mouse Y");
                showTransform.Rotate(rotY, rotX * 10, 0.0f);
            }
        }

        public void OnSelectItem(CharacterItem item)
        {
            if (frost)
                return;

            if (selectedItem)
                selectedItem.Deselect();
            item.Select();
            selectedItem = item;
            selectedNameLabel.text = item.nameLabel.text;
            GameSettings.characterId = item.id;
        }

        public void OnClickReadyButton()
        {
            frost = true;
            readyButton.interactable = false;
        }
    }
}