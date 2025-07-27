using UnityEngine;

public class PlayerLocationUpdater : MonoBehaviour
{
    public double latitude;  // Latitude atual do player
    public double longitude; // Longitude atual do player

    [Header("Suavização")]
    public float smoothTime = 0.3f; // Quanto menor, mais rápido responde; maior, mais macio

    private Vector3 targetPosition;  // Posição alvo no mundo Unity
    private Vector3 velocity = Vector3.zero;  // Usado internamente pelo SmoothDamp

    void Start()
    {
        // Inicializa a posição do player na primeira localização conhecida
        // A posição inicial é zero, deve ser atualizada externamente (ex: integração com MapLibre)
        targetPosition = transform.position;
    }

    void Update()
    {
        // Recalcula a posição alvo no mundo Unity com base nas últimas coordenadas
        // IMPORTANTE: Você deve atualizar targetPosition manualmente ao converter lat/lon para posição Unity

        // Move suavemente o player para a posição alvo usando SmoothDamp
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }

    /// <summary>
    /// Atualiza as coordenadas do player.
    /// Chamado por PlayerLocationProvider (GPS real) ou EditorLocationProvider (simulação).
    /// </summary>
    /// <param name="lat">Nova latitude.</param>
    /// <param name="lon">Nova longitude.</param>
    public void SetLocation(double lat, double lon)
    {
        latitude = lat;
        longitude = lon;
        // A conversão para posição Unity deve ser feita aqui ou externamente
        // Exemplo: targetPosition = SuaFuncaoParaConverterLatLonEmPosicaoUnity(latitude, longitude);
    }

    /// <summary>
    /// Atualiza diretamente a posição no mundo Unity (em metros ou unidade do jogo).
    /// Deve ser chamada depois da conversão da lat/lon para mundo.
    /// </summary>
    /// <param name="pos">Posição no mundo Unity.</param>
    public void SetWorldPosition(Vector3 pos)
    {
        targetPosition = pos;
    }

    /// <summary>
    /// Define a rotação do player.
    /// Chamado por PlayerLocationProvider (bússola) ou EditorLocationProvider (direção simulada).
    /// </summary>
    /// <param name="heading">Novo heading (direção em graus).</param>
    public void SetRotation(float heading)
    {
        // Usando Lerp para uma rotação suave, semelhante ao SmoothDamp de posição
        Quaternion targetRotation = Quaternion.Euler(0, heading, 0);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5f); // Ajuste 5f para a velocidade de rotação
    }
}
