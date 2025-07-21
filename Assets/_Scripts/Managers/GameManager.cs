using UnityEngine;
using UnityEngine.SceneManagement; // Necessário para carregar cenas
using LotusLots.UI; // Necessário para acessar UIManager, se ele estiver em outro namespace

namespace LotusLots.Managers
{
    public class GameManager : MonoBehaviour
    {
        // Singleton Instance
        public static GameManager Instance { get; private set; }

        // Referência para o UIManager (será atribuída no Inspector ou via código)
        // Usamos [SerializeField] para que possamos atribuir no Inspector
        [SerializeField] private UIManager uiManager;

        // Propriedade para verificar se o UIManager está atribuído
        public UIManager UIManager => uiManager;

        // Variável para armazenar um argumento ao carregar uma cena
        private string _sceneLoadArgument = "";

        private void Awake()
        {
            // Implementação do Singleton
            if (Instance == null)
            {
                Instance = this;
                // Garante que o GameManager não seja destruído ao carregar novas cenas
                DontDestroyOnLoad(gameObject);
                Debug.Log("GameManager.Instance definido em Awake().");
            }
            else
            {
                // Se já existir uma instância, destrói esta para garantir apenas uma
                Destroy(gameObject);
                Debug.LogWarning("Tentativa de criar múltiplos GameManager. Destruindo duplicado.");
            }
        }

        private void Start()
        {
            // --- MUDANÇA AQUI: Carrega a cena de Gameplay diretamente ---
            LoadScene("03_Gameplay"); 
        }

        /// <summary>
        /// Carrega uma cena pelo seu nome, com um argumento opcional.
        /// </summary>
        /// <param name="sceneName">O nome da cena a ser carregada (ex: "01_Login").</param>
        /// <param name="argument">Um argumento opcional para a cena de destino (ex: "register").</param>
        public void LoadScene(string sceneName, string argument = "") // Argumento opcional adicionado
        {
            if (!string.IsNullOrEmpty(sceneName))
            {
                _sceneLoadArgument = argument; // Armazena o argumento para a cena de destino

                // Mostra uma tela de carregamento se o UIManager estiver disponível
                if (uiManager != null)
                {
                    uiManager.ShowLoadingScreen(true); 
                }
                else
                {
                    Debug.LogWarning("GameManager LoadScene: UIManager não está atribuído. Não é possível mostrar a tela de carregamento.");
                }
                
                SceneManager.LoadScene(sceneName);
                Debug.Log($"Cena '{sceneName}' carregada.");

                // A tela de carregamento será escondida no OnEnable do painel da cena de destino.
            }
            else
            {
                Debug.LogError("Nome da cena inválido para carregamento.");
            }
        }

        /// <summary>
        /// Chamado quando o login é bem-sucedido.
        /// </summary>
        public void HandleLoginSuccess()
        {
            Debug.Log("Login bem-sucedido! Carregando Main Menu...");
            LoadScene("02_MainMenu"); // Carrega a cena do Main Menu
        }

        /// <summary>
        /// Chamado quando o utilizador faz logout.
        /// </summary>
        public void HandleLogout()
        {
            Debug.Log("Logout realizado. Retornando à tela de Login...");
            LoadScene("01_Login"); // Retorna à cena de Login
        }

        /// <summary>
        /// Retorna o argumento passado na última carga de cena.
        /// </summary>
        public string GetSceneLoadArgument()
        {
            return _sceneLoadArgument;
        }

        /// <summary>
        /// Limpa o argumento de carga de cena. Deve ser chamado pela cena de destino após o uso.
        /// </summary>
        public void ClearSceneLoadArgument()
        {
            _sceneLoadArgument = "";
        }

        // Outros métodos de gerenciamento de jogo podem ser adicionados aqui
        // Ex: PauseGame(), ResumeGame(), QuitGame()
    }
}