using UnityEngine;

public class PlayerLocationModeSelector : MonoBehaviour
{
    public EditorLocationProvider editorProviderScript; // Atribua o script aqui
    public PlayerLocationProvider playerProviderScript; // Atribua o script aqui

    void Awake()
    {
#if UNITY_EDITOR
        if (editorProviderScript != null) editorProviderScript.enabled = true;
        if (playerProviderScript != null) playerProviderScript.enabled = false;
#else
        if (editorProviderScript != null) editorProviderScript.enabled = false;
        if (playerProviderScript != null) playerProviderScript.enabled = true;
#endif
    }
}