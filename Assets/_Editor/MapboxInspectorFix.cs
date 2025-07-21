#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class MapboxInspectorFix
{
    static MapboxInspectorFix()
    {
        EditorApplication.update += FixCoordinates;
    }

    static void FixCoordinates()
    {
        var map = Object.FindObjectOfType<Mapbox.Unity.Map.AbstractMap>();
        if (map != null && map.Options != null && map.Options.locationOptions != null)
        {
            var coord = map.Options.locationOptions.latitudeLongitude;
            if (!string.IsNullOrEmpty(coord) && coord.Contains(","))
            {
                // Se contiver 4 partes (porque decimal usou vírgula), corrige
                var parts = coord.Split(',');
                if (parts.Length == 4)
                {
                    string fixedLat = parts[0] + "." + parts[1];
                    string fixedLon = parts[2] + "." + parts[3];
                    map.Options.locationOptions.latitudeLongitude = fixedLat + "," + fixedLon;
                    EditorUtility.SetDirty(map);
                    Debug.Log($"Corrigido automaticamente para: {fixedLat},{fixedLon}");
                }
            }
        }
    }
}
#endif
