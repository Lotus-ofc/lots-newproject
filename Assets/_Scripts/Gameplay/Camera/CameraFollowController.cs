using UnityEngine;

/// <summary>
/// Segue o player com Free Look (arrastar dedo ou mouse no editor)
/// e recentraliza ao apertar o botão.
/// Tem zoom (pinch) no mobile e scroll no editor.
/// </summary>
public class CameraFollowController : MonoBehaviour
{
    public Transform player;

    [Header("Offset e movimento")]
    public Vector3 offset = new Vector3(0, 10f, -10f);
    public float followSpeed = 5f;

    [Header("Rotação")]
    public float rotationLerpSpeed = 5f;
    private float currentYaw = 0f;
    private bool followRotation = true;

    [Header("Zoom")]
    public float currentZoom = 1f;
    public float zoomSpeed = 0.5f;
    public float minZoom = 0.5f;
    public float maxZoom = 2f;

    [Header("Free Look")]
    public float orbitSpeed = 100f;

    void Update()
    {
        if (player == null) return;

        // Touch arrasta → orbit livre
        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            followRotation = false;
            Touch touch = Input.GetTouch(0);
            currentYaw += touch.deltaPosition.x * orbitSpeed * Time.deltaTime;
        }

        // Pinch para zoom no mobile
        if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            float prevMag = (t0.position - t0.deltaPosition - (t1.position - t1.deltaPosition)).magnitude;
            float currentMag = (t0.position - t1.position).magnitude;
            float diff = currentMag - prevMag;

            currentZoom -= diff * zoomSpeed * Time.deltaTime;
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        // Scroll do mouse para zoom no editor/standalone
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            currentZoom -= scroll * zoomSpeed;
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
        }

        // Mouse arrasta → orbit livre
        if (Input.GetMouseButton(0))
        {
            followRotation = false;
            currentYaw += Input.GetAxis("Mouse X") * orbitSpeed * Time.deltaTime * 100f;
        }
#endif
    }

    void LateUpdate()
    {
        if (player == null) return;

        if (followRotation)
        {
            currentYaw = Mathf.LerpAngle(currentYaw, player.eulerAngles.y, rotationLerpSpeed * Time.deltaTime);
        }

        Vector3 desiredPosition = player.position + Quaternion.Euler(0, currentYaw, 0) * offset * currentZoom;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        Quaternion targetRotation = Quaternion.Euler(30f, currentYaw, 0);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationLerpSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Chama no botão para voltar a seguir o player.
    /// </summary>
    public void CenterCamera()
    {
        followRotation = true;
    }
}
