using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Configuração das Abas")]
    public Transform plantsParent;      // Grid da aba Plantas
    public Transform toolsParent;       // Grid da aba Ferramentas
    public Transform utilitiesParent;   // Grid da aba Utilidades
    public GameObject slotPrefab;       // Prefab do slot
    public ItemDetailPanel detailPanel; // Painel de detalhes

    [Header("Itens do Jogador")]
    public List<ItemData> items = new List<ItemData>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void AddItem(ItemData item)
    {
        items.Add(item);
        RefreshUI();
    }

    public void RefreshUI()
    {
        // Limpa slots antigos de todas as abas
        ClearGrid(plantsParent);
        ClearGrid(toolsParent);
        ClearGrid(utilitiesParent);

        // Cria slots novos
        foreach (ItemData item in items)
        {
            Transform parent = null;

            switch (item.itemType)
            {
                case ItemType.Plant: parent = plantsParent; break;
                case ItemType.Tool: parent = toolsParent; break;
                case ItemType.Utility: parent = utilitiesParent; break;
            }

            if (parent != null)
            {
                GameObject slot = Instantiate(slotPrefab, parent);

                // Procura componente de slot
                InventorySlot slotComp = slot.GetComponent<InventorySlot>();
                if (slotComp != null)
                {
                    slotComp.Setup(item, () => detailPanel.ShowItem(item));
                }
                else
                {
                    // fallback se não tiver InventorySlot.cs
                    Transform iconTransform = slot.transform.Find("Icon");
                    if (iconTransform != null)
                    {
                        Image iconImage = iconTransform.GetComponent<Image>();
                        if (iconImage != null && item.icon != null)
                        {
                            iconImage.sprite = item.icon;
                            iconImage.enabled = true;
                        }
                    }
                }
            }
        }
    }

    private void ClearGrid(Transform parent)
    {
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }
    }
}
