using UnityEngine;
using System.Collections;

public class PlayerLocationProvider : MonoBehaviour
{
    public PlayerLocationUpdater player; // ATRIBUA NO INSPECTOR! (do GameObject Player)
    public PlayerCurrencyManager playerCurrencyManager; // ATRIBUA NO INSPECTOR! (do GameObject _CurrencyManager)

    IEnumerator Start()
    {
        #if UNITY_EDITOR
        Debug.Log("PlayerLocationProvider: Desativado no Editor. Use EditorLocationProvider para simulação.");
        enabled = false;
        yield break;
        #endif

        if (player == null)
        {
            Debug.LogError("PlayerLocationProvider: A referência 'player' (PlayerLocationUpdater) não está atribuída no Inspector. Desativando.");
            enabled = false;
            yield break;
        }
        if (playerCurrencyManager == null)
        {
            Debug.LogWarning("PlayerLocationProvider: A referência 'playerCurrencyManager' não está atribuída no Inspector.");
        }

        if (!Input.location.isEnabledByUser)
        {
            Debug.LogWarning("PlayerLocationProvider: Localização não está habilitada pelo usuário no dispositivo.");
            yield break;
        }

        Debug.Log("PlayerLocationProvider: Tentando iniciar serviço de localização...");
        Input.location.Start(1f, 1f);
        Input.compass.enabled = true;

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1f);
            maxWait--;
        }

        if (maxWait < 1)
        {
            Debug.LogError("PlayerLocationProvider: Tempo esgotado para inicializar o serviço de localização.");
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogError("PlayerLocationProvider: Falha ao obter localização. Verifique as permissões do aplicativo e as configurações de GPS.");
            yield break;
        }

        Debug.Log("PlayerLocationProvider: Serviço de localização inicializado com sucesso.");

        while (true)
        {
            if (Input.location.status == LocationServiceStatus.Running)
            {
                double lat = Input.location.lastData.latitude;
                double lon = Input.location.lastData.longitude;
                float heading = Input.compass.trueHeading;

                // Aqui você deve implementar a atualização do mapa usando MapLibre no futuro
                
                // Atualiza a localização do player no CurrencyManager
                if (playerCurrencyManager != null)
                {
                    playerCurrencyManager.UpdatePlayerLocation(lat, lon);
                }

                // Notifica o PlayerLocationUpdater sobre a nova localização e rotação
                player.SetLocation(lat, lon);
                player.SetRotation(heading);
            }
            else
            {
                Debug.LogWarning("PlayerLocationProvider: Serviço de localização não está rodando. Status: " + Input.location.status);
            }

            yield return new WaitForSeconds(1f);
        }
    }

    private void OnDisable()
    {
        if (Input.location.status == LocationServiceStatus.Running || Input.location.status == LocationServiceStatus.Initializing)
        {
            Input.location.Stop();
            Debug.Log("PlayerLocationProvider: Serviço de localização parado.");
        }
        if (Input.compass.enabled)
        {
            Input.compass.enabled = false;
            Debug.Log("PlayerLocationProvider: Bússola desativada.");
        }
    }
}
