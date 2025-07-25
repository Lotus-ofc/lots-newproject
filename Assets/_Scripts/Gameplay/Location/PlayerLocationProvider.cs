using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using System.Collections;

public class PlayerLocationProvider : MonoBehaviour
{
    public AbstractMap map;
    public PlayerLocationUpdater player;
    public PlayerCurrencyManager playerCurrencyManager;

    private Vector3 targetPosition;  // posição final para o player se mover suavemente
    private bool hasNewLocation = false;

    IEnumerator Start()
    {
        if (!Input.location.isEnabledByUser)
        {
            Debug.LogWarning("Localização não está habilitada pelo usuário.");
            yield break;
        }

        Input.location.Start();
        Input.compass.enabled = true;

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1f);
            maxWait--;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogError("Não foi possível obter localização.");
            yield break;
        }

        while (true)
        {
            double lat = Input.location.lastData.latitude;
            double lon = Input.location.lastData.longitude;

            // Converte para posição no mundo Unity
            Vector2d latLon = new Vector2d(lat, lon);
            targetPosition = map.GeoToWorldPosition(latLon, true);
            hasNewLocation = true;

            // Atualiza mapa na posição atual
            map.UpdateMap(latLon);

            if (playerCurrencyManager != null)
            {
                playerCurrencyManager.UpdatePlayerLocation(lat, lon);
            }

            yield return new WaitForSeconds(1f);
        }
    }

    void Update()
    {
        if (hasNewLocation)
        {
            // Movimento suave até a nova posição
            player.transform.position = Vector3.Lerp(
                player.transform.position,
                targetPosition,
                Time.deltaTime * 5f
            );

            // Rotação suave para o heading da bússola
            float heading = Input.compass.trueHeading;
            Quaternion targetRotation = Quaternion.Euler(0, heading, 0);
            player.transform.rotation = Quaternion.Lerp(
                player.transform.rotation,
                targetRotation,
                Time.deltaTime * 5f
            );
        }
    }

    private void OnDisable()
    {
        if (Input.location.isEnabledByUser)
        {
            Input.location.Stop();
            Input.compass.enabled = false;
        }
    }
}
