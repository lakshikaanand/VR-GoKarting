using System.Collections;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using TMPro;
using UnityEngine.SceneManagement;
using Firebase.Firestore;
using System;
using UnityEngine.UI;
using System.Threading.Tasks;

public class EmailAuthManager : MonoBehaviour
{
    //Firebase variables
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public static FirebaseAuth auth;
    public static FirebaseUser user;
    public static DocumentSnapshot userData;

    //Login variables
    [Header("Login")]
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;
    public TMP_Text warningLoginText;
    public TMP_Text confirmLoginText;
    public GameObject popUpPanel;
   
    //Register variables
    [Header("Register")]
    public TMP_InputField usernameRegisterField;
    public TMP_InputField emailRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField passwordRegisterVerifyField;
    public TMP_Text warningRegisterText;
    public static FirebaseFirestore app;
    
    private void Awake()
    {
        InitializeFirebase();

    }

    public  static  void InitializeFirebase()
    {
        Debug.Log("Setting up Firebase Auth");
        //Set the authentication instance object
        auth = FirebaseAuth.DefaultInstance;
        app = FirebaseFirestore.DefaultInstance;
    }
    private void Start()
    {
        checkUser();
        VrModeController.ExitVR();
    }

    public static async void setUserdata() 
    {
        try
        {
            InitializeFirebase();
            user = auth.CurrentUser;
            userData = await FirebaseFirestore.DefaultInstance.Collection("Users").Document(user.UserId).GetSnapshotAsync();
        }
        catch (Exception e)
        {

        }
        
    }

    private async void checkUser()
    {
        
        if (auth.CurrentUser!=null) 
        {
            user = auth.CurrentUser;
            print("User is not null" + auth.CurrentUser.Email);
            if (user.IsEmailVerified)
            {

                userData = await app.Collection("Users").Document(user.UserId).GetSnapshotAsync();

                if (userData.Exists)
                {
                    SceneManager.LoadScene("MainMenu");
                }
                else
                {
                    print("Plz Register Before Login");
                }
            }
            else {

                try
                {
                    print(user.DisplayName);
                    print(user.IsEmailVerified);

                    await user.SendEmailVerificationAsync();
                    confirmLoginText.text = "Verifincation mail has been sent again to "+ auth.CurrentUser.Email+" please verify and try logging In.";
                    popUpPanel.SetActive(true);
                    print("Verification Email Sent Again");
                }
                catch(Exception e)
                {
                }
            }

        }
        
    }
    public void LoginButton()
    {
        //Call the login coroutine passing the email and password
        StartCoroutine(Login(emailLoginField.text, passwordLoginField.text));
    }
    //Function for the register button
    public void RegisterButton()
    {
        //Call the register coroutine passing the email, password, and username
        StartCoroutine(Register(emailRegisterField.text, passwordRegisterField.text, usernameRegisterField.text));
    }

    public void resetPass() {
        StartCoroutine(forgotPass());

    }


    public  IEnumerator forgotPass()
    {
        if (emailLoginField.text.Length >0)
        {
            Task task =auth.SendPasswordResetEmailAsync(emailLoginField.text);
            yield return new WaitUntil(predicate: () => task.IsCompleted);

            if (task.IsCanceled)
            {
                Debug.Log("SendPasswordResetEmailAsync encountered an error: " + task.Exception);

                warningLoginText.text = "Something went wrong. Please try again later.";
            }
            else if (task.IsFaulted)
            {
                Debug.Log("SendPasswordResetEmailAsync encountered an error: " + task.Exception);

                warningLoginText.text = "Please check your details and try again.";
            } else if (task.IsCompleted) {

                warningLoginText.text = "Pasword reset link sent succcessfull. Check you mail.";

            }

            popUpPanel.SetActive(true);


        }
        else {
            yield return new WaitForSeconds(0);

            warningLoginText.text = "Please enter a proper email and try again.";
            popUpPanel.SetActive(true);


        }

    }
    private IEnumerator Login(string _email, string _password)
    {
        //Call the Firebase auth signin function passing the email and password
        var LoginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);
        //Wait until the task completes
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if (LoginTask.Exception != null)
        {
            //If there are errors handle them
            Debug.LogWarning(message: $"Failed to register task with {LoginTask.Exception}");
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Login Failed!";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "Missing Email";
                    
                    break;
                case AuthError.MissingPassword:
                    message = "Missing Password";
                    
                    break;
                case AuthError.WrongPassword:
                    message = "Wrong Password";
                  
                    break;
                case AuthError.InvalidEmail:
                    message = "Invalid Email";
                  
                    break;
                case AuthError.UserNotFound:
                    message = "Account does not exist";
                   
                    break;
            }
            popUpPanel.SetActive(true);
            warningLoginText.text = message;
        }
        else
        {
            //User is now logged in
            //Now get the result
            
            user = LoginTask.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})", user.DisplayName, user.Email);
            warningLoginText.text = "";
            if (!user.IsEmailVerified)
            {
                confirmLoginText.text = "Please verify you email before login";
                popUpPanel.SetActive(true);


            }
            else {
            }

            checkUser();
        }
    }
    public async void addUserToDb()
    {
        try
        {
            await app.Collection("Users").Document(user.UserId).SetAsync(new { Username = user.DisplayName, Email = user.Email, });
        }
        catch(Exception e)
        {
            print("" + e);
        }
    }
    private IEnumerator Register(string _email, string _password, string _username)
    {
        if (_username == "")
        {
            //If the username field is blank show a warning
            popUpPanel.SetActive(true);
            warningRegisterText.text = "Missing Username";
        }
        else if (passwordRegisterField.text != passwordRegisterVerifyField.text)
        {
            //If the password does not match show a warning
            popUpPanel.SetActive(true);
            warningRegisterText.text = "Password Does Not Match!";
        }
        else
        {
            //Call the Firebase auth signin function passing the email and password
            var RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
            //Wait until the task completes
            yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

            if (RegisterTask.Exception != null)
            {
                //If there are errors handle them
                Debug.LogWarning(message: $"Failed to register task with {RegisterTask.Exception}");
                FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                string message = "Register Failed!";
                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        message = "Missing Email";
                        break;
                    case AuthError.MissingPassword:
                        message = "Missing Password";
                        break;
                    case AuthError.WeakPassword:
                        message = "Weak Password";
                        break;                   
                    case AuthError.EmailAlreadyInUse:
                        message = "Email Already In Use";
                        break;
                }
                popUpPanel.SetActive(true);
                warningRegisterText.text = message;
            }
            else
            {
                //User has now been created
                //Now get the result
                user = RegisterTask.Result;

                if (user != null)
                {
                    //Create a user profile and set the username
                    UserProfile profile = new UserProfile { DisplayName = _username };

                    //Call the Firebase auth update user profile function passing the profile with the username
                    var ProfileTask = user.UpdateUserProfileAsync(profile);
                    //Wait until the task completes
                    yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

                    if (ProfileTask.Exception != null)
                    {
                        //If there are errors handle them
                        Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
                        FirebaseException firebaseEx = ProfileTask.Exception.GetBaseException() as FirebaseException;
                        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                        warningRegisterText.text = "Username Set Failed!";
                    }
                    else
                    {
                        //Username is now set
                        //Now return to login screen
                        popUpPanel.SetActive(true);
                        user.SendEmailVerificationAsync();
                        //UiManager.instance.LoginScreen();
                        addUserToDb();
                        warningRegisterText.text = "User Succesfully Register" +"\n" + "Please Verify your Email";
                    }
                }
            }
        }
    }
}
