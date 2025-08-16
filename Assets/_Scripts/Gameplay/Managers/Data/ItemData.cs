using UnityEngine;

[System.Serializable]
public class ItemData
{
    public string itemName;
    public ItemType itemType;   // enum define a aba do inventário
    public Sprite icon;
    public int price;
    public int value;
    public string description;  // Novo: usado no painel de detalhe
}

public enum ItemType
{
    Plant,
    Tool,
    Utility
}
