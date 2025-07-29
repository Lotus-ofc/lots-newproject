using UnityEngine;
using System.Collections.Generic;

public class GardenManager : MonoBehaviour
{
    public GameObject slotPrefab; // Assign no Inspector
    public int gridRows = 4;
    public int gridColumns = 4;
    public float spacing = 1.2f;

    private List<GardenSlot> slots = new List<GardenSlot>();

    void Start()
    {
        CreateGrid();
    }

    void CreateGrid()
    {
        for (int row = 0; row < gridRows; row++)
        {
            for (int col = 0; col < gridColumns; col++)
            {
                Vector3 position = new Vector3(col * spacing, 0, row * spacing);
                GameObject slotObj = Instantiate(slotPrefab, position, Quaternion.identity);
                slots.Add(slotObj.GetComponent<GardenSlot>());
            }
        }
    }
}
