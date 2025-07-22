using UnityEngine;
using UnityEngine.Android;

public class LocationPermissionManager : MonoBehaviour
{
    void Start()
    {
        CheckAndRequestLocationPermission();
    }

    public void CheckAndRequestLocationPermission()
    {
        // Verifica se já tem permissão
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            // Pede permissão
            Permission.RequestUserPermission(Permission.FineLocation);
        }
        else
        {
            Debug.Log("Permissão de localização já concedida!");
            // Você pode inicializar Mapbox ou sua lógica que depende da localização aqui
        }
    }
}
