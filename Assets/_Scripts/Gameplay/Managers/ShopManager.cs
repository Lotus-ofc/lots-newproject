using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;  // Singleton

    [Header("Referências")]
    public Transform shopContentParent;       // Content do ScrollView onde ficam os cards
    public GameObject shopItemCardPrefab;     // Prefab do card de item

    private List<SeasonItems> itemsBySeason;  // Lista de itens por estação, carregada do JSON
    private Season currentSeason;

    private void Awake()
    {
        // Singleton padrão
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Opcional, caso queira manter entre cenas
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadSeasonData();
    }

    /// <summary>
    /// Carrega SeasonData.json da pasta Resources/Json
    /// </summary>
    private void LoadSeasonData()
    {
        TextAsset jsonText = Resources.Load<TextAsset>("Json/SeasonData");
        if (jsonText != null)
        {
            SeasonData seasonData = JsonUtility.FromJson<SeasonData>(jsonText.text);
            itemsBySeason = seasonData.seasons;
        }
        else
        {
            Debug.LogError("SeasonData.json não encontrado em Resources/Json/");
            itemsBySeason = new List<SeasonItems>();
        }
    }

    /// <summary>
    /// Atualiza loja para a estação atual
    /// </summary>
    public void SetSeason(Season season)
    {
        currentSeason = season;

        // Busca itens pela estação (enum)
        SeasonItems seasonItems = itemsBySeason.Find(s => s.season == currentSeason);
        if (seasonItems != null)
        {
            RefreshShop(seasonItems.items);
        }
        else
        {
            Debug.LogWarning("Nenhum item encontrado para estação: " + currentSeason);
            RefreshShop(new List<ItemData>()); // Limpa a loja
        }
    }

    /// <summary>
    /// Atualiza interface da loja com os itens fornecidos
    /// </summary>
    private void RefreshShop(List<ItemData> itemsForSale)
    {
        // Remove cards antigos
        foreach (Transform child in shopContentParent)
            Destroy(child.gameObject);

        // Cria novos cards
        foreach (var item in itemsForSale)
        {
            GameObject card = Instantiate(shopItemCardPrefab, shopContentParent);

            // Carrega sprite automaticamente da pasta Resources/Icons usando itemName
            Sprite iconSprite = Resources.Load<Sprite>($"Icons/{item.itemName}");
            item.icon = iconSprite; // Atualiza referência no ItemData

            // Seta dados visuais no card
            card.transform.Find("Icon").GetComponent<Image>().sprite = iconSprite;
            card.transform.Find("NameText").GetComponent<TextMeshProUGUI>().text = item.itemName;
            card.transform.Find("PriceText").GetComponent<TextMeshProUGUI>().text = item.price.ToString();

            // Configura botão de compra
            Button buyButton = card.transform.Find("BuyButton").GetComponent<Button>();
            buyButton.onClick.AddListener(() => BuyItem(item));
        }
    }

    /// <summary>
    /// Compra item e adiciona ao inventário
    /// </summary>
    private void BuyItem(ItemData item)
    {
        // ⚠️ Se quiser, depois adicione verificação de saldo/moedas
        InventoryManager.Instance.AddItem(item);
        SceneFlowManager.Instance.ShowFeedback($"Comprou {item.itemName}!");
    }
}
