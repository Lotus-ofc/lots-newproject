using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemDetailPanel : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI description;
    public GameObject panel;

    public void ShowItem(ItemData item)
    {
        icon.sprite = item.icon;
        itemName.text = item.itemName;
        description.text = item.description;
        panel.SetActive(true);
    }

    public void Close()
    {
        panel.SetActive(false);
    }
}
