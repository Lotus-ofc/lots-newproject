using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SceneFlowManager : MonoBehaviour
{
    public static SceneFlowManager Instance;

    [Header("Painéis")]
    public GameObject loginPanel;
    public GameObject registerPanel;
    public GameObject mainMenuPanel;
    public GameObject pressToPlayPanel;
    public GameObject gameplayPanel;
    public GameObject loadingPanel;

    [Header("Feedback")]
    public TextMeshProUGUI mensagemFeedback;

    [Header("Login")]
    public TMP_InputField loginEmailInput;
    public TMP_InputField loginPasswordInput;
    public Image loginEmailErrorImage;
    public Image loginPasswordErrorImage;
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
    public Image registerEmailErrorImage;
    public Image registerPasswordErrorImage;
    public Image registerConfirmPasswordErrorImage;
    public Button registerButton;
    public Button registerBackButton;
    public Button registerTogglePasswordVisibilityButton1;
    public Button registerTogglePasswordVisibilityButton2;
    public Image registerTogglePasswordImage1;
    public Image registerTogglePasswordImage2;

    [Header("Logout")]
    public Button logoutButton;

    [Header("Press To Play")]
    public Button playButton;

    [Header("Sprites")]
    public Sprite eyeOpenSprite;
    public Sprite eyeClosedSprite;

    [Header("Managers de Gameplay (GameObjects)")]
    public GameObject uiManagerGO;
    public GameObject playerGO;

    private bool isLoginPasswordVisible = false;
    private bool isRegisterPasswordVisible = false;
    private bool isRegisterConfirmPasswordVisible = false;

    private Coroutine feedbackCoroutine;

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
        mensagemFeedback?.gameObject.SetActive(false);
        loadingPanel?.SetActive(false);

        playButton?.onClick.AddListener(ShowGameplayPanel);
        loginButton?.onClick.AddListener(OnLoginButtonClicked);
        loginBackButton?.onClick.AddListener(ShowMainMenuPanel);
        forgotPasswordButton?.onClick.AddListener(OnForgotPasswordClicked);
        loginTogglePasswordVisibilityButton?.onClick.AddListener(ToggleLoginPasswordVisibility);
        registerButton?.onClick.AddListener(OnRegisterButtonClicked);
        registerBackButton?.onClick.AddListener(ShowMainMenuPanel);
        registerTogglePasswordVisibilityButton1?.onClick.AddListener(ToggleRegisterPasswordVisibility);
        registerTogglePasswordVisibilityButton2?.onClick.AddListener(ToggleRegisterConfirmPasswordVisibility);
        logoutButton?.onClick.AddListener(OnLogoutButtonClicked);

        ResetFields();
        SetGameplayManagersActive(false);
        ShowOnlyPanel(loadingPanel);
    }

    private void OnEnable()
    {
        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.OnAuthFailed += OnFirebaseAuthFailed;
            FirebaseManager.Instance.OnAuthSuccess += OnFirebaseAuthSuccess;
            FirebaseManager.Instance.OnLogout += OnFirebaseLogout;
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
        loginPanel?.SetActive(false);
        registerPanel?.SetActive(false);
        mainMenuPanel?.SetActive(false);
        pressToPlayPanel?.SetActive(false);
        gameplayPanel?.SetActive(false);
        loadingPanel?.SetActive(false);

        panel?.SetActive(true);
        ClearFeedback();
    }

    public void ShowMainMenuPanel() { ShowOnlyPanel(mainMenuPanel); ResetFields(); }
    public void ShowLoginPanel() { ShowOnlyPanel(loginPanel); ResetFields(); }
    public void ShowRegisterPanel() { ShowOnlyPanel(registerPanel); ResetFields(); }
    public void ShowPressToPlayPanel() { ShowOnlyPanel(pressToPlayPanel); InitializeGameplayManagers(); }
    public void ShowGameplayPanel() { ShowOnlyPanel(gameplayPanel); ShowFeedback("Bem-vindo à Gameplay!"); }

    #endregion

    #region Login

    private void OnLoginButtonClicked()
    {
        ClearInputErrors();
        if (string.IsNullOrEmpty(loginEmailInput.text) || string.IsNullOrEmpty(loginPasswordInput.text))
        {
            loginEmailErrorImage?.gameObject.SetActive(string.IsNullOrEmpty(loginEmailInput.text));
            loginPasswordErrorImage?.gameObject.SetActive(string.IsNullOrEmpty(loginPasswordInput.text));
            loginErrorImage?.gameObject.SetActive(true);
            ShowFeedback("Preencha todos os campos.");
            return;
        }

        if (!IsValidEmail(loginEmailInput.text))
        {
            loginEmailErrorImage?.gameObject.SetActive(true);
            loginErrorImage?.gameObject.SetActive(true);
            ShowFeedback("E-mail inválido.");
            return;
        }

        ShowFeedback("Tentando logar...");
        FirebaseManager.Instance?.LoginUser(loginEmailInput.text, loginPasswordInput.text);
    }

    private void OnForgotPasswordClicked()
    {
        ClearInputErrors();
        if (string.IsNullOrEmpty(loginEmailInput.text))
        {
            loginEmailErrorImage?.gameObject.SetActive(true);
            loginErrorImage?.gameObject.SetActive(true);
            ShowFeedback("Digite o email para resetar a senha.");
            return;
        }

        ShowFeedback("Enviando email de reset...");
        FirebaseManager.Instance?.SendPasswordResetEmail(loginEmailInput.text);
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
        ClearInputErrors();

        bool hasError = false;
        if (string.IsNullOrEmpty(registerEmailInput.text)) { registerEmailErrorImage?.gameObject.SetActive(true); hasError = true; }
        if (string.IsNullOrEmpty(registerPasswordInput.text)) { registerPasswordErrorImage?.gameObject.SetActive(true); hasError = true; }
        if (string.IsNullOrEmpty(registerConfirmPasswordInput.text)) { registerConfirmPasswordErrorImage?.gameObject.SetActive(true); hasError = true; }

        if (hasError) { ShowFeedback("Preencha todos os campos."); return; }

        if (!IsValidEmail(registerEmailInput.text))
        {
            registerEmailErrorImage?.gameObject.SetActive(true);
            ShowFeedback("E-mail inválido.");
            return;
        }

        if (registerPasswordInput.text != registerConfirmPasswordInput.text)
        {
            registerPasswordErrorImage?.gameObject.SetActive(true);
            registerConfirmPasswordErrorImage?.gameObject.SetActive(true);
            ShowFeedback("As senhas não coincidem.");
            return;
        }

        if (registerPasswordInput.text.Length < 6)
        {
            registerPasswordErrorImage?.gameObject.SetActive(true);
            registerConfirmPasswordErrorImage?.gameObject.SetActive(true);
            ShowFeedback("A senha deve ter pelo menos 6 caracteres.");
            return;
        }

        ShowFeedback("Registrando...");
        FirebaseManager.Instance?.RegisterUser(registerEmailInput.text, registerPasswordInput.text);
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
        FirebaseManager.Instance?.Logout();
        ShowFeedback("Você foi deslogado.");
        ShowMainMenuPanel();
        SetGameplayManagersActive(false);
    }

    #endregion

    #region Gameplay Managers

    public void InitializeGameplayManagers()
    {
        SetGameplayManagersActive(true);
    }

    private void SetGameplayManagersActive(bool isActive)
    {
        uiManagerGO?.SetActive(isActive);
        playerGO?.SetActive(isActive);
    }

    #endregion

    #region Feedback e Utils

    public void ShowFeedback(string msg)
    {
        if (mensagemFeedback == null) return;
        mensagemFeedback.text = msg;
        mensagemFeedback.gameObject.SetActive(true);
        if (feedbackCoroutine != null) StopCoroutine(feedbackCoroutine);
        feedbackCoroutine = StartCoroutine(HideFeedbackAfterSeconds(3f));
    }

    public void ClearFeedback()
    {
        if (feedbackCoroutine != null) StopCoroutine(feedbackCoroutine);
        if (mensagemFeedback != null)
        {
            mensagemFeedback.text = "";
            mensagemFeedback.gameObject.SetActive(false);
        }
    }

    private IEnumerator HideFeedbackAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (mensagemFeedback != null)
        {
            mensagemFeedback.text = "";
            mensagemFeedback.gameObject.SetActive(false);
        }
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
    }

    private bool IsValidEmail(string email)
    {
        try { var addr = new System.Net.Mail.MailAddress(email); return addr.Address == email; }
        catch { return false; }
    }

    private void SetInputFieldPasswordMode(TMP_InputField input, bool visible)
    {
        if (input == null) return;
        input.contentType = visible ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;
        input.ForceLabelUpdate();
    }

    private void UpdateToggleIcon(Image img, bool visible)
    {
        if (img != null && eyeOpenSprite != null && eyeClosedSprite != null)
            img.sprite = visible ? eyeOpenSprite : eyeClosedSprite;
    }

    private void ClearInputErrors()
    {
        loginEmailErrorImage?.gameObject.SetActive(false);
        loginPasswordErrorImage?.gameObject.SetActive(false);
        loginErrorImage?.gameObject.SetActive(false);
        registerEmailErrorImage?.gameObject.SetActive(false);
        registerPasswordErrorImage?.gameObject.SetActive(false);
        registerConfirmPasswordErrorImage?.gameObject.SetActive(false);
    }

    #endregion

    #region Firebase Events

    private void OnFirebaseAuthFailed(string msg)
    {
        ClearInputErrors();
        ShowOnlyPanel(loginPanel);
        ResetFields();
        loginEmailErrorImage?.gameObject.SetActive(true);
        loginPasswordErrorImage?.gameObject.SetActive(true);
        loginErrorImage?.gameObject.SetActive(true);
        ShowFeedback(msg);
    }

    private void OnFirebaseAuthSuccess()
    {
        StartCoroutine(ShowLoadingThenPressToPlay());
    }

    private IEnumerator ShowLoadingThenPressToPlay()
    {
        loadingPanel?.SetActive(true);
        yield return new WaitForSeconds(4.6f);
        loadingPanel?.SetActive(false);
        ShowPressToPlayPanel();
        ShowFeedback("Login bem-sucedido! Aperte para jogar!");
    }

    private void OnFirebaseLogout()
    {
        ShowFeedback("Você foi deslogado.");
        ShowMainMenuPanel();
        SetGameplayManagersActive(false);
    }

    #endregion
}
