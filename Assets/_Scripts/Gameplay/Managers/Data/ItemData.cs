using UnityEngine;

[System.Serializable]
public class ItemData
{
    public string itemName;
    public ItemType itemType;   // enum: Seed, Sapling, Fruit, Tool etc.
    public Sprite icon;
    public int price;
    public int value;
}

public enum ItemType
{
    Seed,
    Sapling,
    Fruit,
    Tool
}
