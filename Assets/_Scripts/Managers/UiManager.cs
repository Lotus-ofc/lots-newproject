using UnityEngine;
using System.Collections.Generic; // Necessário para List e Dictionary
using System; // Necessário para Action

namespace LotusLots.Managers
{
    // Enum para identificar os diferentes tipos de painéis de UI
    public enum UIPanelType
    {
        None,
        Login,
        MainMenu,
        GameplayHUD,
        AlertDialog,
        LoadingScreen
        // Adicione outros tipos de painéis conforme necessário
    }

    public class UIManager : MonoBehaviour
    {
        // Singleton Instance
        public static UIManager Instance { get; private set; }

        // Dicionário para armazenar referências a todos os painéis de UI
        // Key: Tipo do painel (UIPanelType), Value: GameObject do painel
        private Dictionary<UIPanelType, GameObject> uiPanels = new Dictionary<UIPanelType, GameObject>();

        // Referência para a tela de carregamento (se for um painel separado)
        [SerializeField] private GameObject loadingScreenPanel;

        private void Awake()
        {
            // Implementação do Singleton
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // Mantém o UIManager entre cenas
                Debug.Log("UIManager.Instance definido em Awake().");
                
                // Garante que o painel de carregamento (e seu Canvas pai) persista
                if (loadingScreenPanel != null)
                {
                    Debug.Log($"UIManager Awake: 'loadingScreenPanel' atribuído: {loadingScreenPanel.name}.");
                    // DontDestroyOnLoad só funciona para GameObjects raiz.
                    // Se o loadingScreenPanel não for raiz, pegamos o pai raiz para persistir.
                    Transform rootParent = loadingScreenPanel.transform.root;
                    if (rootParent != null && rootParent.gameObject != gameObject) // Evita marcar o próprio UIManager novamente
                    {
                        DontDestroyOnLoad(rootParent.gameObject);
                        Debug.Log($"UIManager Awake: Canvas raiz '{rootParent.name}' do loadingScreenPanel marcado para DontDestroyOnLoad.");
                    }
                }
                else
                {
                    Debug.LogWarning("UIManager Awake: 'loadingScreenPanel' NÃO atribuído no Inspector.");
                }
            }
            else
            {
                Destroy(gameObject);
                Debug.LogWarning("Tentativa de criar múltiplos UIManager. Destruindo duplicado.");
            }
        }

        private void Start()
        {
            // Opcional: Registar painéis que já existem na cena inicial (ex: Loading)
            // Ou pode registar dinamicamente quando as cenas são carregadas.
            // Por agora, vamos assumir que os painéis serão registados manualmente ou por scripts de painel.
            if (loadingScreenPanel != null)
            {
                RegisterPanel(UIPanelType.LoadingScreen, loadingScreenPanel);
                ShowLoadingScreen(false); // Garante que a tela de loading esteja inicialmente oculta
            }
        }

        /// <summary>
        /// Regista um painel de UI no UIManager.
        /// Deve ser chamado pelos scripts dos painéis quando eles são inicializados.
        /// </summary>
        /// <param name="type">O tipo do painel.</param>
        /// <param name="panelObject">O GameObject raiz do painel.</param>
        public void RegisterPanel(UIPanelType type, GameObject panelObject)
        {
            if (!uiPanels.ContainsKey(type))
            {
                uiPanels.Add(type, panelObject);
                Debug.Log($"Painel '{type}' registado.");
            }
            else
            {
                Debug.LogWarning($"Painel '{type}' já está registado.");
            }
        }

        /// <summary>
        /// Desregista um painel de UI do UIManager.
        /// </summary>
        /// <param name="type">O tipo do painel a ser desregistado.</param>
        public void DeregisterPanel(UIPanelType type)
        {
            if (uiPanels.ContainsKey(type))
            {
                uiPanels.Remove(type);
                Debug.Log($"Painel '{type}' desregistado.");
            }
        }

        /// <summary>
        /// Mostra um painel de UI específico e esconde todos os outros (opcional).
        /// </summary>
        /// <param name="typeToShow">O tipo do painel a ser mostrado.</param>
        /// <param name="hideOthers">Se deve esconder todos os outros painéis. Padrão é true.</param>
        public void ShowPanel(UIPanelType typeToShow, bool hideOthers = true)
        {
            if (hideOthers)
            {
                HideAllPanels();
            }

            if (uiPanels.TryGetValue(typeToShow, out GameObject panel))
            {
                panel.SetActive(true);
                Debug.Log($"Painel '{typeToShow}' mostrado.");
            }
            else
            {
                Debug.LogWarning($"Painel '{typeToShow}' não encontrado ou não registado.");
            }
        }

        /// <summary>
        /// Esconde um painel de UI específico.
        /// </summary>
        /// <param name="typeToHide">O tipo do painel a ser escondido.</param>
        public void HidePanel(UIPanelType typeToHide)
        {
            if (uiPanels.TryGetValue(typeToHide, out GameObject panel))
            {
                panel.SetActive(false);
                Debug.Log($"Painel '{typeToHide}' escondido.");
            }
        }

        /// <summary>
        /// Esconde todos os painéis de UI registados.
        /// </summary>
        public void HideAllPanels()
        {
            foreach (var panelEntry in uiPanels)
            {
                if (panelEntry.Value != null && panelEntry.Value.activeSelf)
                {
                    panelEntry.Value.SetActive(false);
                }
            }
            Debug.Log("Todos os painéis escondidos.");
        }

        /// <summary>
        /// Mostra ou esconde a tela de carregamento.
        /// </summary>
        /// <param name="show">True para mostrar, False para esconder.</param>
        public void ShowLoadingScreen(bool show)
        {
            Debug.Log($"ShowLoadingScreen chamado. 'loadingScreenPanel' é {(loadingScreenPanel != null ? "NÃO NULO" : "NULO")}.");

            if (loadingScreenPanel != null)
            {
                loadingScreenPanel.SetActive(show);
                Debug.Log($"Tela de carregamento {(show ? "mostrada" : "escondida")}.");
            }
            else
            {
                Debug.LogWarning("Tela de carregamento não atribuída no UIManager.");
                Debug.LogError("Variável 'loadingScreenPanel' é NULL no UIManager. Por favor, atribua-a no Inspector.");
            }
        }

        // Exemplo de como um AlertDialog pode ser implementado
        // Este método precisaria de um painel de AlertDialog pré-configurado
        public void ShowAlertDialog(string message, Action onConfirm = null, Action onCancel = null)
        {
            // Implementação futura: ativar um painel de AlertDialog prefab e configurá-lo
            // Por agora, apenas um log
            Debug.Log($"ALERT: {message}");
            // Exemplo de como chamar o callback se fosse um AlertDialog real:
            // if (onConfirm != null) onConfirm.Invoke();
        }

        // Outros métodos de gerenciamento de UI podem ser adicionados aqui
        // Ex: AnimatePanelIn(), AnimatePanelOut()
    }
}