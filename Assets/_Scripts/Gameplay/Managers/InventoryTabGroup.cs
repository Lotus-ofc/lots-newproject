using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryTabGroup : MonoBehaviour
{
    public List<GameObject> tabContents;
    public List<Button> tabButtons;
    private int currentTab = 0;

    private void Start()
    {
        ShowTab(0); // Abre a primeira aba por padrão
        for (int i = 0; i < tabButtons.Count; i++)
        {
            int index = i;
            tabButtons[i].onClick.AddListener(() => ShowTab(index));
        }
    }

    public void ShowTab(int index)
    {
        for (int i = 0; i < tabContents.Count; i++)
            tabContents[i].SetActive(i == index);

        currentTab = index;
    }
}
