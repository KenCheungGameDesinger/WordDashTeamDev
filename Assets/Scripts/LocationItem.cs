using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets
{
    public class LocationItem : MonoBehaviour
    {
        public int id;
        public GameObject selFrameObj;
        public GameObject selTickObj;
        public TextMeshProUGUI titleLabel;
        public TextMeshProUGUI descriptionLabel;
        public RawImage mapImage;


        // Use this for initialization
        void Awake()
        {
            Deselect();
        }

        // Update is called once per frame
        void Update()
        {

        }
        public void Select()
        {
            selTickObj.SetActive(true);
            selFrameObj.SetActive(true);
        }

        public void Deselect()
        {
            selTickObj.SetActive(false);
            selFrameObj.SetActive(false);
        }

        public void OnClickItem()
        {
            ChooseLocationScreenManager.instance.OnSelectItem(this);
        }
    }
}