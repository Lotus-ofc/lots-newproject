using UnityEngine;
using System.Globalization; // Necessário para CultureInfo e NumberStyles

public static class CoordinateConverter
{
    /// <summary>
    /// Converte uma string de coordenada que usa vírgula como separador decimal (ex: "45,1234")
    /// para uma string que usa ponto como separador decimal (ex: "45.1234").
    /// </summary>
    /// <param name="coordinateString">A string de coordenada com vírgula decimal.</param>
    /// <returns>A string de coordenada com ponto decimal.</returns>
    public static string ConvertCommaToDot(string coordinateString)
    {
        if (string.IsNullOrEmpty(coordinateString))
        {
            Debug.LogWarning("CoordinateConverter: A string de entrada para conversão é nula ou vazia.");
            return string.Empty;
        }

        // Simplesmente substitui todas as vírgulas por pontos.
        // Isso é eficaz para coordenadas numéricas simples.
        string convertedString = coordinateString.Replace(',', '.');
        return convertedString;
    }

    /// <summary>
    /// Tenta converter uma string de coordenada (que pode ter vírgula ou ponto como separador)
    /// para um valor double, garantindo que a cultura seja tratada corretamente.
    /// </summary>
    /// <param name="coordinateString">A string de coordenada.</param>
    /// <param name="result">O valor double resultante.</param>
    /// <returns>True se a conversão foi bem-sucedida, caso contrário, false.</returns>
    public static bool TryParseCoordinate(string coordinateString, out double result)
    {
        // Primeiro, tenta parsear usando a cultura invariante (que usa ponto).
        if (double.TryParse(coordinateString, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
        {
            return true;
        }

        // Se falhar, tenta parsear usando a cultura do Brasil (que usa vírgula).
        // Se a string contiver pontos mas a cultura brasileira for usada, pode haver um problema.
        // Por isso, tentamos InvariantCulture primeiro.
        if (double.TryParse(coordinateString, NumberStyles.Any, new CultureInfo("pt-BR"), out result))
        {
            return true;
        }

        // Última tentativa, substitui vírgula por ponto e tenta com cultura invariante.
        // Isso cobre casos onde a string é "12,34" e TryParse com pt-BR falhou por algum motivo (menos comum, mas possível).
        string cleanedString = coordinateString.Replace(',', '.');
        if (double.TryParse(cleanedString, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
        {
            return true;
        }

        Debug.LogWarning($"CoordinateConverter: Falha ao parsear a string '{coordinateString}' para double.");
        return false;
    }

    /// <summary>
    /// Converte um valor double de latitude ou longitude para uma string no formato
    /// com ponto decimal, ideal para APIs que esperam o padrão internacional.
    /// </summary>
    /// <param name="coordinateValue">O valor double da coordenada (latitude ou longitude).</param>
    /// <returns>A string da coordenada formatada com ponto decimal.</returns>
    public static string ConvertDoubleToDotString(double coordinateValue)
    {
        // Usa CultureInfo.InvariantCulture para garantir que o ponto seja usado como separador decimal.
        return coordinateValue.ToString(CultureInfo.InvariantCulture);
    }
}