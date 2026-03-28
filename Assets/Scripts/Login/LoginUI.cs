using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AsobiDemo
{
    public class LoginUI : MonoBehaviour
    {
        [SerializeField] TMP_InputField usernameField;
        [SerializeField] TMP_InputField passwordField;
        [SerializeField] Button loginButton;
        [SerializeField] Button registerButton;
        [SerializeField] TMP_Text statusText;

        void Start()
        {
            loginButton.onClick.AddListener(OnLogin);
            registerButton.onClick.AddListener(OnRegister);
            statusText.text = "";
        }

        async void OnLogin()
        {
            SetInteractable(false);
            statusText.text = "Logging in...";

            try
            {
                await GameConfig.Client.Auth.LoginAsync(usernameField.text, passwordField.text);
                statusText.text = "Success!";
                SceneLoader.LoadLobby();
            }
            catch (Asobi.AsobiException ex)
            {
                statusText.text = ex.Message;
                SetInteractable(true);
            }
        }

        async void OnRegister()
        {
            SetInteractable(false);
            statusText.text = "Creating account...";

            try
            {
                await GameConfig.Client.Auth.RegisterAsync(usernameField.text, passwordField.text);
                statusText.text = "Account created!";
                SceneLoader.LoadLobby();
            }
            catch (Asobi.AsobiException ex)
            {
                statusText.text = ex.Message;
                SetInteractable(true);
            }
        }

        void SetInteractable(bool value)
        {
            loginButton.interactable = value;
            registerButton.interactable = value;
            usernameField.interactable = value;
            passwordField.interactable = value;
        }
    }
}
