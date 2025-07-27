using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class EditorLocationProvider : MonoBehaviour
{
    public bool simulateLocation = true;
    public double latitude = 37.7749;   // Posição inicial simulada
    public double longitude = -122.4194;
    public float speed = 5f; // Velocidade em metros por segundo
    [Range(0, 360)] public float directionDegrees = 0f; // Direção de movimento simulado

    public PlayerLocationUpdater player; // Atribua no Inspector
    public PlayerCurrencyManager playerCurrencyManager; // Atribua no Inspector

    private double currentLatitude;
    private double currentLongitude;

    void Start()
    {
        #if !UNITY_EDITOR
        Debug.Log("EditorLocationProvider: Desativado em build. Use PlayerLocationProvider.");
        enabled = false;
        return;
        #endif

        currentLatitude = latitude;
        currentLongitude = longitude;

        if (player != null)
        {
            player.SetLocation(currentLatitude, currentLongitude);
            player.SetRotation(directionDegrees);
        }
    }

    void Update()
    {
        if (!simulateLocation)
            return;

        // Converte velocidade de m/s para variação de graus latitude/longitude (aproximado)
        double latChange = Mathf.Cos(directionDegrees * Mathf.Deg2Rad) * speed * Time.deltaTime / 111320.0;
        double lonChange = Mathf.Sin(directionDegrees * Mathf.Deg2Rad) * speed * Time.deltaTime / (111320.0 * Mathf.Cos((float)currentLatitude * Mathf.Deg2Rad));

        currentLatitude += latChange;
        currentLongitude += lonChange;

        currentLatitude = Mathf.Clamp((float)currentLatitude, -90f, 90f);
        currentLongitude = Mathf.Repeat((float)currentLongitude + 180f, 360f) - 180f;

        if (player != null)
        {
            player.SetLocation(currentLatitude, currentLongitude);
            player.SetRotation(directionDegrees);
        }

        if (playerCurrencyManager != null)
        {
            playerCurrencyManager.UpdatePlayerLocation(currentLatitude, currentLongitude);
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
                script.currentLatitude = script.latitude;
                script.currentLongitude = script.longitude;
                if (script.player != null)
                {
                    script.player.SetLocation(script.currentLatitude, script.currentLongitude);
                    script.player.SetRotation(script.directionDegrees);
                }
            }
        }
    }
    #endif
}
