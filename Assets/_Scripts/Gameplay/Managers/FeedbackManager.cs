using UnityEngine;
using TMPro;
using System.Collections;

public class FeedbackManager : MonoBehaviour
{
    public static FeedbackManager Instance;

    [Header("Referência")]
    public TextMeshProUGUI feedbackText;

    [Header("Configuração")]
    public float displayTime = 2f;  // Tempo que a mensagem fica visível

    private Coroutine currentCoroutine;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // Garante que o texto começa vazio
        feedbackText.text = "";
    }

    public void ShowFeedback(string message)
    {
        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        currentCoroutine = StartCoroutine(ShowAndHide(message));
    }

    private IEnumerator ShowAndHide(string message)
    {
        feedbackText.text = message;
        feedbackText.gameObject.SetActive(true);

        yield return new WaitForSeconds(displayTime);

        feedbackText.text = "";
        feedbackText.gameObject.SetActive(false);
    }
}
