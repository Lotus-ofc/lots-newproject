using UnityEngine;
using UnityEngine.Android;

public class LocationPermissionManager : MonoBehaviour
{
    public delegate void PermissionResult(bool granted);
    public static event PermissionResult OnPermissionResult;

    private bool requested = false;

    void Start()
    {
        CheckAndRequestPermission();
    }

    public void CheckAndRequestPermission()
    {
        // Já tem permissão?
        if (Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Debug.Log("[LocationPermissionManager] Permissão FineLocation já concedida ✅");
            OnPermissionResult?.Invoke(true);
        }
        else
        {
            Debug.Log("[LocationPermissionManager] Solicitando permissão FineLocation...");
            Permission.RequestUserPermission(Permission.FineLocation);
            requested = true;
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && requested)
        {
            requested = false; // Evita checar toda hora
            bool granted = Permission.HasUserAuthorizedPermission(Permission.FineLocation);
            Debug.Log($"[LocationPermissionManager] Permissão FineLocation concedida? {granted}");
            OnPermissionResult?.Invoke(granted);
        }
    }

    public bool HasPermission()
    {
        return Permission.HasUserAuthorizedPermission(Permission.FineLocation);
    }
}
