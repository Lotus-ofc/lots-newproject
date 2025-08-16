using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class InventorySlot : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI itemName;
    private ItemData currentItem;
    private Action onClickAction;

    public void Setup(ItemData item, Action onClick)
    {
        currentItem = item;
        icon.sprite = item.icon;
        itemName.text = item.itemName;
        onClickAction = onClick;

        GetComponent<Button>().onClick.AddListener(() => onClickAction?.Invoke());
    }
}
