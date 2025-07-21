using UnityEngine;
using UnityEngine.UI;

public class CompassUI : MonoBehaviour
{
    public RectTransform compassImage; // Arrasta a imagem da bússola aqui no Inspector
    public Transform player; // Arrasta o transform do player aqui no Inspector

    void Update()
    {
        if (compassImage == null || player == null) return;

        // Pega a rotação Y do player (0 a 360)
        float playerYaw = player.eulerAngles.y;

        // Rotaciona a bússola para "cancelar" a rotação do player
        compassImage.localRotation = Quaternion.Euler(0, 0, -playerYaw);
    }
}
