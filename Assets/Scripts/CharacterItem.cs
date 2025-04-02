using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets
{
    public class CharacterItem : MonoBehaviour
    {
        public int id;
        public GameObject selFrameObj;
        public GameObject selTickObj;
        public TextMeshProUGUI nameLabel;
        public RawImage avatarImage;
        public GameObject showObj;

        // Use this for initialization
        void Awake()
        {
            Deselect();
        }

        // Update is called once per frame
        void Update()
        {

        }

        //public void Init(int pId)
        //{
        //    id = pId;
        //    switch (id)
        //    {
        //        case 0:
        //            nameLabel.SetText("Lexi Luna");
        //            break;
        //        case 1:
        //            nameLabel.text = "Rhyme Ryan";
        //            break;
        //        case 2:
        //            nameLabel.text = "Vocab Vivian";
        //            break;
        //        case 3:
        //            nameLabel.text = "Grammar Gary";
        //            break;
        //        case 4:
        //            nameLabel.text = "Syntax Sally";
        //            break;
        //        case 5:
        //            nameLabel.text = "Word Wally";
        //            break;
        //        case 6:
        //            nameLabel.text = "Lingo Lana";
        //            break;
        //        case 7:
        //            nameLabel.text = "Diction Denny";
        //            break;
        //    }
        //}

        public void Select()
        {
            selTickObj.SetActive(true);
            selFrameObj.SetActive(true);
            showObj.SetActive(true);
            showObj.GetComponent<Animator>().SetBool("Grounded", true);
            showObj.transform.parent.localRotation = Quaternion.Euler(0, 180f, 0);
        }

        public void Deselect()
        {
            selTickObj.SetActive(false);
            selFrameObj.SetActive(false);
            showObj.SetActive(false);
        }

        public void OnClickItem()
        {
            ChooseCharacterScreenManager.Singleton.OnSelectItem(this);
        }
    }
}