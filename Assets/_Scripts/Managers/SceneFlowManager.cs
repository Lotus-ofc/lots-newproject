using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SceneFlowManager : MonoBehaviour
{
    public static SceneFlowManager Instance;

    // --- UI Elements ---
    #region UI Elements
    [Header("Panels")]
    public GameObject loginPanel;
    public GameObject registerPanel;
    public GameObject mainMenuPanel;
    public GameObject pressToPlayPanel;
    public GameObject loadingPanel;
    public GameObject seasonsPanel;
    public GameObject inventoryPanel;
    public GameObject shopPanel;
    public GameObject selectionPanel;

    [Header("Canvases")]
    public Canvas gameplayCanvas;
    public Canvas mainMenuCanvas;

    [Header("Feedback")]
    public TextMeshProUGUI feedbackText;

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

    [Header("Gameplay Buttons")]
    public Button playButton;
    public Button seasonButton;
    public Button inventoryButton;
    public Button shopButton;
    public Button closePanelsButton;
    public Button backFromSeasonButton;
    public Button backFromInventoryButton;
    public Button backFromShopButton;
    public Button selectionButton;

    [Header("Sprites")]
    public Sprite eyeOpenSprite;
    public Sprite eyeClosedSprite;
    #endregion

    // --- Private Fields ---
    #region Private Fields
    private bool isLoginPasswordVisible;
    private bool isRegisterPasswordVisible;
    private bool isRegisterConfirmPasswordVisible;

    private Coroutine feedbackCoroutine;

    private Coroutine shopPanelCoroutine;
    private Coroutine inventoryPanelCoroutine;
    private Coroutine seasonsPanelCoroutine;
    private Coroutine selectionPanelCoroutine;
    #endregion

    // --- MonoBehaviour Methods ---
    #region MonoBehaviour Methods
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
        feedbackText?.gameObject.SetActive(false);
        loadingPanel?.SetActive(false);
        SetupButtonEvents();
        ResetInputFields();
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
    #endregion

    // --- Panel & Scene Flow Management ---
    #region Panel & Scene Flow Management
    private void SetupButtonEvents()
    {
        // Main Menu
        loginButton?.onClick.AddListener(OnLoginButtonClicked);
        loginBackButton?.onClick.AddListener(() => ShowOnlyPanel(mainMenuPanel));
        forgotPasswordButton?.onClick.AddListener(OnForgotPasswordClicked);
        loginTogglePasswordVisibilityButton?.onClick.AddListener(() => TogglePasswordVisibility(loginPasswordInput, ref isLoginPasswordVisible, loginTogglePasswordImage));
        
        // Register
        registerButton?.onClick.AddListener(OnRegisterButtonClicked);
        registerBackButton?.onClick.AddListener(() => ShowOnlyPanel(mainMenuPanel));
        registerTogglePasswordVisibilityButton1?.onClick.AddListener(() => TogglePasswordVisibility(registerPasswordInput, ref isRegisterPasswordVisible, registerTogglePasswordImage1));
        registerTogglePasswordVisibilityButton2?.onClick.AddListener(() => TogglePasswordVisibility(registerConfirmPasswordInput, ref isRegisterConfirmPasswordVisible, registerTogglePasswordImage2));
        
        // General
        logoutButton?.onClick.AddListener(OnLogoutButtonClicked);
        playButton?.onClick.AddListener(ShowGameplayPanel);
        
        // Gameplay
        selectionButton?.onClick.AddListener(ToggleSelectionPanel);
        seasonButton?.onClick.AddListener(() => ToggleGameplayPanel(seasonsPanel, ref seasonsPanelCoroutine));
        inventoryButton?.onClick.AddListener(() => ToggleGameplayPanel(inventoryPanel, ref inventoryPanelCoroutine));
        shopButton?.onClick.AddListener(() => ToggleGameplayPanel(shopPanel, ref shopPanelCoroutine));
        closePanelsButton?.onClick.AddListener(CloseSelectionPanel);
        
        // Back from panels
        backFromSeasonButton?.onClick.AddListener(BackFromSeasonPanel);
        backFromInventoryButton?.onClick.AddListener(BackFromInventoryPanel);
        backFromShopButton?.onClick.AddListener(BackFromShopPanel);
    }

    public void ShowOnlyPanel(GameObject panel)
    {
        // Deactivate all main panels
        loginPanel?.SetActive(false);
        registerPanel?.SetActive(false);
        mainMenuPanel?.SetActive(false);
        pressToPlayPanel?.SetActive(false);
        loadingPanel?.SetActive(false);
        gameplayCanvas?.gameObject.SetActive(false);

        // Activate the requested panel
        panel?.SetActive(true);
        ClearFeedback();
    }

    private void ToggleGameplayPanel(GameObject panel, ref Coroutine coroutine)
    {
        if (panel == null) return;

        if (panel.activeSelf)
        {
            ClosePanelAnimated(panel, ref coroutine);
            ShowSelectionPanel();
        }
        else
        {
            CloseAllGameplayPanels(panel);
            OpenPanelAnimated(panel, ref coroutine);
            HideSelectionPanel();
        }
    }

    public void ShowGameplayPanel()
    {
        mainMenuCanvas?.gameObject.SetActive(false);
        gameplayCanvas?.gameObject.SetActive(true);
        pressToPlayPanel?.SetActive(true);
        ShowFeedback("Bem-vindo à Gameplay!");
    }

    private void ToggleSelectionPanel()
    {
        if (selectionPanel == null) return;
        selectionPanel.SetActive(!selectionPanel.activeSelf);
    }

    private void HideSelectionPanel()
    {
        selectionPanel?.SetActive(false);
    }

    private void ShowSelectionPanel()
    {
        selectionPanel?.SetActive(true);
    }

    // Novo método para fechar o painel de seleção
public void CloseSelectionPanel()
{
    if (selectionPanel != null)
    {
        selectionPanel.SetActive(false);
    }
    // Você pode adicionar um feedback, se quiser
    ShowFeedback("Painel de seleção fechado.");
}

            public void BackFromSeasonPanel()
        {
            ClosePanelAnimated(seasonsPanel, ref seasonsPanelCoroutine);
            ShowSelectionPanel();
        }

        public void BackFromInventoryPanel()
        {
            ClosePanelAnimated(inventoryPanel, ref inventoryPanelCoroutine);
            ShowSelectionPanel();
        }

        public void BackFromShopPanel()
        {
            ClosePanelAnimated(shopPanel, ref shopPanelCoroutine);
            ShowSelectionPanel();
        }

    private void CloseAllGameplayPanels(GameObject except = null)
    {
        if (seasonsPanel != null && seasonsPanel.activeSelf && seasonsPanel != except)
        {
            ClosePanelAnimated(seasonsPanel, ref seasonsPanelCoroutine);
        }

        if (inventoryPanel != null && inventoryPanel.activeSelf && inventoryPanel != except)
        {
            ClosePanelAnimated(inventoryPanel, ref inventoryPanelCoroutine);
        }

        if (shopPanel != null && shopPanel.activeSelf && shopPanel != except)
        {
            ClosePanelAnimated(shopPanel, ref shopPanelCoroutine);
        }
    }
    #endregion

    // --- Login / Register / Logout Logic ---
    #region Login / Register / Logout
    private void OnLoginButtonClicked()
    {
        ClearInputErrors();

        if (string.IsNullOrEmpty(loginEmailInput.text) || string.IsNullOrEmpty(loginPasswordInput.text))
        {
            loginEmailErrorImage?.gameObject.SetActive(string.IsNullOrEmpty(loginEmailInput.text));
            loginPasswordErrorImage?.gameObject.SetActive(string.IsNullOrEmpty(loginPasswordInput.text));
            ShowFeedback("Preencha todos os campos.");
            return;
        }

        if (!IsValidEmail(loginEmailInput.text))
        {
            loginEmailErrorImage?.gameObject.SetActive(true);
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
    }
    #endregion

    // --- Feedback & Utility Methods ---
    #region Feedback & Utility Methods
    public void ShowFeedback(string msg)
    {
        if (feedbackText == null) return;

        feedbackText.text = msg;
        feedbackText.gameObject.SetActive(true);

        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
        }
        feedbackCoroutine = StartCoroutine(HideFeedbackAfterSeconds(3f));
    }

    public void ClearFeedback()
    {
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
        }
        feedbackText?.gameObject.SetActive(false);
    }

    private IEnumerator HideFeedbackAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        feedbackText?.gameObject.SetActive(false);
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

    private void ResetInputFields()
    {
        if (loginEmailInput != null) loginEmailInput.text = "";
        if (loginPasswordInput != null) loginPasswordInput.text = "";
        if (registerEmailInput != null) registerEmailInput.text = "";
        if (registerPasswordInput != null) registerPasswordInput.text = "";
        if (registerConfirmPasswordInput != null) registerConfirmPasswordInput.text = "";

        isLoginPasswordVisible = false;
        isRegisterPasswordVisible = false;
        isRegisterConfirmPasswordVisible = false;
    }

    private bool IsValidEmail(string email)
    {
        try 
        {
            var addr = new System.Net.Mail.MailAddress(email); 
            return addr.Address == email; 
        }
        catch 
        { 
            return false; 
        }
    }
    #endregion

    // --- Firebase Event Handlers ---
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

        ShowOnlyPanel(pressToPlayPanel);
        mainMenuCanvas?.gameObject.SetActive(false);
        gameplayCanvas?.gameObject.SetActive(true);
        ShowFeedback("Login bem-sucedido! Aperte para jogar!");
    }

    private void OnFirebaseLogout()
    {
        ShowOnlyPanel(mainMenuPanel);
        ShowFeedback("Você foi deslogado.");
    }
    #endregion

    // --- Panel Animation ---
    #region Panel Animation
    private void OpenPanelAnimated(GameObject panel, ref Coroutine panelCoroutine)
    {
        if (panelCoroutine != null)
        {
            StopCoroutine(panelCoroutine);
        }
        panelCoroutine = StartCoroutine(AnimatePanelIn(panel));
    }

    private void ClosePanelAnimated(GameObject panel, ref Coroutine panelCoroutine)
    {
        if (panelCoroutine != null)
        {
            StopCoroutine(panelCoroutine);
        }
        panelCoroutine = StartCoroutine(AnimatePanelOut(panel));
    }
    
    private IEnumerator AnimatePanelIn(GameObject panel)
    {
        if (panel == null) yield break;

        panel.SetActive(true);
        RectTransform rt = panel.GetComponent<RectTransform>();
        if (rt == null) yield break;

        float screenHeight = Screen.height;
        Vector2 startPos = new Vector2(0, screenHeight);
        Vector2 endPos = Vector2.zero;

        float duration = 0.3f;
        float elapsed = 0f;

        rt.anchoredPosition = startPos;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            rt.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        rt.anchoredPosition = endPos;
    }

    private IEnumerator AnimatePanelOut(GameObject panel)
    {
        if (panel == null) yield break;

        RectTransform rt = panel.GetComponent<RectTransform>();
        if (rt == null) yield break;

        float screenHeight = Screen.height;
        Vector2 startPos = rt.anchoredPosition;
        Vector2 endPos = new Vector2(0, -screenHeight);

        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            rt.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        rt.anchoredPosition = endPos;
        panel.SetActive(false);
    }
    #endregion
}