// SceneFlowManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Globalization;
using Mapbox.Unity.Map; // Adicionado para AbstractMap

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

    [Header("Press To Play")]
    public Button playButton;

    [Header("Sprites")]
    public Sprite eyeOpenSprite;
    public Sprite eyeClosedSprite;

    [Header("Managers de Gameplay Controlados (GameObjects)")]
    public GameObject locationPermissionManagerGO; // Renomeado para clareza
    public GameObject currencyManagerGO; // Renomeado para clareza
    public GameObject locationProviderGO; // Renomeado para clareza: representa o GameObject que tem os provedores
    public GameObject uiManagerGO; // Renomeado para clareza
    public GameObject playerLocationUpdaterGO; // Renomeado para clareza
    public GameObject playerGO;

    // Referências diretas para os scripts dos managers (obtidas via GetComponent)
    private LocationPermissionManager _locationPermissionManager;
    private PlayerCurrencyManager _playerCurrencyManager;
    private PlayerLocationProvider _playerLocationProvider;
    private EditorLocationProvider _editorLocationProvider;
    private PlayerLocationUpdater _playerLocationUpdater;
    
    private bool isLoginPasswordVisible = false;
    private bool isRegisterPasswordVisible = false;
    private bool isRegisterConfirmPasswordVisible = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("SceneFlowManager: Instância criada e DontDestroyOnLoad ativado.");
        }
        else
        {
            Debug.LogWarning("SceneFlowManager: Instância já existe. Destruindo esta nova.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        mensagemFeedback.text = "";
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

        // Começa desativando os managers
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
        else
        {
            Debug.LogError("SceneFlowManager: FirebaseManager.Instance é NULO no OnEnable! Verifique a ordem dos scripts.");
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

        ClearFeedback();
    }

    public void ShowMainMenuPanel()
    {
        ShowOnlyPanel(mainMenuPanel);
        ResetFields();
    }

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

    public void ShowPressToPlayPanel()
    {
        ShowOnlyPanel(pressToPlayPanel);
        InitializeGameplayManagers(); // Inicializa os managers de gameplay
    }

    public void ShowGameplayPanel()
    {
        ShowOnlyPanel(gameplayPanel);
        ShowFeedback("Bem-vindo à Gameplay!");
        // Não chame InitializeGameplayManagers() novamente aqui se já foi chamado em ShowPressToPlayPanel
        // A menos que você queira re-inicializar
    }

    #endregion

    #region Login

    private void OnLoginButtonClicked()
    {
        if (string.IsNullOrEmpty(loginEmailInput.text) || string.IsNullOrEmpty(loginPasswordInput.text))
        {
            ShowFeedback("Preencha email e senha.");
            loginErrorImage?.gameObject.SetActive(true);
            return;
        }

        loginErrorImage?.gameObject.SetActive(false);
        ShowFeedback("Tentando logar...");

        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.LoginUser(loginEmailInput.text, loginPasswordInput.text);
        }
        else
        {
            ShowFeedback("Erro interno: Serviço de autenticação indisponível.");
        }
    }

    private void OnForgotPasswordClicked()
    {
        if (string.IsNullOrEmpty(loginEmailInput.text))
        {
            ShowFeedback("Digite o email para resetar a senha.");
            return;
        }

        ShowFeedback("Enviando email de reset...");

        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.SendPasswordResetEmail(loginEmailInput.text);
        }
        else
        {
            ShowFeedback("Erro interno: Serviço de autenticação indisponível.");
        }
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
            ShowFeedback("Preencha todos os campos.");
            return;
        }

        if (registerPasswordInput.text != registerConfirmPasswordInput.text)
        {
            ShowFeedback("As senhas não conferem.");
            return;
        }

        ShowFeedback("Registrando...");

        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.RegisterUser(registerEmailInput.text, registerPasswordInput.text);
            ShowLoginPanel();
            ShowFeedback("Usuário registrado com sucesso! Por favor, faça login.");
        }
        else
        {
            ShowFeedback("Erro interno: Serviço de autenticação indisponível.");
        }
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
        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.Logout();
        }
        else
        {
            ShowFeedback("Erro interno: Serviço de autenticação indisponível.");
        }
    }

    #endregion

    #region Firebase Events

    private void OnFirebaseAuthFailed(string msg)
    {
        ShowFeedback(msg);
        loginErrorImage?.gameObject.SetActive(true);
        ShowOnlyPanel(loginPanel);
    }

    private void OnFirebaseAuthSuccess()
    {
        StartCoroutine(ShowLoadingThenPressToPlay());
    }

    private IEnumerator ShowLoadingThenPressToPlay()
    {
        loadingPanel?.SetActive(true);
        ClearFeedback();

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

    // Centraliza ativação e obtenção de referências dos managers
    public void InitializeGameplayManagers()
    {
        SetGameplayManagersActive(true); // Ativa os GameObjects primeiro

        // Obter referências aos scripts dos managers
        if (locationPermissionManagerGO != null)
        {
            _locationPermissionManager = locationPermissionManagerGO.GetComponent<LocationPermissionManager>();
            if (_locationPermissionManager == null) Debug.LogError("SceneFlowManager: LocationPermissionManager não encontrado no GameObject 'locationPermissionManagerGO'.");
        }
        else Debug.LogWarning("SceneFlowManager: GameObject 'locationPermissionManagerGO' não atribuído no Inspector.");

        if (currencyManagerGO != null)
        {
            _playerCurrencyManager = currencyManagerGO.GetComponent<PlayerCurrencyManager>();
            if (_playerCurrencyManager == null) Debug.LogError("SceneFlowManager: PlayerCurrencyManager não encontrado no GameObject 'currencyManagerGO'.");
        }
        else Debug.LogWarning("SceneFlowManager: GameObject 'currencyManagerGO' não atribuído no Inspector.");

        if (locationProviderGO != null)
        {
            // Tenta obter o EditorLocationProvider (principalmente para uso no Editor)
            _editorLocationProvider = locationProviderGO.GetComponent<EditorLocationProvider>();
            if (_editorLocationProvider == null)
            {
                // Se não encontrou o EditorLocationProvider, tenta o PlayerLocationProvider (principalmente para mobile)
                Debug.LogWarning("SceneFlowManager: EditorLocationProvider não encontrado no 'locationProviderGO'. Tentando PlayerLocationProvider...");
                _playerLocationProvider = locationProviderGO.GetComponent<PlayerLocationProvider>();
                if (_playerLocationProvider == null)
                {
                    Debug.LogError("SceneFlowManager: Nenhum provedor de localização (EditorLocationProvider ou PlayerLocationProvider) encontrado no 'locationProviderGO'.");
                }
            }
        }
        else Debug.LogWarning("SceneFlowManager: GameObject 'locationProviderGO' não atribuído no Inspector.");

        if (playerLocationUpdaterGO != null)
        {
            _playerLocationUpdater = playerLocationUpdaterGO.GetComponent<PlayerLocationUpdater>();
            if (_playerLocationUpdater == null) Debug.LogError("SceneFlowManager: PlayerLocationUpdater não encontrado no GameObject 'playerLocationUpdaterGO'.");
        }
        else Debug.LogWarning("SceneFlowManager: GameObject 'playerLocationUpdaterGO' não atribuído no Inspector.");
        
        // Agora, você pode interagir com os managers através das suas referências privadas (_managerName)
        // Por exemplo:
        // if (_playerCurrencyManager != null) _playerCurrencyManager.InitializeEconomy();
        // if (_uiManager != null) _uiManager.SetupUI(); // Se UI Manager for um script com método de setup
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

    #region Feedback e Utils

    public void ShowFeedback(string msg)
    {
        if (mensagemFeedback != null)
        {
            mensagemFeedback.text = msg;
        }
    }

    public void ClearFeedback()
    {
        if (mensagemFeedback != null)
        {
            mensagemFeedback.text = "";
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

    // Método para converter vírgula em ponto em strings numéricas (coordenadas)
    public static string FixDecimalSeparator(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return input.Replace(',', '.');
    }

    #endregion
}