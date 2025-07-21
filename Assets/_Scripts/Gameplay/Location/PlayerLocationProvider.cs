using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using System.Collections;

public class PlayerLocationProvider : MonoBehaviour
{
    public AbstractMap map;
    public PlayerLocationUpdater player;
    public PlayerCurrencyManager playerCurrencyManager;

    IEnumerator Start()
    {
        if (!Input.location.isEnabledByUser)
        {
            Debug.LogWarning("Localização não está habilitada pelo usuário.");
            yield break;
        }

        Input.location.Start();

        // Ativa bússola
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

            player.SetLocation(lat, lon);

            // Rotaciona o player para o heading da bússola (suaviza com Lerp)
            float heading = Input.compass.trueHeading;
            Quaternion targetRotation = Quaternion.Euler(0, heading, 0);
            player.transform.rotation = Quaternion.Lerp(player.transform.rotation, targetRotation, Time.deltaTime * 5f);

            map.UpdateMap(new Vector2d(lat, lon));

            if (playerCurrencyManager != null)
            {
                playerCurrencyManager.UpdatePlayerLocation(lat, lon);
            }

            yield return new WaitForSeconds(1f);
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
