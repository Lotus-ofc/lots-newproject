using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LotusLots.Managers; // GameManager e UIManager
using System.Collections;

namespace LotusLots.UI
{
    public class LoginPanel : MonoBehaviour
    {
        [Header("Painéis")]
        [SerializeField] private GameObject loginFormPanel;
        [SerializeField] private GameObject registerFormPanel;

        [Header("Login")]
        [SerializeField] private TMP_InputField loginEmailInputField;
        [SerializeField] private TMP_InputField loginPasswordInputField;
        [SerializeField] private Button loginButton;
        [SerializeField] private Button loginShowPasswordButton;
        [SerializeField] private Image loginShowPasswordIcon;

        [Header("Cadastro")]
        [SerializeField] private TMP_InputField registerEmailInputField;
        [SerializeField] private TMP_InputField registerPasswordInputField;
        [SerializeField] private TMP_InputField registerConfirmPasswordInputField;
        [SerializeField] private Button registerAccountButton;
        [SerializeField] private Button registerShowPasswordButton;
        [SerializeField] private Image registerShowPasswordIcon;
        [SerializeField] private Button confirmRegisterShowPasswordButton;
        [SerializeField] private Image confirmRegisterShowPasswordIcon;

        [Header("Navegação")]
        [SerializeField] private Button backToLoginButton; // Botão "Já tenho conta"
        [SerializeField] private Button goToRegisterButton; // Botão "Quero me registrar"
        [SerializeField] private Button loginFormBackToMainMenuButton;
        [SerializeField] private Button registerFormBackToMainMenuButton;

        [Header("Feedback")]
        [SerializeField] private TextMeshProUGUI feedbackText;

        [Header("Sprites")]
        [SerializeField] private Sprite eyeOpenSprite;
        [SerializeField] private Sprite eyeClosedSprite;

        private bool _isLoginPasswordVisible = false;
        private bool _isRegisterPasswordVisible = false;
        private bool _isConfirmRegisterPasswordVisible = false;

        private const string MOCK_EMAIL = "teste@teste.com";
        private const string MOCK_PASSWORD = "password123";

        private void OnEnable()
        {
            loginButton.onClick.AddListener(OnLoginButtonClicked);
            registerAccountButton.onClick.AddListener(OnRegisterButtonClicked);
            backToLoginButton.onClick.AddListener(ShowLoginForm);
            goToRegisterButton.onClick.AddListener(ShowRegisterForm);
            loginFormBackToMainMenuButton.onClick.AddListener(BackToMainMenu);
            registerFormBackToMainMenuButton.onClick.AddListener(BackToMainMenu);

            loginShowPasswordButton.onClick.AddListener(ToggleLoginPasswordVisibility);
            registerShowPasswordButton.onClick.AddListener(ToggleRegisterPasswordVisibility);
            confirmRegisterShowPasswordButton.onClick.AddListener(ToggleConfirmRegisterPasswordVisibility);

            // Começa na tela de login por padrão
            ShowLoginForm();
            UpdatePasswordVisibilityIcons();
        }

        private void OnDisable()
        {
            loginButton.onClick.RemoveAllListeners();
            registerAccountButton.onClick.RemoveAllListeners();
            backToLoginButton.onClick.RemoveAllListeners();
            goToRegisterButton.onClick.RemoveAllListeners();
            loginFormBackToMainMenuButton.onClick.RemoveAllListeners();
            registerFormBackToMainMenuButton.onClick.RemoveAllListeners();
            loginShowPasswordButton.onClick.RemoveAllListeners();
            registerShowPasswordButton.onClick.RemoveAllListeners();
            confirmRegisterShowPasswordButton.onClick.RemoveAllListeners();
        }

        private void ShowLoginForm()
        {
            loginFormPanel.SetActive(true);
            registerFormPanel.SetActive(false);
            ClearFeedbackMessage();

            _isLoginPasswordVisible = false;
            loginPasswordInputField.contentType = TMP_InputField.ContentType.Password;
            loginPasswordInputField.ForceLabelUpdate();

            UpdatePasswordVisibilityIcons();
        }

