#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class MapboxInspectorFix
{
    private static bool alreadyFixed = false;  // controla se já aplicou o fix

    static MapboxInspectorFix()
    {
        EditorApplication.update += FixCoordinates;
    }

    static void FixCoordinates()
    {
        if (alreadyFixed) return;  // se já corrigiu, sai

        var map = Object.FindObjectOfType<Mapbox.Unity.Map.AbstractMap>();
        if (map != null && map.Options != null && map.Options.locationOptions != null)
        {
            var coord = map.Options.locationOptions.latitudeLongitude;
            if (!string.IsNullOrEmpty(coord) && coord.Contains(","))
            {
                var parts = coord.Split(',');
                if (parts.Length == 4)
                {
                    string fixedLat = parts[0] + "." + parts[1];
                    string fixedLon = parts[2] + "." + parts[3];
                    map.Options.locationOptions.latitudeLongitude = fixedLat + "," + fixedLon;
                    EditorUtility.SetDirty(map);
                    Debug.Log($"[MapboxInspectorFix] Corrigido automaticamente para: {fixedLat},{fixedLon}");
                    alreadyFixed = true;
                }
                else
                {
                    alreadyFixed = true;  // sem correção, não precisa rodar mais
                }
            }
            else
            {
                alreadyFixed = true; // sem vírgula incomum, pula fix
            }
        }
        else
        {
            alreadyFixed = true; // sem mapa ou opções, não precisa continuar
        }
    }
}
#endif
