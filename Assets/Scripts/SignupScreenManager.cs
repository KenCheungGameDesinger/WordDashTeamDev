using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SignupScreenManager : MonoBehaviour
{
    public TMP_InputField usernameInput;
    
    public TMP_InputField emailInput;
    
    public TMP_InputField passwordInput;
    
    public TMP_Text error;
    
    
    void Start()
    {
        error.gameObject.SetActive(false);
    }
    
    public void OnSignup()
    {
        if(usernameInput.text == "")
        {
            StartCoroutine(OnError("Please input username!"));
            return;
        }
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
        param["name"] = usernameInput.text;
        param["type"] = "Student";
        param["email"] = emailInput.text;
        param["password"] = passwordInput.text;
        StartCoroutine(WebServices.Signup(param, OnSignupComplete));
    }
    
    public void OnSignupComplete(bool isFail, Dictionary<string, object> param)
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
    }
    
    IEnumerator OnError(string msg)
    {
        error.text = msg;
        error.gameObject.SetActive(true);
        yield return new WaitForSeconds(5f);
        error.gameObject.SetActive(false);
    }
}
