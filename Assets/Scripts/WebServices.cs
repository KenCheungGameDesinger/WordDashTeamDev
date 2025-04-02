using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using Assets;
using MiniJSON;

public class WebServices : MonoBehaviour
{
    static readonly string url = "https://5vaz7pxxrm.us-east-1.awsapprunner.com/";
    static readonly string url2 = "https://04ed-138-201-248-109.ngrok-free.app/";
    
    public static IEnumerator CheckWord(string word, Action<bool> onComplete)
    {
        ArenaHudManager.Singleton.loadingObj.SetActive(true);
        UnityWebRequest request  = UnityWebRequest.Get(url + "word-dash/check-word?word=" + word);
        Debug.Log("Sending request " + request.url);
        yield return request.SendWebRequest();
        Debug.Log("Response " + request.downloadHandler.text);
        if(request.downloadHandler.text != "")
        {
            Dictionary<string, object> dicData = Json.Deserialize(request.downloadHandler.text) as Dictionary<string, object>;
            if(dicData != null)
                onComplete?.Invoke(dicData["accuracy"].ToString() == "Correct");
        }
        ArenaHudManager.Singleton.loadingObj.SetActive(false);
    }
    
    public static IEnumerator GetMysteryWords(Action<bool, List<object>> onComplete)
    {
        EntrySceneManager.Singleton.loadingObj.SetActive(true);
        UnityWebRequest request  = UnityWebRequest.Get(url + "word-dash/mystery-word?class_id=" + GlobalData.Singleton.classId);
        Debug.Log("Sending request " + request.url);
        yield return request.SendWebRequest();
        Debug.Log("Response " + request.downloadHandler.text);
        if(request.downloadHandler.text == "")
        {
            onComplete?.Invoke(true, null);
        }
        else
        {
            List<object> listData = Json.Deserialize(request.downloadHandler.text) as List<object>;
            onComplete?.Invoke(false, listData);
        }
        EntrySceneManager.Singleton.loadingObj.SetActive(false);
    }
    
    public static IEnumerator GetClassData(Action<bool, Dictionary<string, object>> onComplete)
    {
        EntrySceneManager.Singleton.loadingObj.SetActive(true);
        UnityWebRequest request  = UnityWebRequest.Get(url + "word-dash/classroombyclassid?class_id=" + GlobalData.Singleton.classId);
        Debug.Log("Sending request " + request.url);
        yield return request.SendWebRequest();
        Debug.Log("Response " + request.downloadHandler.text);
        if(request.downloadHandler.text == "")
        {
            onComplete?.Invoke(true, null);
        }
        else
        {
            Dictionary<string, object> dicData = Json.Deserialize(request.downloadHandler.text) as Dictionary<string, object>;
            onComplete?.Invoke(false, dicData);
        }
        EntrySceneManager.Singleton.loadingObj.SetActive(false);
    }
    
    public static IEnumerator Login(Dictionary<string, string> param, Action<bool, Dictionary<string, object>> onComplete)
    {
        EntrySceneManager.Singleton.loadingObj.SetActive(true);
        UnityWebRequest request  = UnityWebRequest.Post(url + "auth/signin-class-id", param);
        Debug.Log("Sending request " + request.url);
        yield return request.SendWebRequest();
        Debug.Log("Response " + request.downloadHandler.text);
        if(request.downloadHandler.text == "")
        {
            onComplete?.Invoke(true, null);
        }
        else
        {
            Dictionary<string, object> dicData = Json.Deserialize(request.downloadHandler.text) as Dictionary<string, object>;
            onComplete?.Invoke(false, dicData);
        }
        EntrySceneManager.Singleton.loadingObj.SetActive(false);
    }
    
    public static IEnumerator Signup(Dictionary<string, string> param, Action<bool, Dictionary<string, object>> onComplete)
    {
        EntrySceneManager.Singleton.loadingObj.SetActive(true);
        UnityWebRequest request  = UnityWebRequest.Post(url + "auth/signup", param);
        Debug.Log("Sending request " + request.url);
        yield return request.SendWebRequest();
        Debug.Log("Response " + request.downloadHandler.text);
        if(request.downloadHandler.text == "")
        {
            onComplete?.Invoke(true, null);
        }
        else
        {
            Dictionary<string, object> dicData = Json.Deserialize(request.downloadHandler.text) as Dictionary<string, object>;
            onComplete?.Invoke(false, dicData);
        }
        EntrySceneManager.Singleton.loadingObj.SetActive(false);
    }
    
    public static IEnumerator PostTopWord(Dictionary<string, string> param, Action<bool, Dictionary<string, object>> onComplete)
    {
        UnityWebRequest request  = UnityWebRequest.Post(url2 + "word-dash/analytic-data", param);
        Debug.Log("Sending request " + request.url);
        yield return request.SendWebRequest();
        Debug.Log("Response " + request.downloadHandler.text);
        if(request.downloadHandler.text == "")
        {
            onComplete?.Invoke(true, null);
        }
        else
        {
        }
    }
}
