using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets;

public class GlobalData : MonoBehaviour
{
    public static GlobalData Singleton;
    
    public string email;
    public string classId;
    public string studentId;
    public List<MysteryWord> mysteryWords = new List<MysteryWord>();
    
    void Awake()
    {
        if(Singleton != null)
        {
            Destroy(this.gameObject);
            return;
        }
        Singleton = this;
        DontDestroyOnLoad(gameObject);
    }
}
