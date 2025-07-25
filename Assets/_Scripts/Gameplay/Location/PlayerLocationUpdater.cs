using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Utils;

public class PlayerLocationUpdater : MonoBehaviour
{
    public AbstractMap map;
    public double latitude;
    public double longitude;

    [Header("Suavização")]
    public float smoothTime = 0.3f;  // Quanto menor, mais rápido responde; maior, mais macio

    private Vector3 targetPosition;
    private Vector3 velocity = Vector3.zero;  // Interno para SmoothDamp

    void Start()
    {
        if (map != null) // Já tem a verificação, excelente!
        {
            Vector2d latLon = new Vector2d(latitude, longitude);
            targetPosition = map.GeoToWorldPosition(latLon, true);
            transform.position = targetPosition;
        }
        else
        {
            Debug.LogError("PlayerLocationUpdater: A referência 'map' (AbstractMap) não está atribuída no Inspector.");
            // Você pode desativar o script ou lidar com isso de outra forma
            // enabled = false; 
        }
    }

    void Update()
    {
        if (map == null) return;

        Vector2d latLon = new Vector2d(latitude, longitude);
        targetPosition = map.GeoToWorldPosition(latLon, true);

        // Move suavemente com inércia elegante
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }

    // Atualiza coordenadas do player
    public void SetLocation(double lat, double lon)
    {
        latitude = lat;
        longitude = lon;
    }
}
