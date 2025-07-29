using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Configuração")]
    public Transform gridParent;    // Referência ao Grid Layout do inventário
    public GameObject slotPrefab;   // Prefab do slot

    [Header("Itens")]
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
        // Limpa slots antigos
        foreach (Transform child in gridParent)
        {
            Destroy(child.gameObject);
        }

        // Cria slots novos
        foreach (ItemData item in items)
        {
            GameObject slot = Instantiate(slotPrefab, gridParent);
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