        private void ShowRegisterForm()
        {
            loginFormPanel.SetActive(false);
            registerFormPanel.SetActive(true);
            ClearFeedbackMessage();

            _isRegisterPasswordVisible = false;
            _isConfirmRegisterPasswordVisible = false;

            registerPasswordInputField.contentType = TMP_InputField.ContentType.Password;
            registerConfirmPasswordInputField.contentType = TMP_InputField.ContentType.Password;

            registerPasswordInputField.ForceLabelUpdate();
            registerConfirmPasswordInputField.ForceLabelUpdate();

            UpdatePasswordVisibilityIcons();
        }

        private void BackToMainMenu()
        {
            GameManager.Instance?.LoadScene("02_MainMenu");
        }

        private void OnLoginButtonClicked()
        {
            string email = loginEmailInputField.text;
            string password = loginPasswordInputField.text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                DisplayFeedback("Preencha e-mail e senha.", true);
                return;
            }

            if (email == MOCK_EMAIL && password == MOCK_PASSWORD)
            {
                StartCoroutine(SimulateLoginSuccess());
            }
            else
            {
                DisplayFeedback("Credenciais inválidas.", true);
            }
        }

        private IEnumerator SimulateLoginSuccess()
        {
            DisplayFeedback("Fazendo login...", false);
            yield return new WaitForSeconds(1.5f);
            DisplayFeedback("Login bem-sucedido!", false);
            GameManager.Instance?.HandleLoginSuccess();
        }

        private void OnRegisterButtonClicked()
        {
            string email = registerEmailInputField.text;
            string password = registerPasswordInputField.text;
            string confirm = registerConfirmPasswordInputField.text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirm))
            {
                DisplayFeedback("Preencha todos os campos.", true);
                return;
            }

            if (password != confirm)
            {
                DisplayFeedback("As senhas não coincidem!", true);
                return;
            }

            StartCoroutine(SimulateRegisterSuccess());
        }

        private IEnumerator SimulateRegisterSuccess()
        {
            DisplayFeedback("Registrando...", false);
            yield return new WaitForSeconds(1.5f);
            DisplayFeedback("Cadastro concluído! Faça login.", false);
            yield return new WaitForSeconds(2f);
            ShowLoginForm();
        }

        private void ToggleLoginPasswordVisibility()
        {
            _isLoginPasswordVisible = !_isLoginPasswordVisible;
            loginPasswordInputField.contentType = _isLoginPasswordVisible ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;
            loginPasswordInputField.ForceLabelUpdate();
            UpdatePasswordVisibilityIcons();
        }

        private void ToggleRegisterPasswordVisibility()
        {
            _isRegisterPasswordVisible = !_isRegisterPasswordVisible;
            registerPasswordInputField.contentType = _isRegisterPasswordVisible ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;
            registerPasswordInputField.ForceLabelUpdate();
            UpdatePasswordVisibilityIcons();
        }

        private void ToggleConfirmRegisterPasswordVisibility()
        {
            _isConfirmRegisterPasswordVisible = !_isConfirmRegisterPasswordVisible;
            registerConfirmPasswordInputField.contentType = _isConfirmRegisterPasswordVisible ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;
            registerConfirmPasswordInputField.ForceLabelUpdate();
            UpdatePasswordVisibilityIcons();
        }

        private void UpdatePasswordVisibilityIcons()
        {
            if (loginShowPasswordIcon != null)
                loginShowPasswordIcon.sprite = _isLoginPasswordVisible ? eyeOpenSprite : eyeClosedSprite;

            if (registerShowPasswordIcon != null)
                registerShowPasswordIcon.sprite = _isRegisterPasswordVisible ? eyeOpenSprite : eyeClosedSprite;

            if (confirmRegisterShowPasswordIcon != null)
                confirmRegisterShowPasswordIcon.sprite = _isConfirmRegisterPasswordVisible ? eyeOpenSprite : eyeClosedSprite;
        }

        private void DisplayFeedback(string message, bool isError)
        {
            if (feedbackText != null)
            {
                feedbackText.text = message;
                feedbackText.color = isError ? Color.red : Color.green;
                StopAllCoroutines(); // Evita conflito de mensagens
                StartCoroutine(ClearFeedbackAfterDelay(3f));
            }
        }

        private IEnumerator ClearFeedbackAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            ClearFeedbackMessage();
        }

        private void ClearFeedbackMessage()
        {
            if (feedbackText != null)
                feedbackText.text = "";
        }
    }
}
