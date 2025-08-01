using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    public static EnvironmentManager Instance;

    [Header("Cores por estação")]
    public Color springColor = new Color(0.5f, 1f, 0.5f);
    public Color summerColor = new Color(1f, 0.9f, 0.4f);
    public Color autumnColor = new Color(1f, 0.6f, 0.2f);
    public Color winterColor = new Color(0.8f, 0.9f, 1f);

    [Header("Referência visual")]
    public Camera mainCamera;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Atualiza visual conforme a estação
    /// </summary>
    public void SetSeason(Season season)
    {
        Color targetColor = season switch
        {
            Season.Spring => springColor,
            Season.Summer => summerColor,
            Season.Autumn => autumnColor,
            Season.Winter => winterColor,
            _ => Color.white
        };

        if (mainCamera != null)
            mainCamera.backgroundColor = targetColor;

        Debug.Log($"EnvironmentManager: Mudou para estação {season}, cor aplicada.");
    }
}
