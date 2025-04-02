using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets
{
    public class HomeDownLetter : MonoBehaviour
    {
        public Button mainButton;
        public Image letterImage;
        public TextMeshProUGUI countLabel;
        public int index { get; private set; }
        int count = 0;
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
        public void SetIndex(int i)
        {
            index = i;
            letterImage.sprite = ArenaHudManager.Singleton.letterSprites[index];
        }

        public void SetCount(int cnt)
        {
            count = cnt;
            mainButton.interactable = count > 0;
            countLabel.text = count.ToString();
            countLabel.gameObject.SetActive(count > 1);
        }

        public void Inc()
        {
            SetCount(count + 1);
        }
        public void Dec()
        {
            SetCount(count - 1);
        }

        public void OnItemClick()
        {
            HomeHudManager.Singleton.OnDownItemClick(this);
        }

    }
}