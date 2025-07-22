using UnityEngine;

public class FadeController : MonoBehaviour
{
    [Header("Fade Settings")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 0.5f;

    private void Awake()
    {
        if (fadeCanvasGroup == null)
        {
            Debug.LogError("FadeController: CanvasGroup não atribuído!");
        }

        // Começa invisível e inativo
        fadeCanvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    // Faz fade escurecendo e executa ação depois
    public void FadeOut(System.Action onComplete = null)
    {
        gameObject.SetActive(true);
        fadeCanvasGroup.alpha = 0f;
        LeanTween.alphaCanvas(fadeCanvasGroup, 1f, fadeDuration).setOnComplete(() =>
        {
            onComplete?.Invoke();
        });
    }

    // Faz fade clareando e desativa o objeto no fim
    public void FadeIn(System.Action onComplete = null)
    {
        gameObject.SetActive(true);
        fadeCanvasGroup.alpha = 1f;
        LeanTween.alphaCanvas(fadeCanvasGroup, 0f, fadeDuration).setOnComplete(() =>
        {
            gameObject.SetActive(false);
            onComplete?.Invoke();
        });
    }

    // Método útil pra fade out + in sequenciais (não vamos usar aqui)
    public void FadeOutIn(System.Action middleAction = null)
    {
        gameObject.SetActive(true);
        fadeCanvasGroup.alpha = 0f;
        LeanTween.alphaCanvas(fadeCanvasGroup, 1f, fadeDuration).setOnComplete(() =>
        {
            middleAction?.Invoke();
            LeanTween.alphaCanvas(fadeCanvasGroup, 0f, fadeDuration).setOnComplete(() =>
            {
                gameObject.SetActive(false);
            });
        });
    }
}
