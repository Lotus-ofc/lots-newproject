using UnityEngine;
using Mapbox.Unity.Map;
using System.Globalization;

[ExecuteAlways] // Funciona inclusive ao editar na cena
public class MapboxLatLonFixer : MonoBehaviour
{
    [SerializeField] private AbstractMap map;

    private void OnValidate()
    {
        FixLatLon();
    }

    private void Start()
    {
        FixLatLon();
    }

    private void FixLatLon()
    {
        if (map == null || map.Options == null || map.Options.locationOptions == null)
        {
            Debug.LogWarning("MapboxLatLonFixer: Referências não configuradas.");
            return;
        }

        string raw = map.Options.locationOptions.latitudeLongitude;

        if (string.IsNullOrWhiteSpace(raw))
        {
            Debug.LogWarning("MapboxLatLonFixer: Campo latitudeLongitude vazio.");
            return;
        }

        // Substitui vírgula decimal por ponto, mas mantém vírgula de separação entre lat e lon
        string fixedRaw = raw;

        // Tenta parsear como double usando locale
        string[] parts = raw.Split(',');
        if (parts.Length == 2)
        {
            double lat, lon;

            // Tenta parsear considerando que pode ter vindo como "-23,550520" em pt-BR
            if (double.TryParse(parts[0].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out lat) == false)
                double.TryParse(parts[0].Trim(), NumberStyles.Any, CultureInfo.CurrentCulture, out lat);

            if (double.TryParse(parts[1].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out lon) == false)
                double.TryParse(parts[1].Trim(), NumberStyles.Any, CultureInfo.CurrentCulture, out lon);

            // Gera string garantida no formato "-23.550520, -46.633308"
            fixedRaw = $"{lat.ToString(CultureInfo.InvariantCulture)}, {lon.ToString(CultureInfo.InvariantCulture)}";

            if (fixedRaw != raw)
            {
                Debug.Log($"MapboxLatLonFixer: Corrigido '{raw}' → '{fixedRaw}'");
                map.Options.locationOptions.latitudeLongitude = fixedRaw;
            }
        }
        else
        {
            Debug.LogWarning("MapboxLatLonFixer: Formato inesperado de latitudeLongitude: " + raw);
        }
    }
}
