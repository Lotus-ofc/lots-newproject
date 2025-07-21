using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Utils;

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

    public AbstractMap map;
    public PlayerLocationUpdater player;
    public PlayerCurrencyManager playerCurrencyManager;

    private Vector2d currentLocation;

    void Start()
    {
        currentLocation = new Vector2d(latitude, longitude);
    }

    void Update()
    {
        if (!simulateLocation || player == null) return;

        float distance = speed * Time.deltaTime;
        double deltaLat = distance * Mathf.Cos(directionDegrees * Mathf.Deg2Rad) / 111320f;
        double deltaLon = distance * Mathf.Sin(directionDegrees * Mathf.Deg2Rad) / (111320f * Mathf.Cos((float)currentLocation.x * Mathf.Deg2Rad));

        currentLocation.x += deltaLat;
        currentLocation.y += deltaLon;

        // Atualiza player (posição e rotação)
        player.SetLocation(currentLocation.x, currentLocation.y);
        player.transform.rotation = Quaternion.Euler(0, directionDegrees, 0);

        // Centraliza mapa na posição simulada
        map.UpdateMap(currentLocation);

        // Conta moedas
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
            }
        }
    }
#endif
}

/*
 * Funciona só no Editor.
 * Simula movimento baseado em direção e velocidade.
 * Atualiza player, mapa e moedas.
 */
