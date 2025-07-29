using UnityEngine;

public class CameraFarmController : MonoBehaviour
{
    [Header("Configuração Inicial")]
    public Vector3 initialTargetPosition = new Vector3(-1.6f, 0f, -6f);
    public float initialZoom = 1f;

    [Header("Controle de Pan")]
    public float panSpeed = 0.5f;            // Sensibilidade do arrastar
    public Vector2 panLimitX = new Vector2(-20f, 20f);
    public Vector2 panLimitZ = new Vector2(-20f, 20f);

    [Header("Controle de Zoom")]
    public float zoomSpeed = 0.5f;
    public float minZoom = 0.8f;
    public float maxZoom = 2f;

    [Header("Ângulo Fixo da Câmera")]
    public float pitch = 50f;     // Inclinação pra baixo
    public float yaw = 30f;       // Rotação lateral (meio de lado)

    // Estado interno
    private Vector3 targetPosition;
    private float currentZoom;
    private Vector3 velocity;  // Para SmoothDamp

    void Start()
    {
        // Inicializa posição e zoom
        targetPosition = initialTargetPosition;
        currentZoom = Mathf.Clamp(initialZoom, minZoom, maxZoom);

        // Posiciona a câmera instantaneamente no ponto correto
        SnapCameraToTarget();
    }

    void Update()
    {
        HandlePanInput();
        HandleZoomInput();
    }

    void LateUpdate()
    {
        // Calcula a posição desejada da câmera com base no target + zoom + ângulo
        Vector3 offset = Quaternion.Euler(pitch, yaw, 0) * new Vector3(0, 0, -10f) * currentZoom;
        Vector3 desiredPosition = targetPosition + offset;

        // Move suavemente a câmera para a posição desejada
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, 0.15f);
        transform.rotation = Quaternion.Euler(pitch, yaw, 0);
    }

    private void HandlePanInput()
    {
        Vector3 panDelta = Vector3.zero;

#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButton(0))
        {
            float dx = -Input.GetAxis("Mouse X") * panSpeed;
            float dz = -Input.GetAxis("Mouse Y") * panSpeed;
            panDelta = new Vector3(dx, 0, dz);
        }
#endif

        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            Touch t = Input.GetTouch(0);
            float dx = -t.deltaPosition.x * panSpeed * Time.deltaTime;
            float dz = -t.deltaPosition.y * panSpeed * Time.deltaTime;
            panDelta = new Vector3(dx, 0, dz);
        }

        // Aplica pan no target
        targetPosition += panDelta;

        // Limita dentro dos limites configurados
        targetPosition.x = Mathf.Clamp(targetPosition.x, panLimitX.x, panLimitX.y);
        targetPosition.z = Mathf.Clamp(targetPosition.z, panLimitZ.x, panLimitZ.y);
    }

    private void HandleZoomInput()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            currentZoom -= scroll * zoomSpeed;
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
        }
#endif

        if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            float prevDist = (t0.position - t0.deltaPosition - (t1.position - t1.deltaPosition)).magnitude;
            float currDist = (t0.position - t1.position).magnitude;

            float delta = currDist - prevDist;
            currentZoom -= delta * zoomSpeed * Time.deltaTime * 0.1f;
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
        }
    }

    /// <summary>
    /// Posiciona a câmera no centro passado, ou no centro inicial se nada for passado.
    /// Use para resetar ou focar em pontos específicos.
    /// </summary>
    public void CenterCamera(Vector3? newCenter = null)
    {
        targetPosition = newCenter ?? initialTargetPosition;
        SnapCameraToTarget();
    }

    /// <summary>
    /// Posiciona a câmera instantaneamente no target com zoom e rotação atuais.
    /// </summary>
    private void SnapCameraToTarget()
    {
        Vector3 offset = Quaternion.Euler(pitch, yaw, 0) * new Vector3(0, 0, -5f) * currentZoom;
        transform.position = targetPosition + offset;
        transform.rotation = Quaternion.Euler(pitch, yaw, 0);
        velocity = Vector3.zero;  // Reseta a suavização para evitar delay na posição inicial
    }
}
