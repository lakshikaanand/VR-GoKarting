using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AuthManager : MonoBehaviour
{
    public TMP_InputField username;
    public TMP_InputField password;

    public GameObject loginFailedPanel;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void login() {
        if (username.text == "admin" && password.text == "password")
        {
            SceneLoader.gotoScene("MainMenu");
            username.text = "";
            password.text = "";
        }
        else {
            loginFailedPanel.SetActive(true);
        }
    }

}
