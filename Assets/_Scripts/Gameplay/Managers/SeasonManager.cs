using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SeasonManager : MonoBehaviour
{
    public static SeasonManager Instance;

    [Header("Configuração")]
    public Season currentSeason = Season.Spring;
    public int daysPerSeason = 15; // Dias reais para poder trocar

    private DateTime lastChangeDate;
    private string savePath;

    private List<SeasonItems> seasonItemsList; // Lista carregada do JSON

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
        savePath = Path.Combine(Application.persistentDataPath, "seasonData.json");

        LoadSeasonData();
        LoadSeasonItems();
    }

    private void LoadSeasonItems()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Json/SeasonData");
        if (jsonFile != null)
        {
            SeasonData data = JsonUtility.FromJson<SeasonData>(jsonFile.text);
            seasonItemsList = data.seasons;
            Debug.Log("Itens da estação carregados com sucesso.");
        }
        else
        {
            Debug.LogError("Arquivo JSON não encontrado em Resources/Json/SeasonData.json");
            seasonItemsList = new List<SeasonItems>();
        }
    }

    public void TryChangeSeason(Season newSeason)
{
    DateTime now = DateTime.UtcNow;
    TimeSpan diff = now - lastChangeDate;

    if (diff.TotalDays >= daysPerSeason)
    {
        currentSeason = newSeason;
        lastChangeDate = now;
        SaveSeasonData();

        ShopManager.Instance?.SetSeason(currentSeason);

        string feedbackText = newSeason switch
        {
            Season.Spring => "🌸 Olá Primavera",
            Season.Summer => "☀️ Olá Verão",
            Season.Autumn => "🍂 Olá Outono",
            Season.Winter => "❄️ Olá Inverno",
            _ => $"Olá {newSeason}"
        };

        Debug.Log(feedbackText);
        SceneFlowManager.Instance?.ShowFeedback(feedbackText);
    }
    else
    {
        int daysLeft = Mathf.CeilToInt((float)(daysPerSeason - diff.TotalDays));
        string feedbackText = $"Faltam {daysLeft} dia(s) para trocar de estação.";
        Debug.Log(feedbackText);
        SceneFlowManager.Instance?.ShowFeedback(feedbackText);
    }
}


    private void ShowItemsForSeason(Season season)
    {
        if (seasonItemsList == null) return;

        var seasonData = seasonItemsList.Find(s => s.season == season);
        if (seasonData != null)
        {
            Debug.Log($"Itens da estação {season}:");
            foreach (var item in seasonData.items)
            {
                Debug.Log($"- {item.itemName} (Tipo {item.itemType}) Preço: {item.price} Valor: {item.value}");
            }
        }
        else
        {
            Debug.LogWarning($"Nenhum dado de itens encontrado para a estação {season}");
        }
    }

    private void SaveSeasonData()
    {
        var data = new SeasonSaveData
        {
            currentSeason = currentSeason,
            lastChangeDateStr = lastChangeDate.ToString("o")
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
    }

    private void LoadSeasonData()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            var data = JsonUtility.FromJson<SeasonSaveData>(json);
            currentSeason = data.currentSeason;
            lastChangeDate = DateTime.Parse(data.lastChangeDateStr, null, System.Globalization.DateTimeStyles.RoundtripKind);
            Debug.Log($"Dados carregados: estação {currentSeason}, última troca {lastChangeDate}");
        }
        else
        {
            Debug.Log("Primeira vez jogando. Usando estação padrão e data atual.");
            currentSeason = Season.Spring;
            lastChangeDate = DateTime.UtcNow;
            SaveSeasonData();
        }

        ShopManager.Instance?.SetSeason(currentSeason);
        ShowItemsForSeason(currentSeason);
    }

    [Serializable]
    private class SeasonSaveData
    {
        public Season currentSeason;
        public string lastChangeDateStr;
    }
}
