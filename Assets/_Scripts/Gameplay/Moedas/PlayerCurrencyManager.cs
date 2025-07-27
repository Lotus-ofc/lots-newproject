using UnityEngine;
using TMPro;

public class PlayerCurrencyManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text coinsText; // Arraste seu texto da UI aqui no Inspector

    [Header("Recompensa por distância")]
    public double rewardDistanceMeters = 1000; // 1 km para ganhar moedas
    public int coinsPerReward = 10;

    private int coins = 0;

    private double lastLatitude = 0;
    private double lastLongitude = 0;
    private double distanceAccumulated = 0;

    void Start()
    {
        coins = 0;
        UpdateUI();

        // Inicialize a última posição com valores inválidos para detectar primeira atualização
        lastLatitude = double.NaN;
        lastLongitude = double.NaN;
    }

    // Atualiza a posição do player e verifica se ganhou moedas
    public void UpdatePlayerLocation(double latitude, double longitude)
    {
        // Ignora se for a primeira vez (posição inválida)
        if (double.IsNaN(lastLatitude) || double.IsNaN(lastLongitude))
        {
            lastLatitude = latitude;
            lastLongitude = longitude;
            return;
        }

        // Calcula distância aproximada em metros usando fórmula simples para pequenos deslocamentos
        double distance = HaversineDistance(lastLatitude, lastLongitude, latitude, longitude);

        distanceAccumulated += distance;

        if (distanceAccumulated >= rewardDistanceMeters)
        {
            int rewards = (int)(distanceAccumulated / rewardDistanceMeters);
            AddCoins(rewards * coinsPerReward);
            distanceAccumulated %= rewardDistanceMeters;
        }

        lastLatitude = latitude;
        lastLongitude = longitude;
    }

    // Adiciona moedas e atualiza UI
    public void AddCoins(int amount)
    {
        coins += amount;
        UpdateUI();
    }

    // Atualiza o texto da UI
    private void UpdateUI()
    {
        if (coinsText != null)
            coinsText.text = coins.ToString();
    }

    // Reseta moedas
    public void ResetCoins()
    {
        coins = 0;
        UpdateUI();
    }

    // Função para calcular distância entre duas coordenadas geográficas em metros
    private double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000; // Raio da Terra em metros
        double latRad1 = lat1 * Mathf.Deg2Rad;
        double latRad2 = lat2 * Mathf.Deg2Rad;
        double deltaLat = (lat2 - lat1) * Mathf.Deg2Rad;
        double deltaLon = (lon2 - lon1) * Mathf.Deg2Rad;

        double a = Mathf.Sin((float)(deltaLat / 2)) * Mathf.Sin((float)(deltaLat / 2)) +
                   Mathf.Cos((float)latRad1) * Mathf.Cos((float)latRad2) *
                   Mathf.Sin((float)(deltaLon / 2)) * Mathf.Sin((float)(deltaLon / 2));

        double c = 2 * Mathf.Atan2(Mathf.Sqrt((float)a), Mathf.Sqrt((float)(1 - a)));

        double distance = R * c;
        return distance;
    }
}
