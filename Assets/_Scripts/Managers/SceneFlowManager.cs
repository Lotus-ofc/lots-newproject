using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SceneFlowManager : MonoBehaviour
{
    public static SceneFlowManager Instance;

    [Header("Painéis")]
    public GameObject loginPanel;
    public GameObject registerPanel;
    public GameObject mainMenuPanel;

    [Header("Fade")]
    public FadeController fadeController;

    [Header("Feedback")]
    public TextMeshProUGUI mensagemFeedback;

    [Header("Login Inputs")]
    public TMP_InputField loginEmailInput;
    public TMP_InputField loginPasswordInput;
    public Image loginErrorImage;

    [Header("Login Buttons")]
    public Button loginButton;
    public Button loginBackButton;
    public Button forgotPasswordButton;
    public Button loginTogglePasswordVisibilityButton;

    [Header("Register Inputs")]
    public TMP_InputField registerEmailInput;
    public TMP_InputField registerPasswordInput;
    public TMP_InputField registerConfirmPasswordInput;

    [Header("Register Buttons")]
    public Button registerButton;
    public Button registerBackButton;
    public Button registerTogglePasswordVisibilityButton1;
    public Button registerTogglePasswordVisibilityButton2;

    [Header("Sprites Olho")]
    public Sprite eyeOpenSprite;
    public Sprite eyeClosedSprite;

    [Header("Login Toggle Image")]
    public Image loginTogglePasswordImage;

    [Header("Register Toggle Images")]
    public Image registerTogglePasswordImage1;
    public Image registerTogglePasswordImage2;

    private bool isLoginPasswordVisible = false;
    private bool isRegisterPasswordVisible = false;
    private bool isRegisterConfirmPasswordVisible = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ShowMainMenuPanel();
        mensagemFeedback.text = "";

        loginButton.onClick.AddListener(OnLoginButtonClicked);
        loginBackButton.onClick.AddListener(() => FadeToPanel(mainMenuPanel));
        forgotPasswordButton.onClick.AddListener(OnForgotPasswordClicked);
        loginTogglePasswordVisibilityButton.onClick.AddListener(ToggleLoginPasswordVisibility);

        registerButton.onClick.AddListener(OnRegisterButtonClicked);
        registerBackButton.onClick.AddListener(() => FadeToPanel(mainMenuPanel));
        registerTogglePasswordVisibilityButton1.onClick.AddListener(ToggleRegisterPasswordVisibility);
        registerTogglePasswordVisibilityButton2.onClick.AddListener(ToggleRegisterConfirmPasswordVisibility);

        if (loginErrorImage != null) loginErrorImage.gameObject.SetActive(false);

        SetInputFieldPasswordMode(loginPasswordInput, false);
        SetInputFieldPasswordMode(registerPasswordInput, false);
        SetInputFieldPasswordMode(registerConfirmPasswordInput, false);
    }

    #region Painéis
    private void ShowOnlyPanel(GameObject panelToShow)
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(false);
        mainMenuPanel.SetActive(false);

        if (panelToShow != null)
        {
            panelToShow.SetActive(true);
        }

        ClearFeedback();
    }

    public void ShowLoginPanel() { ShowOnlyPanel(loginPanel); ClearLoginFields(); }
    public void ShowRegisterPanel() { ShowOnlyPanel(registerPanel); ClearRegisterFields(); }
    public void ShowMainMenuPanel() { ShowOnlyPanel(mainMenuPanel); }

    public void FadeToPanel(GameObject targetPanel)
    {
        fadeController.FadeOut(() =>
        {
            ShowOnlyPanel(targetPanel);

            if (targetPanel == loginPanel) ClearLoginFields();
            else if (targetPanel == registerPanel) ClearRegisterFields();

            fadeController.FadeIn();
        });
    }

    public void GoToGameplayScene()
    {
        fadeController.FadeOut(() =>
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("01_Gameplay");
        });
    }
    #endregion

    #region Feedback
    public void ShowFeedback(string message)
    {
        if (mensagemFeedback != null)
        {
            string cleanMessage = System.Text.RegularExpressions.Regex.Replace(message, @"[^a-zA-Z0-9 .,]", "");
            mensagemFeedback.text = cleanMessage;
        }
    }

    public void ClearFeedback()
    {
        if (mensagemFeedback != null)
            mensagemFeedback.text = "";
    }
    #endregion

    #region Login Methods
    private void ClearLoginFields()
    {
        if (loginEmailInput != null) loginEmailInput.text = "";
        if (loginPasswordInput != null) loginPasswordInput.text = "";
        if (loginErrorImage != null) loginErrorImage.gameObject.SetActive(false);
        isLoginPasswordVisible = false;
        SetInputFieldPasswordMode(loginPasswordInput, false);
        UpdateToggleIcon(loginTogglePasswordImage, isLoginPasswordVisible);
    }

    private void ToggleLoginPasswordVisibility()
    {
        isLoginPasswordVisible = !isLoginPasswordVisible;
        SetInputFieldPasswordMode(loginPasswordInput, isLoginPasswordVisible);
        UpdateToggleIcon(loginTogglePasswordImage, isLoginPasswordVisible);
    }

    private void OnLoginButtonClicked()
    {
        if (string.IsNullOrEmpty(loginEmailInput.text) || string.IsNullOrEmpty(loginPasswordInput.text))
        {
            ShowFeedback("Preencha email e senha");
            if (loginErrorImage != null) loginErrorImage.gameObject.SetActive(true);
            return;
        }
        if (loginErrorImage != null) loginErrorImage.gameObject.SetActive(false);

        ClearFeedback();

        // TODO: Firebase login
        ShowFeedback("Tentando logar...");
    }

    private void OnForgotPasswordClicked()
    {
        ClearFeedback();
        ShowFeedback("Funcionalidade em desenvolvimento");
    }
    #endregion

    #region Register Methods
    private void ClearRegisterFields()
    {
        if (registerEmailInput != null) registerEmailInput.text = "";
        if (registerPasswordInput != null) registerPasswordInput.text = "";
        if (registerConfirmPasswordInput != null) registerConfirmPasswordInput.text = "";
        isRegisterPasswordVisible = false;
        isRegisterConfirmPasswordVisible = false;
        SetInputFieldPasswordMode(registerPasswordInput, false);
        SetInputFieldPasswordMode(registerConfirmPasswordInput, false);
        UpdateToggleIcon(registerTogglePasswordImage1, isRegisterPasswordVisible);
        UpdateToggleIcon(registerTogglePasswordImage2, isRegisterConfirmPasswordVisible);
    }

    private void ToggleRegisterPasswordVisibility()
    {
        isRegisterPasswordVisible = !isRegisterPasswordVisible;
        SetInputFieldPasswordMode(registerPasswordInput, isRegisterPasswordVisible);
        UpdateToggleIcon(registerTogglePasswordImage1, isRegisterPasswordVisible);
    }

    private void ToggleRegisterConfirmPasswordVisibility()
    {
        isRegisterConfirmPasswordVisible = !isRegisterConfirmPasswordVisible;
        SetInputFieldPasswordMode(registerConfirmPasswordInput, isRegisterConfirmPasswordVisible);
        UpdateToggleIcon(registerTogglePasswordImage2, isRegisterConfirmPasswordVisible);
    }

    private void OnRegisterButtonClicked()
    {
        if (string.IsNullOrEmpty(registerEmailInput.text) ||
            string.IsNullOrEmpty(registerPasswordInput.text) ||
            string.IsNullOrEmpty(registerConfirmPasswordInput.text))
        {
            ShowFeedback("Preencha todos os campos");
            return;
        }

        if (registerPasswordInput.text != registerConfirmPasswordInput.text)
        {
            ShowFeedback("Senhas nao conferem");
            return;
        }

        ClearFeedback();

        // TODO: Firebase register
        ShowFeedback("Tentando registrar...");
    }
    #endregion

    #region Helpers
    private void SetInputFieldPasswordMode(TMP_InputField inputField, bool visible)
    {
        if (inputField == null) return;

        inputField.contentType = visible ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;
        inputField.ForceLabelUpdate();
    }

    private void UpdateToggleIcon(Image iconImage, bool visible)
    {
        if (iconImage == null || eyeOpenSprite == null || eyeClosedSprite == null) return;

        iconImage.sprite = visible ? eyeOpenSprite : eyeClosedSprite;
    }
    #endregion
}
