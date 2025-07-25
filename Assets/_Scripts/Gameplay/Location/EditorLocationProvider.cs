using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class EditorLocationProvider : MonoBehaviour
{
    public bool simulateLocation = true;
    public double latitude = 37.7749;   // Posição inicial simulada
    public double longitude = -122.4194;
    public float speed = 5f;
    [Range(0, 360)] public float directionDegrees = 0f;

    public AbstractMap map; // Certifique-se de que este campo está preenchido no Inspector
    public PlayerLocationUpdater player; // Certifique-se de que este campo está preenchido no Inspector
    public PlayerCurrencyManager playerCurrencyManager; // Certifique-se de que este campo está preenchido no Inspector

    private Vector2d currentLocation;

    void Start()
    {
        // VERIFICAÇÃO CRÍTICA AQUI
        if (map == null)
        {
            Debug.LogError("EditorLocationProvider: A referência 'map' (AbstractMap) não está atribuída no Inspector. Desativando.");
            enabled = false; // Desativa este script para evitar mais NREs
            return;
        }

        currentLocation = new Vector2d(latitude, longitude);

        // Debug para ver o que está sendo passado
        Debug.Log($"EditorLocationProvider: Tentando atualizar mapa no Start com Lat={currentLocation.x}, Lon={currentLocation.y}");

        try
        {
            // Tenta atualizar o mapa. Se AbstractMap.UpdateMap ainda falhar, o problema é dentro do AbstractMap.
            map.UpdateMap(currentLocation);
        }
        catch (Exception ex)
        {
            Debug.LogError($"EditorLocationProvider: Erro ao chamar UpdateMap no AbstractMap durante o Start. " +
                           $"Verifique as configurações internas do AbstractMap no Inspector. Erro: {ex.Message}");
            enabled = false; // Desativa este script em caso de falha grave na inicialização
            return;
        }
        
        // Se a inicialização do mapa for bem-sucedida, inicializa o player
        if (player == null)
        {
            Debug.LogWarning("EditorLocationProvider: A referência 'player' (PlayerLocationUpdater) não está atribuída no Inspector.");
        }
        else
        {
            // Inicializa a posição do player assim que o mapa for atualizado.
            // Certifique-se de que PlayerLocationUpdater.SetLocation não chame map.GeoToWorldPosition no seu Start
            // ou que ele também lide com um mapa não inicializado.
            player.SetLocation(currentLocation.x, currentLocation.y);
            player.transform.rotation = Quaternion.Euler(0, directionDegrees, 0); // Define a rotação inicial
        }
    }

    void Update()
    {
        if (!simulateLocation || player == null || map == null) // Adicione map == null aqui para segurança
        {
             // Debug.Log("EditorLocationProvider: Update pulado. simulateLocation=" + simulateLocation + 
             //           ", player=" + (player != null) + ", map=" + (map != null));
             return;
        }

        // ... o restante do seu código Update ...

        // Atualiza player (posição e rotação)
        player.SetLocation(currentLocation.x, currentLocation.y);
        player.transform.rotation = Quaternion.Euler(0, directionDegrees, 0); // Isso sobrescreve a rotação da bússola no PlayerLocationProvider
        // ...
        map.UpdateMap(currentLocation); // Chamada principal do Update

        // ...
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
                // Opcional: Chamar UpdateMap aqui também se o mapa já estiver ativo
                // if (script.map != null)
                // {
                //     script.map.UpdateMap(script.currentLocation);
                // }
            }
        }
    }
    #endif
}