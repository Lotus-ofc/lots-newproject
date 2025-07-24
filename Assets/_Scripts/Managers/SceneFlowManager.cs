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
    public GameObject gameplayPanel;

    [Header("Feedback")]
    public TextMeshProUGUI mensagemFeedback;

    [Header("Login")]
    public TMP_InputField loginEmailInput;
    public TMP_InputField loginPasswordInput;
    public Image loginErrorImage;
    public Button loginButton;
    public Button loginBackButton;
    public Button forgotPasswordButton;
    public Button loginTogglePasswordVisibilityButton;
    public Image loginTogglePasswordImage;

    [Header("Register")]
    public TMP_InputField registerEmailInput;
    public TMP_InputField registerPasswordInput;
    public TMP_InputField registerConfirmPasswordInput;
    public Button registerButton;
    public Button registerBackButton;
    public Button registerTogglePasswordVisibilityButton1;
    public Button registerTogglePasswordVisibilityButton2;
    public Image registerTogglePasswordImage1;
    public Image registerTogglePasswordImage2;

    [Header("Logout")]
    public Button logoutButton;

    [Header("Sprites")]
    public Sprite eyeOpenSprite;
    public Sprite eyeClosedSprite;

    private bool isLoginPasswordVisible = false;
    private bool isRegisterPasswordVisible = false;
    private bool isRegisterConfirmPasswordVisible = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("SceneFlowManager criado.");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        mensagemFeedback.text = "";

        ShowMainMenuPanel();

        // Eventos
        loginButton.onClick.AddListener(OnLoginButtonClicked);
        loginBackButton.onClick.AddListener(ShowMainMenuPanel);
        forgotPasswordButton.onClick.AddListener(OnForgotPasswordClicked);
        loginTogglePasswordVisibilityButton.onClick.AddListener(ToggleLoginPasswordVisibility);

        registerButton.onClick.AddListener(OnRegisterButtonClicked);
        registerBackButton.onClick.AddListener(ShowMainMenuPanel);
        registerTogglePasswordVisibilityButton1.onClick.AddListener(ToggleRegisterPasswordVisibility);
        registerTogglePasswordVisibilityButton2.onClick.AddListener(ToggleRegisterConfirmPasswordVisibility);

        if (logoutButton != null)
            logoutButton.onClick.AddListener(OnLogoutButtonClicked);

        ResetFields();
    }

    private void OnEnable()
    {
        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.OnAuthFailed += OnFirebaseAuthFailed;
            FirebaseManager.Instance.OnAuthSuccess += OnFirebaseAuthSuccess;
            FirebaseManager.Instance.OnLogout += OnFirebaseLogout;
        }
        else
        {
            Debug.LogError("FirebaseManager.Instance é nulo no OnEnable!");
        }
    }

    private void OnDisable()
    {
        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.OnAuthFailed -= OnFirebaseAuthFailed;
            FirebaseManager.Instance.OnAuthSuccess -= OnFirebaseAuthSuccess;
            FirebaseManager.Instance.OnLogout -= OnFirebaseLogout;
        }
    }

    #region Panels
    private void ShowOnlyPanel(GameObject panel)
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(false);
        mainMenuPanel.SetActive(false);
        gameplayPanel.SetActive(false);

        if (panel != null)
            panel.SetActive(true);

        ClearFeedback();
    }

    public void ShowMainMenuPanel() => ShowOnlyPanel(mainMenuPanel);
    public void ShowLoginPanel()
    {
        ShowOnlyPanel(loginPanel);
        ResetFields();
    }
    public void ShowRegisterPanel()
    {
        ShowOnlyPanel(registerPanel);
        ResetFields();
    }
    #endregion

    #region Login
    private void OnLoginButtonClicked()
    {
        if (string.IsNullOrEmpty(loginEmailInput.text) || string.IsNullOrEmpty(loginPasswordInput.text))
        {
            ShowFeedback("Preencha email e senha");
            loginErrorImage?.gameObject.SetActive(true);
            return;
        }

        loginErrorImage?.gameObject.SetActive(false);
        ShowFeedback("Tentando logar...");
        FirebaseManager.Instance.LoginUser(loginEmailInput.text, loginPasswordInput.text);
    }

    private void OnForgotPasswordClicked()
    {
        if (string.IsNullOrEmpty(loginEmailInput.text))
        {
            ShowFeedback("Digite o email para resetar");
            return;
        }

        ShowFeedback("Enviando email de reset...");
        FirebaseManager.Instance.SendPasswordResetEmail(loginEmailInput.text);
    }

    private void ToggleLoginPasswordVisibility()
    {
        isLoginPasswordVisible = !isLoginPasswordVisible;
        SetInputFieldPasswordMode(loginPasswordInput, isLoginPasswordVisible);
        UpdateToggleIcon(loginTogglePasswordImage, isLoginPasswordVisible);
    }
    #endregion

    #region Register
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
            ShowFeedback("Senhas não conferem");
            return;
        }

        ShowFeedback("Registrando...");
        FirebaseManager.Instance.RegisterUser(registerEmailInput.text, registerPasswordInput.text);

        ShowLoginPanel();
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
    #endregion

    #region Logout
    private void OnLogoutButtonClicked()
    {
        FirebaseManager.Instance.Logout();
    }
    #endregion

    #region Firebase Events
    private void OnFirebaseAuthFailed(string msg)
    {
        ShowFeedback(msg);
        loginErrorImage?.gameObject.SetActive(true);
    }

    private void OnFirebaseAuthSuccess()
    {
        Debug.Log("Usuário autenticado. Ativando gameplay panel...");
        ShowOnlyPanel(gameplayPanel);
    }

    private void OnFirebaseLogout()
    {
        ShowFeedback("Deslogado.");
        ShowMainMenuPanel();
    }
    #endregion

    #region Feedback e Utils
    public void ShowFeedback(string msg)
    {
        if (mensagemFeedback != null)
            mensagemFeedback.text = msg;
    }

    public void ClearFeedback()
    {
        if (mensagemFeedback != null)
            mensagemFeedback.text = "";
    }

    private void ResetFields()
    {
        loginEmailInput.text = "";
        loginPasswordInput.text = "";
        registerEmailInput.text = "";
        registerPasswordInput.text = "";
        registerConfirmPasswordInput.text = "";

        isLoginPasswordVisible = false;
        isRegisterPasswordVisible = false;
        isRegisterConfirmPasswordVisible = false;

        SetInputFieldPasswordMode(loginPasswordInput, false);
        SetInputFieldPasswordMode(registerPasswordInput, false);
        SetInputFieldPasswordMode(registerConfirmPasswordInput, false);

        UpdateToggleIcon(loginTogglePasswordImage, false);
        UpdateToggleIcon(registerTogglePasswordImage1, false);
        UpdateToggleIcon(registerTogglePasswordImage2, false);

        loginErrorImage?.gameObject.SetActive(false);
        ClearFeedback();
    }

    private void SetInputFieldPasswordMode(TMP_InputField input, bool visible)
    {
        if (input == null) return;
        input.contentType = visible ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;
        input.ForceLabelUpdate();
    }

    private void UpdateToggleIcon(Image img, bool visible)
    {
        if (img == null || eyeOpenSprite == null || eyeClosedSprite == null) return;
        img.sprite = visible ? eyeOpenSprite : eyeClosedSprite;
    }
    #endregion
}
