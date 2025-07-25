// EditorLocationProvider.cs
using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using System;

#if UNITY_EDITOR // Esta diretiva garante que este código só seja compilado no Editor
using UnityEditor;
#endif

public class EditorLocationProvider : MonoBehaviour
{
    public bool simulateLocation = true;
    public double latitude = 37.7749;   // Posição inicial simulada
    public double longitude = -122.4194;
    public float speed = 5f; // Velocidade de movimento simulado em metros por segundo
    [Range(0, 360)] public float directionDegrees = 0f; // Direção de movimento simulado

    public AbstractMap map; // ATRIBUA NO INSPECTOR! (do GameObject Map)
    public PlayerLocationUpdater player; // ATRIBUA NO INSPECTOR! (do GameObject Player)
    public PlayerCurrencyManager playerCurrencyManager; // ATRIBUA NO INSPECTOR! (do GameObject _CurrencyManager)

    private Vector2d currentLocation;

    void Start()
    {
        // Se NÃO estiver no Unity Editor, desativa este script.
        // O PlayerLocationProvider será usado para o GPS real.
        #if !UNITY_EDITOR
        Debug.Log("EditorLocationProvider: Desativado em dispositivos mobile. Use PlayerLocationProvider para GPS.");
        enabled = false;
        return; // Interrompe a execução do Start() para este script
        #endif

        // A PARTIR DAQUI, O CÓDIGO É EXCLUSIVO PARA O UNITY EDITOR.

        if (map == null)
        {
            Debug.LogError("EditorLocationProvider: A referência 'map' (AbstractMap) não está atribuída no Inspector. Desativando.");
            enabled = false;
            return;
        }
        if (player == null)
        {
            Debug.LogWarning("EditorLocationProvider: A referência 'player' (PlayerLocationUpdater) não está atribuída no Inspector.");
        }
        if (playerCurrencyManager == null)
        {
            Debug.LogWarning("EditorLocationProvider: A referência 'playerCurrencyManager' não está atribuída no Inspector.");
        }

        // Inicializa a localização simulada
        currentLocation = new Vector2d(latitude, longitude);

        Debug.Log($"EditorLocationProvider: Tentando atualizar mapa no Start com Lat={currentLocation.x}, Lon={currentLocation.y}");

        try
        {
            // Tenta atualizar o mapa.
            map.UpdateMap(currentLocation);
        }
        catch (Exception ex)
        {
            Debug.LogError($"EditorLocationProvider: Erro ao chamar UpdateMap no AbstractMap durante o Start. " +
                           $"Verifique as configurações internas do AbstractMap no Inspector. Erro: {ex.Message}");
            enabled = false;
            return;
        }
        
        // Se a inicialização do mapa for bem-sucedida, inicializa o player
        if (player != null)
        {
            player.SetLocation(currentLocation.x, currentLocation.y);
            player.SetRotation(directionDegrees); // Usa o novo método SetRotation
        }
    }

    void Update()
    {
        // Garante que o script só funcione se a simulação estiver ativa e as referências essenciais não forem nulas
        if (!simulateLocation || map == null)
        {
            return;
        }

        // Simula o movimento do player
        if (simulateLocation)
        {
            // Converte velocidade de m/s para mudança de latitude/longitude
            // CORREÇÃO: Usar Mathf.Deg2Rad para Math (o erro CS0117)
            double latChange = Mathf.Cos(directionDegrees * Mathf.Deg2Rad) * speed * Time.deltaTime / 111320.0; // aprox. metros por grau lat
            double lonChange = Mathf.Sin(directionDegrees * Mathf.Deg2Rad) * speed * Time.deltaTime / (111320.0 * Mathf.Cos((float)currentLocation.x * Mathf.Deg2Rad)); // aprox. metros por grau lon

            currentLocation.x += latChange;
            currentLocation.y += lonChange;

            // Garante que a latitude e longitude permaneçam dentro dos limites válidos
            currentLocation.x = Mathf.Clamp((float)currentLocation.x, -90f, 90f);
            currentLocation.y = Mathf.Repeat((float)currentLocation.y + 180f, 360f) - 180f; // Ajusta longitude para -180 a 180
        }

        // Atualiza a posição do player e a rotação no PlayerLocationUpdater
        if (player != null)
        {
            player.SetLocation(currentLocation.x, currentLocation.y);
            player.SetRotation(directionDegrees); // Define a rotação do player baseada na direção simulada
        }

        // Atualiza o mapa na nova localização simulada
        map.UpdateMap(currentLocation);

        // Atualiza a localização do player no PlayerCurrencyManager
        if (playerCurrencyManager != null)
        {
            playerCurrencyManager.UpdatePlayerLocation(currentLocation.x, currentLocation.y);
        }
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(EditorLocationProvider))]
    public class EditorLocationProviderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorLocationProvider script = (EditorLocationProvider)target;
            if (GUILayout.Button("Resetar posição inicial"))
            {
                script.currentLocation = new Vector2d(script.latitude, script.longitude);
                if (script.map != null)
                {
                    script.map.UpdateMap(script.currentLocation);
                }
                if (script.player != null)
                {
                    script.player.SetLocation(script.currentLocation.x, script.currentLocation.y);
                    script.player.SetRotation(script.directionDegrees); // Usa o novo método SetRotation
                }
            }
        }
    }
    #endif
}