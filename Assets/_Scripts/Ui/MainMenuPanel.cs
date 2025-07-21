using UnityEngine;
using UnityEngine.UI; // Necessário para Button
using TMPro; // Necessário para TextMeshProUGUI
using LotusLots.Managers; // Para acessar GameManager e UIManager

namespace LotusLots.UI
{
    public class MainMenuPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button loginButton; // Botão "Entrar"
        [SerializeField] private Button registerButton; // Botão "Cadastre-se"
        [SerializeField] private Button playButton; // Botão "Jogar" (se houver um no MainMenu)
        [SerializeField] private Button settingsButton; // Botão "Configurações" (se houver)
        [SerializeField] private Button quitButton; // Botão "Sair" (para PC)

        // Referência para o painel raiz do Main Menu
        [SerializeField] private GameObject mainMenuPanelGameObject;

        private void Awake()
        {
            // Garante que o painel raiz está atribuído
            if (mainMenuPanelGameObject == null)
            {
                mainMenuPanelGameObject = gameObject; // Se não atribuído, assume que o próprio script está no painel raiz
            }
        }

        private void OnEnable()
        {
            // Registra este painel no UIManager quando ele é ativado
            if (UIManager.Instance != null && GameManager.Instance != null)
            {
                UIManager.Instance.RegisterPanel(UIPanelType.MainMenu, mainMenuPanelGameObject);
                // Opcional: Esconder a tela de loading se ela ainda estiver visível
                UIManager.Instance.ShowLoadingScreen(false);
            }
            else
            {
                Debug.LogError("UIManager.Instance ou GameManager.Instance não encontrado. Verifique a ordem de execução dos scripts e se a cena 00_Loading está sendo carregada primeiro.");
            }

            // Adiciona listeners aos botões
            if (loginButton != null)
            {
                loginButton.onClick.AddListener(OnLoginButtonClicked);
            }
            if (registerButton != null)
            {
                registerButton.onClick.AddListener(OnRegisterButtonClicked);
            }
            if (playButton != null)
            {
                playButton.onClick.AddListener(OnPlayButtonClicked);
            }
            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(OnSettingsButtonClicked);
            }
            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitButtonClicked);
            }
        }

        private void OnDisable()
        {
            // Remove listeners dos botões para evitar memory leaks
            if (loginButton != null)
            {
                loginButton.onClick.RemoveListener(OnLoginButtonClicked);
            }
            if (registerButton != null)
            {
                registerButton.onClick.RemoveListener(OnRegisterButtonClicked);
            }
            if (playButton != null)
            {
                playButton.onClick.RemoveListener(OnPlayButtonClicked);
            }
            if (settingsButton != null)
            {
                settingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
            }
            if (quitButton != null)
            {
                quitButton.onClick.RemoveListener(OnQuitButtonClicked);
            }

            // Desregistra este painel do UIManager quando ele é desativado
            if (UIManager.Instance != null)
            {
                UIManager.Instance.DeregisterPanel(UIPanelType.MainMenu);
            }
        }

        /// <summary>
        /// Chamado quando o botão "Entrar" é clicado.
        /// </summary>
        private void OnLoginButtonClicked()
        {
            Debug.Log("Botão 'Entrar' clicado. Carregando cena de Login...");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadScene("01_Login"); // Carrega a cena de Login sem argumento (padrão é login)
            }
            else
            {
                Debug.LogError("GameManager.Instance é null ao tentar carregar a cena de Login.");
            }
        }

        /// <summary>
        /// Chamado quando o botão "Cadastre-se" é clicado.
        /// </summary>
        private void OnRegisterButtonClicked()
        {
            Debug.Log("Botão 'Cadastre-se' clicado. Carregando cena de Login para registro...");
            if (GameManager.Instance != null)
            {
                // --- MUDANÇA AQUI: Passa o argumento "register" ao carregar a cena de Login ---
                GameManager.Instance.LoadScene("01_Login", "register"); 
            }
            else
            {
                Debug.LogError("GameManager.Instance é null ao tentar carregar a cena de Login para registro.");
            }
        }

        /// <summary>
        /// Chamado quando o botão "Jogar" é clicado.
        /// </summary>
        private void OnPlayButtonClicked()
        {
            Debug.Log("Botão 'Jogar' clicado. Carregando cena de Gameplay...");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadScene("03_Gameplay"); // Carrega a cena de Gameplay
            }
            else
            {
                Debug.LogError("GameManager.Instance é null ao tentar carregar a cena de Gameplay.");
            }
        }

        /// <summary>
        /// Chamado quando o botão "Configurações" é clicado.
        /// </summary>
        private void OnSettingsButtonClicked()
        {
            Debug.Log("Botão 'Configurações' clicado. (Funcionalidade futura)");
            // UIManager.Instance.ShowPanel(UIPanelType.Settings); // Exemplo de como mostrar um painel de configurações
        }

        /// <summary>
        /// Chamado quando o botão "Sair" é clicado.
        /// </summary>
        private void OnQuitButtonClicked()
        {
            Debug.Log("Botão 'Sair' clicado. Fechando aplicação.");
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false; // Para sair do modo Play no Editor
            #else
                Application.Quit(); // Para sair da aplicação construída
            #endif
        }
    }
}