using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InstructionManager : MonoBehaviour
{
    public Toggle showToggle;
    
    public void ShowTutorial()
    {
        PlayerPrefs.SetInt("istutorial", showToggle.isOn ? 0 : 1);
    }
}
