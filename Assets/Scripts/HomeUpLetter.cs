using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets
{
    public enum ELetterState
    {
        NONE,
        BAD,    //not letter
        SPELL,  //only letter
        GOOD    //both letter, position
    }

    public class HomeUpLetter : MonoBehaviour
    {
        
        public enum LetterType
        {
            HOME,
            MYSTERY
        }
        
        public Button mainButton;
        public Image letterImage;
        public Image stateGoodImage;
        public Image stateBadImage;
        public Image stateSpellImage;
        public LetterType type;
        public int index;

        public void SetIndex(int i)
        {
            index = i;
            letterImage.sprite = ArenaHudManager.Singleton.letterSprites[index];
            if(type == LetterType.HOME)
                mainButton.interactable= index != 27;
        }

        public void SetEmpty()
        {
            SetIndex(27);
            SetState(ELetterState.NONE);
        }

        public bool IsEmpty()
        {
            return index == 27;
        }

        public void OnItemClick()
        {
            HomeHudManager.Singleton.OnUpItemClick(this);
        }

        public void SetState(ELetterState eState)
        {
            stateBadImage.gameObject.SetActive(false);
            stateGoodImage.gameObject.SetActive(false);
            stateSpellImage.gameObject.SetActive(false);
            switch (eState) {
                case ELetterState.BAD:
                    stateBadImage.gameObject.SetActive(true);
                    break;
                case ELetterState.SPELL:
                    stateSpellImage.gameObject.SetActive(true);
                    break;
                case ELetterState.GOOD:
                    stateGoodImage.gameObject.SetActive(true);
                    break;
            }
        }

    }
}