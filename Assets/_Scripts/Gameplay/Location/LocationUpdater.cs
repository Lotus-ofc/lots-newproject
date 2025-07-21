using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Utils;

public class PlayerLocationUpdater : MonoBehaviour
{
    public AbstractMap map;
    public double latitude;
    public double longitude;

    void Update()
    {
        if (map == null) return;

        Vector2d latLon = new Vector2d(latitude, longitude);
        Vector3 position = map.GeoToWorldPosition(latLon, true);
        transform.position = position;
    }

    // Para atualizar posição via código
    public void SetLocation(double lat, double lon)
    {
        latitude = lat;
        longitude = lon;
    }
}

/*
 * Mantém o Player na posição correta no mapa,
 * de acordo com as coordenadas recebidas.
 * Funciona tanto no Editor quanto no dispositivo.
 */
