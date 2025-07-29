using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SeasonData
{
    public List<SeasonItems> seasons;
}

[System.Serializable]
public class SeasonItems
{
    public Season season;           // Enum: Spring, Summer, Autumn, Winter
    public List<ItemData> items;    // Itens da estação
}

// Enum que representa as estações
public enum Season
{
    Spring,
    Summer,
    Autumn,
    Winter
}
