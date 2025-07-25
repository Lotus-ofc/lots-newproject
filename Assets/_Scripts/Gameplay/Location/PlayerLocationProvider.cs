// PlayerLocationProvider.cs
using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using System.Collections; // Importante para IEnumerator

public class PlayerLocationProvider : MonoBehaviour
{
    public AbstractMap map; // ATRIBUA NO INSPECTOR! (do GameObject Map)
    public PlayerLocationUpdater player; // ATRIBUA NO INSPECTOR! (do GameObject Player)
    public PlayerCurrencyManager playerCurrencyManager; // ATRIBUA NO INSPECTOR! (do GameObject _CurrencyManager)

    // Removido: private bool hasNewLocation = false; // Não é mais usada, causando CS0414

    IEnumerator Start()
    {
        #if UNITY_EDITOR
        Debug.Log("PlayerLocationProvider: Desativado no Editor. Use EditorLocationProvider para simulação."); // CORREÇÃO: Log movido antes do yield break
        enabled = false;
        yield break; // Interrompe a execução do Start() para este script
        #endif

        // A PARTIR DAQUI, O CÓDIGO É EXCLUSIVO PARA CONSTRUÇÕES (mobile, etc.)

        if (map == null)
        {
            Debug.LogError("PlayerLocationProvider: A referência 'map' (AbstractMap) não está atribuída no Inspector. Desativando.");
            enabled = false;
            yield break;
        }
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

        // Verifica se o serviço de localização está habilitado pelo usuário
        if (!Input.location.isEnabledByUser)
        {
            Debug.LogWarning("PlayerLocationProvider: Localização não está habilitada pelo usuário no dispositivo.");
            yield break;
        }

        // Inicia o serviço de localização
        Debug.Log("PlayerLocationProvider: Tentando iniciar serviço de localização...");
        Input.location.Start(1f, 1f);
        Input.compass.enabled = true;

        // Espera pela inicialização do serviço de localização
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

        // Loop principal de atualização de localização
        while (true)
        {
            if (Input.location.status == LocationServiceStatus.Running)
            {
                double lat = Input.location.lastData.latitude;
                double lon = Input.location.lastData.longitude;
                float heading = Input.compass.trueHeading; // Obtém o heading da bússola

                // Atualiza o mapa na posição atual (o mapa segue o player)
                map.UpdateMap(new Vector2d(lat, lon));

                // Atualiza a localização do player no CurrencyManager
                if (playerCurrencyManager != null)
                {
                    playerCurrencyManager.UpdatePlayerLocation(lat, lon);
                }

                // Notifica o PlayerLocationUpdater sobre a nova localização e rotação
                player.SetLocation(lat, lon);
                player.SetRotation(heading); // Passa o heading da bússola para o PlayerLocationUpdater
            }
            else
            {
                Debug.LogWarning("PlayerLocationProvider: Serviço de localização não está rodando. Status: " + Input.location.status);
            }

            yield return new WaitForSeconds(1f);
        }
    }

    void Update()
    {
        // Este Update não precisa fazer nada, pois o Start() com sua coroutine já está fazendo a lógica principal.
        // O PlayerLocationUpdater já está lidando com a suavização.
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