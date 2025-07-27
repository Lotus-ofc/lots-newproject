using UnityEngine;
using UnityEngine.Android;

/// <summary>
/// Gerencia a permissão de localização no Android.
/// Ideal para inicializar Mapbox ou lógica que depende de GPS.
/// </summary>
public class LocationPermissionManager : MonoBehaviour
{
    /// <summary>
    /// Chama assim que o objeto ativa.
    /// </summary>
    private void Start()
    {
        CheckAndRequestLocationPermission();
    }

    /// <summary>
    /// Verifica se já temos a permissão e, se não, solicita.
    /// </summary>
    public void CheckAndRequestLocationPermission()
    {
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Debug.Log("Permissão de localização NÃO concedida. Solicitando ao usuário...");
            Permission.RequestUserPermission(Permission.FineLocation);
        }
        else
        {
            Debug.Log("Permissão de localização já concedida!");
            OnLocationPermissionGranted();
        }
#else
        Debug.Log("Plataforma não Android - permissão não é necessária.");
        OnLocationPermissionGranted();
#endif
    }

    /// <summary>
    /// Chame essa função se quiser saber se o player aceitou depois.
    /// Exemplo: pode usar em um botão "Tentar novamente".
    /// </summary>
    public bool IsLocationPermissionGranted()
    {
#if UNITY_ANDROID
        return Permission.HasUserAuthorizedPermission(Permission.FineLocation);
#else
        return true; // Em outras plataformas, assume true
#endif
    }

    /// <summary>
    /// Chamada quando a permissão foi garantida (ou sempre em plataformas não-Android).
    /// Ideal pra inicializar Mapbox, serviços de localização, etc.
    /// </summary>
    private void OnLocationPermissionGranted()
    {
        Debug.Log("LocationPermissionManager: Permissão concedida, podemos inicializar localização e Mapbox.");
        // Exemplo:
        // MapboxManager.Instance?.Initialize();
    }
}
