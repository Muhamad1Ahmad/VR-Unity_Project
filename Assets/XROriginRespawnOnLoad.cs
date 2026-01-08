using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.XR.CoreUtils;

public class XROriginRespawnOnLoad : MonoBehaviour
{
    [Header("Assign in Inspector")]
    [SerializeField] private XROrigin xrOrigin;          // drag XR Origin (XR Rig)
    [SerializeField] private Transform headSpawn;        // PlayerSpawnPosition
    [SerializeField] private bool matchYaw = true;

    [Header("Stability")]
    [SerializeField] private int settleFrames = 5;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        for (int i = 0; i < settleFrames; i++)
            yield return null;

        if (xrOrigin == null) xrOrigin = FindFirstObjectByType<XROrigin>();
        if (xrOrigin == null)
        {
            Debug.LogError("[Respawn] No XROrigin found.");
            yield break;
        }

        if (headSpawn == null)
        {
            Debug.LogError("[Respawn] headSpawn not assigned.");
            yield break;
        }

        if (xrOrigin.Camera == null)
        {
            Debug.LogError("[Respawn] XROrigin.Camera is null.");
            yield break;
        }

        // --- Deterministic head placement ---
        // Where is the camera relative to the XR Origin right now (local offset)?
        Transform cam = xrOrigin.Camera.transform;

        Vector3 camLocal = xrOrigin.transform.InverseTransformPoint(cam.position);

        // We want cam world position == headSpawn.position
        // So xrOrigin.position should be headSpawn.position - (xrOrigin.rotation * camLocal)
        Vector3 targetOriginPos = headSpawn.position - (xrOrigin.transform.rotation * camLocal);
        xrOrigin.transform.position = targetOriginPos;

        // --- Match yaw (optional) ---
        if (matchYaw)
        {
            float currentYaw = cam.eulerAngles.y;
            float targetYaw = headSpawn.eulerAngles.y;
            float yawDelta = targetYaw - currentYaw;

            xrOrigin.transform.Rotate(0f, yawDelta, 0f, Space.World);

            // After rotating, recalc origin position again so head stays exactly on spawn
            camLocal = xrOrigin.transform.InverseTransformPoint(cam.position);
            targetOriginPos = headSpawn.position - (xrOrigin.transform.rotation * camLocal);
            xrOrigin.transform.position = targetOriginPos;
        }

        Debug.Log("[Respawn] Deterministic respawn done.");
    }
}
