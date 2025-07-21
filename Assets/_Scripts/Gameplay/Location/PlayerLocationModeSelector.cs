using UnityEngine;

public class PlayerLocationModeSelector : MonoBehaviour
{
    public GameObject editorLocationProvider;
    public GameObject playerLocationProvider;

    void Awake()
    {
#if UNITY_EDITOR
        if (editorLocationProvider != null) editorLocationProvider.SetActive(true);
        if (playerLocationProvider != null) playerLocationProvider.SetActive(false);
#else
        if (editorLocationProvider != null) editorLocationProvider.SetActive(false);
        if (playerLocationProvider != null) playerLocationProvider.SetActive(true);
#endif
    }
}

/* Trocar de Script "EditorLocationProvider" e "PlayerLocationProvider" automaticamente.
para que não o boneco não começe a andar sozinho nos dispositivos. */