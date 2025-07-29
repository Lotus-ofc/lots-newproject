using UnityEngine;

public class GardenSlot : MonoBehaviour
{
    public string plantType = "";
    public int growthStage = 0;
    public System.DateTime plantingDate;

    public void PlantSeed(string newPlantType)
    {
        plantType = newPlantType;
        growthStage = 0;
        plantingDate = System.DateTime.Now;
        Debug.Log($"Planted {newPlantType}!");
    }

    public void Harvest()
    {
        Debug.Log($"Harvested {plantType}!");
        plantType = "";
        growthStage = 0;
    }

    private void OnMouseDown()
    {
        // Exemplo: se tiver planta, colhe. Se não, planta "DefaultPlant"
        if (string.IsNullOrEmpty(plantType))
            PlantSeed("DefaultPlant");
        else
            Harvest();
    }
}
