using UnityEngine;
using UnityEngine.UI;

public class CCTVConnector : MonoBehaviour
{
    [Header("Connections")]
    [Tooltip("The camera that films the footage.")]
    public Camera surveillanceCamera;

    [Tooltip("The 3D Object (Plane/Quad) to show the footage on.")]
    public Renderer monitorScreen;

    [Tooltip("Alternatively, drag a UI RawImage here if using a Canvas.")]
    public RawImage rawImageScreen;

    [Header("Settings")]
    [Tooltip("Higher = Better Quality, Lower = Better Performance.")]
    public Vector2Int resolution = new Vector2Int(512, 512);

    [Tooltip("Frames per second for this camera (save performance by lowering this).")]
    [Range(1, 90)] public int refreshRate = 30;

    private RenderTexture _renderTexture;
    private float _timer;
    private float _refreshInterval;

    void Start()
    {
        if (surveillanceCamera == null)
        {
            Debug.LogError("CCTVConnector: No Camera assigned!");
            return;
        }

        // 1. Create a Run-Time Render Texture
        _renderTexture = new RenderTexture(resolution.x, resolution.y, 16);
        _renderTexture.name = $"CCTV_Feed_{gameObject.name}";

        // 2. Assign it to the Camera
        surveillanceCamera.targetTexture = _renderTexture;
        surveillanceCamera.enabled = false; // We will manually render it for performance

        // 3. Assign it to the Screen (Monitor)
        if (monitorScreen != null)
        {
            // Create a new material instance so we don't overwrite other screens
            monitorScreen.material.mainTexture = _renderTexture;
        }
        else if (rawImageScreen != null)
        {
            rawImageScreen.texture = _renderTexture;
        }

        _refreshInterval = 1.0f / refreshRate;
    }

    void Update()
    {
        // 4. Manually Render to save performance (e.g., 10fps security cam style)
        _timer += Time.deltaTime;
        if (_timer >= _refreshInterval)
        {
            surveillanceCamera.Render();
            _timer = 0f;
        }
    }

    void OnDestroy()
    {
        // Cleanup memory
        if (_renderTexture != null)
        {
            _renderTexture.Release();
        }
    }
}