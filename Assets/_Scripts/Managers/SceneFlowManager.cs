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

    [Header("Fade")]
    public FadeController fadeController;

    [Header("Feedback")]
    public TextMeshProUGUI mensagemFeedback;

    [Header("Login Inputs")]
    public TMP_InputField loginEmailInput;
    public TMP_InputField loginPasswordInput;
    public Image loginErrorImage;  // Imagem vermelha que aparece quando erro

    [Header("Login Buttons")]
    public Button loginButton;
    public Button loginBackButton;
    public Button forgotPasswordButton;
    public Button loginTogglePasswordVisibilityButton; // botão olho

    [Header("Register Inputs")]
    public TMP_InputField registerEmailInput;
    public TMP_InputField registerPasswordInput;
    public TMP_InputField registerConfirmPasswordInput;

    [Header("Register Buttons")]
    public Button registerButton;
    public Button registerBackButton;
    public Button registerTogglePasswordVisibilityButton1; // olho senha
    public Button registerTogglePasswordVisibilityButton2; // olho confirmar senha

    // Controle de visibilidade das senhas
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

        // Ativa listeners dos botões
        loginButton.onClick.AddListener(OnLoginButtonClicked);
        loginBackButton.onClick.AddListener(ShowMainMenuPanel);
        forgotPasswordButton.onClick.AddListener(OnForgotPasswordClicked);
        loginTogglePasswordVisibilityButton.onClick.AddListener(ToggleLoginPasswordVisibility);

        registerButton.onClick.AddListener(OnRegisterButtonClicked);
        registerBackButton.onClick.AddListener(ShowMainMenuPanel);
        registerTogglePasswordVisibilityButton1.onClick.AddListener(ToggleRegisterPasswordVisibility);
        registerTogglePasswordVisibilityButton2.onClick.AddListener(ToggleRegisterConfirmPasswordVisibility);

        // Esconde imagem de erro no início
        if (loginErrorImage != null) loginErrorImage.gameObject.SetActive(false);

        // Garanta que os inputs com senha estejam no modo password oculto no início
        SetInputFieldPasswordMode(loginPasswordInput, false);
        SetInputFieldPasswordMode(registerPasswordInput, false);
        SetInputFieldPasswordMode(registerConfirmPasswordInput, false);
    }

    #region Painéis
    public void ShowLoginPanel()
    {
        loginPanel.SetActive(true);
        registerPanel.SetActive(false);
        mainMenuPanel.SetActive(false);
        gameplayPanel.SetActive(false);
        ClearLoginFields();
        ClearFeedback();
    }

    public void ShowRegisterPanel()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
        gameplayPanel.SetActive(false);
        ClearRegisterFields();
        ClearFeedback();
    }

    public void ShowMainMenuPanel()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        gameplayPanel.SetActive(false);
        ClearFeedback();
    }

    public void ShowGameplayPanel()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(false);
        mainMenuPanel.SetActive(false);
        gameplayPanel.SetActive(true);
        ClearFeedback();
    }
    #endregion

    #region Feedback
    public void ShowFeedback(string message)
    {
        if (mensagemFeedback != null)
        {
            // Remove caracteres especiais: só deixa letras, números, espaço e pontuação básica
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
    }

    private void ToggleLoginPasswordVisibility()
    {
        isLoginPasswordVisible = !isLoginPasswordVisible;
        SetInputFieldPasswordMode(loginPasswordInput, isLoginPasswordVisible);
        UpdateToggleIcon(loginTogglePasswordVisibilityButton, isLoginPasswordVisible);
    }

    private void OnLoginButtonClicked()
    {
        // Aqui validações iniciais
        if (string.IsNullOrEmpty(loginEmailInput.text) || string.IsNullOrEmpty(loginPasswordInput.text))
        {
            ShowFeedback("Preencha email e senha");
            if (loginErrorImage != null) loginErrorImage.gameObject.SetActive(true);
            return;
        }
        if (loginErrorImage != null) loginErrorImage.gameObject.SetActive(false);

        ClearFeedback();

        // TODO: Implementar login Firebase aqui
        // Exemplo: FirebaseLogin(loginEmailInput.text, loginPasswordInput.text);

        ShowFeedback("Tentando logar..."); // feedback temporário
    }

    private void OnForgotPasswordClicked()
    {
        ClearFeedback();
        // TODO: Implementar ação "Esqueci minha senha"
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
    }

    private void ToggleRegisterPasswordVisibility()
    {
        isRegisterPasswordVisible = !isRegisterPasswordVisible;
        SetInputFieldPasswordMode(registerPasswordInput, isRegisterPasswordVisible);
        UpdateToggleIcon(registerTogglePasswordVisibilityButton1, isRegisterPasswordVisible);
    }

    private void ToggleRegisterConfirmPasswordVisibility()
    {
        isRegisterConfirmPasswordVisible = !isRegisterConfirmPasswordVisible;
        SetInputFieldPasswordMode(registerConfirmPasswordInput, isRegisterConfirmPasswordVisible);
        UpdateToggleIcon(registerTogglePasswordVisibilityButton2, isRegisterConfirmPasswordVisible);
    }

    private void OnRegisterButtonClicked()
    {
        // Validações simples
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

        // TODO: Implementar registro Firebase aqui
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

    private void UpdateToggleIcon(Button toggleButton, bool visible)
    {
        if (toggleButton == null) return;

        var image = toggleButton.GetComponent<Image>();
        if (image == null) return;

        // Troque aqui pelos sprites que você tiver do olho aberto/fechado
        // Por enquanto só muda a cor como exemplo
        image.color = visible ? Color.green : Color.white;
    }

    #endregion
}
