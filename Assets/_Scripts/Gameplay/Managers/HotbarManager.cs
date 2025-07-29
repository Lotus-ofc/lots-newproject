using UnityEngine;
using UnityEngine.UI;

public class HotbarManager : MonoBehaviour
{
    public Image[] slotImages;  // Conecte aqui os 8 slots
    public int activeSlotIndex = 0;

    private void Start()
    {
        UpdateHighlight();
    }

    private void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            activeSlotIndex = (activeSlotIndex + 1) % slotImages.Length;
            UpdateHighlight();
        }
        else if (scroll < 0f)
        {
            activeSlotIndex = (activeSlotIndex - 1 + slotImages.Length) % slotImages.Length;
            UpdateHighlight();
        }
    }

    public void SetActiveSlot(int index)
    {
        activeSlotIndex = index;
        UpdateHighlight();
    }

    private void UpdateHighlight()
    {
        for (int i = 0; i < slotImages.Length; i++)
        {
            slotImages[i].color = (i == activeSlotIndex) ? Color.yellow : Color.white;
        }
    }
}
