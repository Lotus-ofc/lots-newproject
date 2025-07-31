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
    public GameObject loadingPanel;

    [Header("Canvas")]
    public Canvas gameplayCanvas;
    public Canvas mainMenuCanvas;

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

    private bool isLoginPasswordVisible;
    private bool isRegisterPasswordVisible;
    private bool isRegisterConfirmPasswordVisible;
    private Coroutine feedbackCoroutine;

    [Header("Painéis gameplay")]
    public GameObject seasonsPanel;
    public GameObject inventoryPanel;
    public GameObject shopPanel;

    [Header("Botão para voltar")]
    public Button closePanelsButton;

    [Header("Botões Gameplay")]
    public Button seasonButton;
    public Button inventoryButton;
    public Button shopButton;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    private void Start()
    {
        mensagemFeedback?.gameObject.SetActive(false);
        loadingPanel?.SetActive(false);
        SetupButtonEvents();
        ResetFields();
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

    private void SetupButtonEvents()
    {
        playButton?.onClick.AddListener(ShowGameplayPanel);
        loginButton?.onClick.AddListener(OnLoginButtonClicked);
        loginBackButton?.onClick.AddListener(() => ShowOnlyPanel(mainMenuPanel));
        forgotPasswordButton?.onClick.AddListener(OnForgotPasswordClicked);
        loginTogglePasswordVisibilityButton?.onClick.AddListener(() => TogglePasswordVisibility(loginPasswordInput, ref isLoginPasswordVisible, loginTogglePasswordImage));
        registerButton?.onClick.AddListener(OnRegisterButtonClicked);
        registerBackButton?.onClick.AddListener(() => ShowOnlyPanel(mainMenuPanel));
        registerTogglePasswordVisibilityButton1?.onClick.AddListener(() => TogglePasswordVisibility(registerPasswordInput, ref isRegisterPasswordVisible, registerTogglePasswordImage1));
        registerTogglePasswordVisibilityButton2?.onClick.AddListener(() => TogglePasswordVisibility(registerConfirmPasswordInput, ref isRegisterConfirmPasswordVisible, registerTogglePasswordImage2));
        logoutButton?.onClick.AddListener(OnLogoutButtonClicked);

        // Botões gameplay
        seasonButton?.onClick.AddListener(ToggleSeasonsPanel);
        inventoryButton?.onClick.AddListener(ToggleInventoryPanel);
        closePanelsButton?.onClick.AddListener(CloseAllPanelsAndShowButtons);
    }

    private void ShowOnlyPanel(GameObject panel)
    {
        loginPanel?.SetActive(false);
        registerPanel?.SetActive(false);
        mainMenuPanel?.SetActive(false);
        pressToPlayPanel?.SetActive(false);
        loadingPanel?.SetActive(false);
        gameplayCanvas?.gameObject.SetActive(false);

        panel?.SetActive(true);
        ClearFeedback();
    }

    public void ShowGameplayPanel()
    {
        gameplayCanvas?.gameObject.SetActive(true);
        ClearFeedback();
        ShowFeedback("Bem-vindo à Gameplay!");
    }

    #region Login / Register / Logout

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

    private void OnForgotPasswordClicked()
    {
        if (string.IsNullOrEmpty(loginEmailInput.text))
        {
            loginEmailErrorImage?.gameObject.SetActive(true);
            ShowFeedback("Digite o email para resetar a senha.");
            return;
        }

        ShowFeedback("Enviando email de reset...");
        FirebaseManager.Instance?.SendPasswordResetEmail(loginEmailInput.text);
    }

    private void OnLogoutButtonClicked()
    {
        FirebaseManager.Instance?.Logout();
        ShowOnlyPanel(mainMenuPanel);
        ShowFeedback("Você foi deslogado.");
    }

    public void ShowLoginPanel()
    {
        ShowOnlyPanel(loginPanel);
    }

    #endregion

    #region Feedback / Utils

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
        mensagemFeedback?.gameObject.SetActive(false);
    }

    private IEnumerator HideFeedbackAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        mensagemFeedback?.gameObject.SetActive(false);
    }

    private void TogglePasswordVisibility(TMP_InputField input, ref bool isVisible, Image icon)
    {
        isVisible = !isVisible;
        input.contentType = isVisible ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;
        input.ForceLabelUpdate();
        if (icon != null) icon.sprite = isVisible ? eyeOpenSprite : eyeClosedSprite;
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
    }

    private bool IsValidEmail(string email)
    {
        try { var addr = new System.Net.Mail.MailAddress(email); return addr.Address == email; }
        catch { return false; }
    }

    #endregion

    #region Botões Gameplay

    public void ToggleSeasonsPanel()
    {
        if (seasonsPanel == null || seasonButton == null) return;

        bool isActive = seasonsPanel.activeSelf;
        seasonsPanel.SetActive(!isActive);
        seasonButton.gameObject.SetActive(isActive);
    }

    public void ToggleInventoryPanel()
{
    if (inventoryPanel == null) return;

    bool isActive = inventoryPanel.activeSelf;
    inventoryPanel.SetActive(!isActive);
}



    public void CloseAllPanelsAndShowButtons()
{
    // Fecha todos os painéis de gameplay, estejam ativos ou não
    if (inventoryPanel != null) inventoryPanel.SetActive(false);
    if (shopPanel != null) shopPanel.SetActive(false);
    if (seasonsPanel != null) seasonsPanel.SetActive(false);

    // Reativa todos os botões de gameplay
    if (inventoryButton != null) inventoryButton.gameObject.SetActive(true);
    if (seasonButton != null) seasonButton.gameObject.SetActive(true);
    if (shopButton != null) shopButton.gameObject.SetActive(true);

    // (Opcional) Feedback visual se quiser
    ShowFeedback("Todos os painéis foram fechados.");
}

public void ToggleShopPanel()
{
    if (shopPanel == null || shopButton == null) return;

    bool isActive = shopPanel.activeSelf;

    if (isActive)
    {
        // Se está aberto, fecha e mostra o botão
        shopPanel.SetActive(false);
        shopButton.gameObject.SetActive(true);
    }
    else
    {
        // Se está fechado, abre e oculta o botão
        shopPanel.SetActive(true);
        shopButton.gameObject.SetActive(false);
    }
}

    #endregion

    #region Firebase Events

    private void OnFirebaseAuthFailed(string msg)
    {
        ClearInputErrors();
        ShowOnlyPanel(loginPanel);
        ShowFeedback(msg);
    }

    private void OnFirebaseAuthSuccess()
    {
        StartCoroutine(ShowLoadingThenPressToPlay());
    }

    private IEnumerator ShowLoadingThenPressToPlay()
    {
        ShowOnlyPanel(loadingPanel);
        yield return new WaitForSeconds(4.6f);

        mainMenuCanvas?.gameObject.SetActive(false);
        gameplayCanvas?.gameObject.SetActive(true);
        pressToPlayPanel?.SetActive(true);

        ShowFeedback("Login bem-sucedido! Aperte para jogar!");
    }

    private void OnFirebaseLogout()
    {
        ShowOnlyPanel(mainMenuPanel);
        ShowFeedback("Você foi deslogado.");
    }

    #endregion
}
