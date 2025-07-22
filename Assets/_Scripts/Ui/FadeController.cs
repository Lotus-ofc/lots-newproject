using UnityEngine;

public class FadeController : MonoBehaviour
{
    [Header("Configurações")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 0.5f;

    void Awake()
    {
        DontDestroyOnLoad(gameObject); // Mantém entre cenas
        if (fadeCanvasGroup != null)
            fadeCanvasGroup.alpha = 0; // Garante que começa invisível
    }

    /// <summary>
    /// Faz o fade in (desaparecer) e desativa o objeto ao terminar.
    /// </summary>
    public void FadeIn()
    {
        if (fadeCanvasGroup == null) return;

        gameObject.SetActive(true);
        fadeCanvasGroup.alpha = 1;

        LeanTween.alphaCanvas(fadeCanvasGroup, 0, fadeDuration)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnComplete(() => gameObject.SetActive(false));
    }

    /// <summary>
    /// Faz o fade out (escurecer) e chama uma ação ao terminar.
    /// </summary>
    public void FadeOut(System.Action onComplete)
    {
        if (fadeCanvasGroup == null) return;

        gameObject.SetActive(true);
        fadeCanvasGroup.alpha = 0;

        LeanTween.alphaCanvas(fadeCanvasGroup, 1, fadeDuration)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnComplete(() => onComplete?.Invoke());
    }
}
