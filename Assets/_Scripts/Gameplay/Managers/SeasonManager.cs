using UnityEngine;
using System.IO;

public class SeasonManager : MonoBehaviour
{
    public static SeasonManager Instance;

    [Header("Configuração")]
    public Season currentSeason = Season.Spring;   // Estação inicial padrão
    public int daysPerSeason = 15;                 // Dias até trocar de estação
    private int currentDay = 1;

    private string savePath;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        savePath = Path.Combine(Application.persistentDataPath, "seasonData.json");

        LoadSeasonData();
    }

    /// <summary>
    /// Chame esse método quando passar um dia no jogo
    /// </summary>
    public void NextDay()
    {
        currentDay++;

        if (currentDay > daysPerSeason)
        {
            currentDay = 1;
            CycleToNextSeason();
        }

        SaveSeasonData();
    }

    /// <summary>
    /// Troca automaticamente para a próxima estação
    /// </summary>
    private void CycleToNextSeason()
    {
        currentSeason = (Season)(((int)currentSeason + 1) % 4); // Loop: Spring→Summer→Autumn→Winter→Spring

        // Atualiza loja para nova estação
        ShopManager.Instance?.SetSeason(currentSeason);

        // Se quiser, atualiza plantas, clima etc. aqui
        Debug.Log("Nova estação: " + currentSeason);
    }

    /// <summary>
    /// Trocar estação manualmente (ex.: botão de debug ou opção do jogador)
    /// </summary>
    public void SetSeasonManually(Season newSeason)
    {
        currentSeason = newSeason;
        currentDay = 1;

        ShopManager.Instance?.SetSeason(currentSeason);

        SaveSeasonData();
    }

    /// <summary>
    /// Salva a estação e dia atuais em JSON local
    /// </summary>
    private void SaveSeasonData()
    {
        SeasonSaveData data = new SeasonSaveData
        {
            currentSeason = currentSeason,
            currentDay = currentDay
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
    }

    /// <summary>
    /// Carrega dados salvos ou usa padrão
    /// </summary>
    private void LoadSeasonData()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            SeasonSaveData data = JsonUtility.FromJson<SeasonSaveData>(json);

            currentSeason = data.currentSeason;
            currentDay = data.currentDay;
        }
        else
        {
            Debug.Log("Primeira vez jogando, usando estação padrão.");
            currentSeason = Season.Spring;
            currentDay = 1;
        }

        ShopManager.Instance?.SetSeason(currentSeason);
    }

    [System.Serializable]
    private class SeasonSaveData
    {
        public Season currentSeason;
        public int currentDay;
    }
}
