using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Globalization;
using Mapbox.Unity.Map; // Adicionado para AbstractMap (garanta que está instalado ou remova se não for usar)

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
    public Image loginErrorImage; // Imagem de erro geral, como um ícone de exclamação
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

    [Header("Managers de Gameplay Controlados (GameObjects)")]
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
        Debug.Log("SceneFlowManager: Start chamado.");
        if (mensagemFeedback != null)
        {
            mensagemFeedback.text = "";
            mensagemFeedback.gameObject.SetActive(false); // Garante que começa desativado
        }
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
            Debug.Log("SceneFlowManager: Inscrevendo-se em eventos do FirebaseManager.");
            FirebaseManager.Instance.OnAuthFailed += OnFirebaseAuthFailed;
            FirebaseManager.Instance.OnAuthSuccess += OnFirebaseAuthSuccess;
            FirebaseManager.Instance.OnLogout += OnFirebaseLogout;
        }
        else
        {
            Debug.LogWarning("SceneFlowManager: FirebaseManager.Instance é null em OnEnable. Eventos não inscritos.");
        }
    }

    private void OnDisable()
    {
        if (FirebaseManager.Instance != null)
        {
            Debug.Log("SceneFlowManager: Desinscrevendo-se de eventos do FirebaseManager.");
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
        Debug.Log($"SceneFlowManager: Painel '{panel?.name}' ativado.");

        ClearFeedback(); // Limpa feedback ao mudar de painel
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
        InitializeGameplayManagers();
    }

    public void ShowGameplayPanel()
    {
        ShowOnlyPanel(gameplayPanel);
        ShowFeedback("Bem-vindo à Gameplay!");
    }

    #endregion

    #region Login

    private void OnLoginButtonClicked()
    {
        Debug.Log("SceneFlowManager: Botão de Login clicado.");
        ClearInputErrors();

        bool hasError = false;

        if (string.IsNullOrEmpty(loginEmailInput.text))
        {
            loginEmailErrorImage?.gameObject.SetActive(true);
            hasError = true;
        }

        if (string.IsNullOrEmpty(loginPasswordInput.text))
        {
            loginPasswordErrorImage?.gameObject.SetActive(true);
            hasError = true;
        }

        if (hasError)
        {
            ShowFeedback("Preencha todos os campos.");
            loginErrorImage?.gameObject.SetActive(true);
            StartCoroutine(HideFeedbackAndClearErrorsAfterSeconds(3f));
            return;
        }

        if (!IsValidEmail(loginEmailInput.text))
        {
            loginEmailErrorImage?.gameObject.SetActive(true);
            ShowFeedback("E-mail inválido.");
            loginErrorImage?.gameObject.SetActive(true);
            StartCoroutine(HideFeedbackAndClearErrorsAfterSeconds(3f));
            return;
        }

        ShowFeedback("Tentando logar...");
        FirebaseManager.Instance?.LoginUser(loginEmailInput.text, loginPasswordInput.text);
    }

    private void OnForgotPasswordClicked()
    {
        Debug.Log("SceneFlowManager: Botão Esqueceu a Senha clicado.");
        ClearInputErrors();

        if (string.IsNullOrEmpty(loginEmailInput.text))
        {
            loginEmailErrorImage?.gameObject.SetActive(true);
            ShowFeedback("Digite o email para resetar a senha.");
            loginErrorImage?.gameObject.SetActive(true);
            StartCoroutine(HideFeedbackAndClearErrorsAfterSeconds(3f));
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
            loginErrorImage?.gameObject.SetActive(true);
            StartCoroutine(HideFeedbackAndClearErrorsAfterSeconds(3f));
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
        Debug.Log("SceneFlowManager: Botão de Registrar clicado.");
        ClearInputErrors();

        bool hasError = false;

        if (string.IsNullOrEmpty(registerEmailInput.text))
        {
            registerEmailErrorImage?.gameObject.SetActive(true);
            hasError = true;
        }

        if (string.IsNullOrEmpty(registerPasswordInput.text))
        {
            registerPasswordErrorImage?.gameObject.SetActive(true);
            hasError = true;
        }

        if (string.IsNullOrEmpty(registerConfirmPasswordInput.text))
        {
            registerConfirmPasswordErrorImage?.gameObject.SetActive(true);
            hasError = true;
        }

        if (hasError)
        {
            ShowFeedback("Preencha todos os campos.");
            StartCoroutine(HideFeedbackAfterSeconds(3f));
            return;
        }

        if (!IsValidEmail(registerEmailInput.text))
        {
            registerEmailErrorImage?.gameObject.SetActive(true);
            ShowFeedback("E-mail inválido.");
            StartCoroutine(HideFeedbackAfterSeconds(3f));
            return;
        }

        if (registerPasswordInput.text != registerConfirmPasswordInput.text)
        {
            registerPasswordErrorImage?.gameObject.SetActive(true);
            registerConfirmPasswordErrorImage?.gameObject.SetActive(true);
            ShowFeedback("As senhas não coincidem."); // Ajustado para ser mais claro
            StartCoroutine(HideFeedbackAfterSeconds(3f));
            return;
        }
        
        if (registerPasswordInput.text.Length < 6) // Exemplo: senha mínima de 6 caracteres
        {
            registerPasswordErrorImage?.gameObject.SetActive(true);
            registerConfirmPasswordErrorImage?.gameObject.SetActive(true);
            ShowFeedback("A senha deve ter pelo menos 6 caracteres.");
            StartCoroutine(HideFeedbackAfterSeconds(3f));
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

    #region Helpers

    private void ClearInputErrors()
    {
        loginEmailErrorImage?.gameObject.SetActive(false);
        loginPasswordErrorImage?.gameObject.SetActive(false);
        loginErrorImage?.gameObject.SetActive(false);

        registerEmailErrorImage?.gameObject.SetActive(false);
        registerPasswordErrorImage?.gameObject.SetActive(false);
        registerConfirmPasswordErrorImage?.gameObject.SetActive(false);
        Debug.Log("SceneFlowManager: Erros de input limpos.");
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

    #region Logout

    private void OnLogoutButtonClicked()
    {
        Debug.Log("SceneFlowManager: Botão de Logout clicado.");
        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.Logout();
        }
        else
        {
            ShowFeedback("Erro interno: Serviço de autenticação indisponível.");
            StartCoroutine(HideFeedbackAfterSeconds(3f));
        }
    }

    #endregion

    #region Firebase Events

    private void OnFirebaseAuthFailed(string msg)
    {
        Debug.LogWarning($"SceneFlowManager: OnFirebaseAuthFailed invocado com mensagem: {msg}");

        ClearInputErrors(); // Limpa quaisquer erros visuais de inputs individuais ou gerais
        ShowOnlyPanel(loginPanel); // Garante que estamos no painel de login
        ResetFields(); // Limpa os campos de input, desativando os ícones de visibilidade de senha também.

        // Ativa os indicadores visuais de erro relevantes para o painel de login
        // Dependendo da 'msg', você pode ser mais granular aqui.
        // Como o FirebaseManager já está enviando "E-mail e ou senha incorretos" ou "Email inválido.",
        // podemos ativar ambos os indicadores de erro de campo de senha/email e o geral para cobrir todos os casos.
        if (msg.Contains("Email inválido")) // Verifica se a mensagem de erro específica foi passada
        {
            loginEmailErrorImage?.gameObject.SetActive(true);
        }
        else // Para "E-mail e ou senha incorretos" ou outros erros gerais de login
        {
            loginEmailErrorImage?.gameObject.SetActive(true);
            loginPasswordErrorImage?.gameObject.SetActive(true);
        }
        loginErrorImage?.gameObject.SetActive(true); // Ativa o ícone de erro geral

        // EXIBE A MENSAGEM RECEBIDA DO FIREBASE MANAGER
        ShowFeedback(msg); 
        
        // Garante que o coroutine HideFeedbackAndClearErrorsAfterSeconds seja o único ativo e que ele limpe os erros do login panel.
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
        }
        feedbackCoroutine = StartCoroutine(HideFeedbackAndClearErrorsAfterSeconds(3f));
    }

    // Coroutine para esconder o feedback e limpar os erros visuais do painel de login
    private IEnumerator HideFeedbackAndClearErrorsAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Debug.Log("SceneFlowManager: Escondendo feedback e limpando erros visuais.");

        if (mensagemFeedback != null)
        {
            mensagemFeedback.text = "";
            mensagemFeedback.gameObject.SetActive(false);
        }

        // Desativa todas as imagens de erro específicas do login
        loginEmailErrorImage?.gameObject.SetActive(false);
        loginPasswordErrorImage?.gameObject.SetActive(false);
        loginErrorImage?.gameObject.SetActive(false);
    }

    private void OnFirebaseAuthSuccess()
    {
        Debug.Log("SceneFlowManager: OnFirebaseAuthSuccess invocado.");
        StartCoroutine(ShowLoadingThenPressToPlay());
    }

    private IEnumerator ShowLoadingThenPressToPlay()
    {
        Debug.Log("SceneFlowManager: Iniciando coroutine ShowLoadingThenPressToPlay.");
        loadingPanel?.SetActive(true);
        ClearFeedback(); // Garante que nenhum feedback anterior esteja visível

        yield return new WaitForSeconds(4.6f); // Tempo do seu loading

        loadingPanel?.SetActive(false);
        ShowPressToPlayPanel();
        ShowFeedback("Login bem-sucedido! Aperte para jogar!");
    }

    private void OnFirebaseLogout()
    {
        Debug.Log("SceneFlowManager: OnFirebaseLogout invocado.");
        ShowFeedback("Você foi deslogado.");
        ShowMainMenuPanel();
        SetGameplayManagersActive(false);
    }

    #endregion

    public void InitializeGameplayManagers()
    {
        SetGameplayManagersActive(true);

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
            _editorLocationProvider = locationProviderGO.GetComponent<EditorLocationProvider>();
            if (_editorLocationProvider == null)
            {
                Debug.LogWarning("SceneFlowManager: EditorLocationProvider não encontrado no 'locationProviderGO'. Tentando PlayerLocationProvider...");
                _playerLocationProvider = locationProviderGO.GetComponent<PlayerLocationProvider>();
                if (_playerLocationProvider == null)
                {
                    Debug.LogError("SceneFlowManager: Nenhum provedor de localização encontrado no 'locationProviderGO'.");
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
            mensagemFeedback.gameObject.SetActive(true);
            Debug.Log($"SceneFlowManager: Exibindo feedback: '{msg}'");

            if (feedbackCoroutine != null)
                StopCoroutine(feedbackCoroutine);

            feedbackCoroutine = StartCoroutine(HideFeedbackAfterSeconds(3f));
        }
        else
        {
            Debug.LogError("SceneFlowManager: mensagemFeedback é null! Não é possível exibir feedback.");
        }
    }

    public void ClearFeedback()
    {
        if (feedbackCoroutine != null)
            StopCoroutine(feedbackCoroutine);

        if (mensagemFeedback != null)
        {
            mensagemFeedback.text = "";
            mensagemFeedback.gameObject.SetActive(false);
            Debug.Log("SceneFlowManager: Feedback limpo.");
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

        Debug.Log("SceneFlowManager: Campos de input resetados e visibilidade de senha desativada.");
        // ClearInputErrors() é chamado separadamente quando necessário.
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

    public static string FixDecimalSeparator(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return input.Replace(',', '.');
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

    #endregion
}