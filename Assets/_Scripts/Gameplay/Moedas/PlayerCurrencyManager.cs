using UnityEngine;
using TMPro;
using Mapbox.Utils;

public class PlayerCurrencyManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text coinsText; // arraste seu texto da UI aqui no Inspector

    [Header("Recompensa por distância")]
    public double rewardDistanceMeters = 1000; // 1 km para ganhar moedas
    public int coinsPerReward = 10;

    private int coins = 0;

    private Vector2d lastPosition;
    private double distanceAccumulated = 0;

    void Start()
    {
        coins = 0;
        UpdateUI();

        // Inicialize a última posição com algo válido
        lastPosition = new Vector2d(0, 0);
    }

    // Chame esse método para atualizar a posição do player e verificar se ganhou moedas
    public void UpdatePlayerLocation(double latitude, double longitude)
    {
        Vector2d currentPosition = new Vector2d(latitude, longitude);

        // Ignora se for a primeira vez
        if (lastPosition.x == 0 && lastPosition.y == 0)
        {
            lastPosition = currentPosition;
            return;
        }

        double distance = Vector2d.Distance(lastPosition, currentPosition) * 111320; // aprox metros entre lat/lon

        distanceAccumulated += distance;

        if (distanceAccumulated >= rewardDistanceMeters)
        {
            int rewards = (int)(distanceAccumulated / rewardDistanceMeters);
            AddCoins(rewards * coinsPerReward);
            distanceAccumulated %= rewardDistanceMeters;
        }

        lastPosition = currentPosition;
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

    // Se precisar, reseta moedas
    public void ResetCoins()
    {
        coins = 0;
        UpdateUI();
    }
}
