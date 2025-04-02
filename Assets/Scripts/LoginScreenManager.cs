using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Assets;

public class LoginScreenManager : MonoBehaviour
{
    public TMP_InputField emailInput;
    
    public TMP_InputField passwordInput;
    
    public TMP_InputField classInput;
    
    public TMP_Text error;
    
    void Start()
    {
        error.gameObject.SetActive(false);
        emailInput.text = PlayerPrefs.GetString("email", "");
        passwordInput.text = PlayerPrefs.GetString("password", "");
        classInput.text = PlayerPrefs.GetString("classId", "");
    }
    
    public void OnSignup()
    {
        if(emailInput.text == "")
        {
            StartCoroutine(OnError("Please input email!"));
            return;
        }
        if(passwordInput.text == "")
        {
            StartCoroutine(OnError("Please input password!"));
            return;
        }
        if(classInput.text == "")
        {
            StartCoroutine(OnError("Please input ClassID!"));
            return;
        }
        Dictionary<string, string> param = new Dictionary<string, string>();
        param["email"] = emailInput.text;
        param["password"] = passwordInput.text;
        StartCoroutine(WebServices.Signup(param, OnLoginComplete));
    }
    
    public void OnLogin()
    {
        if(emailInput.text == "")
        {
            StartCoroutine(OnError("Please input email!"));
            return;
        }
        if(passwordInput.text == "")
        {
            StartCoroutine(OnError("Please input password!"));
            return;
        }
        Dictionary<string, string> param = new Dictionary<string, string>();
        param["email"] = emailInput.text;
        param["password"] = passwordInput.text;
        param["classId"] = classInput.text;
        StartCoroutine(WebServices.Login(param, OnLoginComplete));
    }
    
    public void OnLoginComplete(bool isFail, Dictionary<string, object> param)
    {
        if(isFail)
        {
            StartCoroutine(OnError("You input wrong email or password. Please try again!"));
            return;
        }
        
        if(param.ContainsKey("message"))
        {
            StartCoroutine(OnError(param["message"].ToString()));
            return;            
        }
        
        PlayerPrefs.SetString("email", emailInput.text);
        PlayerPrefs.SetString("password", passwordInput.text);
        PlayerPrefs.SetString("classId", classInput.text);
        
        GlobalData.Singleton.email = emailInput.text;
        GlobalData.Singleton.classId = classInput.text;
        GlobalData.Singleton.studentId = param["uid"].ToString();
        
        StartCoroutine(WebServices.GetClassData(OnGetClassDataComplete));
    }
    
    public void OnGetClassDataComplete(bool isFail, Dictionary<string, object> param)
    {
        if(param.ContainsKey("responseTime"))
        {
            GameSettings.gameTime = int.Parse(param["responseTime"].ToString()) * 60;
        }
        EntrySceneManager.Singleton.SetScreen("OpenScreen");
    }
    
    IEnumerator OnError(string msg)
    {
        error.text = msg;
        error.gameObject.SetActive(true);
        yield return new WaitForSeconds(5f);
        error.gameObject.SetActive(false);
    }
}
