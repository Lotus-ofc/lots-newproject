using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Globalization;
using Mapbox.Unity.Map;
using UnityEngine.Android;

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

    [Header("Managers de Gameplay Controlados")]
    public GameObject locationPermissionManagerGO;
    public GameObject currencyManagerGO;
    public GameObject locationProviderGO;
    public GameObject uiManagerGO;
    public GameObject playerLocationUpdaterGO;
    public GameObject playerGO;

    private LocationPermissionManager _locationPermissionManager;
    private PlayerCurrencyManager _playerCurrencyManager;
    private PlayerLocationProvider _playerLocationProvider;
    private EditorLocationProvider _editorLocationProvider;
    private PlayerLocationUpdater _playerLocationUpdater;

    private bool locationPermissionGranted = false;

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
            Debug.Log("[SceneFlowManager] Instância criada.");
        }
        else
        {
            Debug.LogWarning("[SceneFlowManager] Instância duplicada, destruindo.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Debug.Log("[SceneFlowManager] Start inicializado.");
        mensagemFeedback.text = "";
        mensagemFeedback.gameObject.SetActive(false);
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

        // Configura permissão de localização
        if (locationPermissionManagerGO != null)
        {
            _locationPermissionManager = locationPermissionManagerGO.GetComponent<LocationPermissionManager>();
            if (_locationPermissionManager != null)
            {
                Debug.Log("[SceneFlowManager] Inscrevendo em LocationPermissionManager.");
                LocationPermissionManager.OnPermissionResult += HandleLocationPermissionResult;
                _locationPermissionManager.CheckAndRequestPermission();
            }
            else Debug.LogError("[SceneFlowManager] LocationPermissionManager não encontrado no GameObject!");
        }
        else Debug.LogWarning("[SceneFlowManager] locationPermissionManagerGO não atribuído.");
    }

    private void OnDestroy()
    {
        LocationPermissionManager.OnPermissionResult -= HandleLocationPermissionResult;
    }

    #region Painéis
    private void ShowOnlyPanel(GameObject panel)
    {
        loginPanel?.SetActive(false);
        registerPanel?.SetActive(false);
        mainMenuPanel?.SetActive(false);
        pressToPlayPanel?.SetActive(false);
        gameplayPanel?.SetActive(false);
        loadingPanel?.SetActive(false);

        panel?.SetActive(true);
        Debug.Log($"[SceneFlowManager] Painel ativado: {panel?.name}");
        ClearFeedback();
    }

    public void ShowMainMenuPanel() { ShowOnlyPanel(mainMenuPanel); ResetFields(); }
    public void ShowLoginPanel() { ShowOnlyPanel(loginPanel); ResetFields(); }
    public void ShowRegisterPanel() { ShowOnlyPanel(registerPanel); ResetFields(); }
    public void ShowPressToPlayPanel() { ShowOnlyPanel(pressToPlayPanel); InitializeGameplayManagers(); }
    public void ShowGameplayPanel() { ShowOnlyPanel(gameplayPanel); ShowFeedback("Bem-vindo à Gameplay!"); }
    #endregion

    private void HandleLocationPermissionResult(bool granted)
    {
        locationPermissionGranted = granted;
        Debug.Log($"[SceneFlowManager] Permissão de localização: {(granted ? "Concedida ✅" : "Negada ❌")}");

        if (granted)
        {
            ShowFeedback("Permissão de localização concedida! 🌍");
        }
        else
        {
            ShowFeedback("Permissão negada. Algumas funções podem não funcionar.");
        }
    }

    public void InitializeGameplayManagers()
    {
        SetGameplayManagersActive(true);

        if (locationPermissionGranted)
        {
            Debug.Log("[SceneFlowManager] Inicializando provedores de localização.");
            if (locationProviderGO != null)
            {
                _editorLocationProvider = locationProviderGO.GetComponent<EditorLocationProvider>();
                if (_editorLocationProvider == null)
                {
                    _playerLocationProvider = locationProviderGO.GetComponent<PlayerLocationProvider>();
                    if (_playerLocationProvider == null)
                    {
                        Debug.LogError("[SceneFlowManager] Nenhum provedor de localização encontrado.");
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("[SceneFlowManager] Providers de localização não inicializados: permissão negada.");
        }
    }

    private void SetGameplayManagersActive(bool isActive)
    {
        locationPermissionManagerGO?.SetActive(isActive);
        currencyManagerGO?.SetActive(isActive);
        locationProviderGO?.SetActive(isActive);
        uiManagerGO?.SetActive(isActive);
        playerLocationUpdaterGO?.SetActive(isActive);
        playerGO?.SetActive(isActive);
    }

    // Restante do seu fluxo: login, register, feedback, etc.
    // Use o que já estava 100% funcionando.

    #region Feedback
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

        mensagemFeedback.text = "";
        mensagemFeedback.gameObject.SetActive(false);
    }

    private IEnumerator HideFeedbackAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        ClearFeedback();
    }
    #endregion

    // Mantém seu fluxo de login/register/reset intacto
}
