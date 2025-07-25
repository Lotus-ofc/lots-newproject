// PlayerLocationUpdater.cs
using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Utils;

public class PlayerLocationUpdater : MonoBehaviour
{
    public AbstractMap map; // ATRIBUA NO INSPECTOR! (do GameObject Map)
    public double latitude; // Latitude atual do player
    public double longitude; // Longitude atual do player

    [Header("Suavização")]
    public float smoothTime = 0.3f; // Quanto menor, mais rápido responde; maior, mais macio

    private Vector3 targetPosition; // Posição alvo no mundo Unity
    private Vector3 velocity = Vector3.zero; // Usado internamente pelo SmoothDamp

    void Start()
    {
        if (map == null)
        {
            Debug.LogError("PlayerLocationUpdater: A referência 'map' (AbstractMap) não está atribuída no Inspector. Desativando.");
            enabled = false;
            return;
        }

        // Inicializa a posição do player na primeira localização conhecida
        Vector2d latLon = new Vector2d(latitude, longitude);
        targetPosition = map.GeoToWorldPosition(latLon, true);
        transform.position = targetPosition; // Define a posição inicial sem suavização
    }

    void Update()
    {
        if (map == null || !enabled) return; // Se o mapa ou este script estiverem desativados, não faz nada

        // Recalcula a posição alvo no mundo Unity com base nas últimas coordenadas
        Vector2d latLon = new Vector2d(latitude, longitude);
        targetPosition = map.GeoToWorldPosition(latLon, true);

        // Move suavemente o player para a posição alvo usando SmoothDamp
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }

    /// <summary>
    /// Atualiza as coordenadas do player.
    /// Chamado por PlayerLocationProvider (GPS real) ou EditorLocationProvider (simulação).
    /// </summary>
    /// <param name="lat">Nova latitude.</param>
    /// <param name="lon">Nova longitude.</param>
    public void SetLocation(double lat, double lon)
    {
        latitude = lat;
        longitude = lon;
        // A atualização da posição no mundo Unity e a suavização ocorrem no Update()
        // para garantir que a lógica de suavização seja executada a cada frame.
    }

    /// <summary>
    /// Define a rotação do player.
    /// Chamado por PlayerLocationProvider (bússola) ou EditorLocationProvider (direção simulada).
    /// </summary>
    /// <param name="heading">Novo heading (direção em graus).</param>
    public void SetRotation(float heading)
    {
        // Usando Lerp para uma rotação suave, semelhante ao SmoothDamp de posição
        Quaternion targetRotation = Quaternion.Euler(0, heading, 0);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5f); // Ajuste 5f para a velocidade de rotação
    }
}