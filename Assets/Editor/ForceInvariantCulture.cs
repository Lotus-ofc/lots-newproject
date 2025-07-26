using UnityEngine; // <--- Adicione esta linha
using UnityEditor;
using System.Globalization;
using System.Threading;

[InitializeOnLoad]
public class ForceInvariantCulture
{
    static ForceInvariantCulture()
    {
        // Define a cultura atual para a cultura invariante
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
        Debug.Log("Forçando CultureInfo.InvariantCulture no Editor.");
    }
}