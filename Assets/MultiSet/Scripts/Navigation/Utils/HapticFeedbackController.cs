using UnityEngine;

/// <summary>
/// Triggers a short vibration whenever the camera is pointed
/// within a small horizontal angle of the next path segment.
/// Attach this to your ARCamera.
/// </summary>
[RequireComponent(typeof(Camera))]
public class HapticFeedbackController : MonoBehaviour
{
    [Tooltip("Max horizontal angle (degrees) between camera forward and path direction to vibrate.")]
    public float angleThreshold = 10f;

    [Tooltip("Minimum seconds between consecutive vibrations.")]
    public float vibrationCooldown = 0.5f;

    private float _lastVibrateTime = -Mathf.Infinity;
    private ShowPath _showPath;

    void Awake()
    {
        // Try to grab ShowPath; it might not be ready yet
        _showPath = ShowPath.instance;
        if (_showPath == null)
            Debug.LogWarning("HapticFeedbackController: ShowPath.instance is null in Awake; retrying each frame.");
    }

    void LateUpdate()
    {
        // 1. Make sure NavigationController exists and navigation has started
        if (NavigationController.instance == null || !NavigationController.instance.IsCurrentlyNavigating())
            return;   // :contentReference[oaicite:0]{index=0}&#8203;:contentReference[oaicite:1]{index=1}

        // 2. Ensure we have a valid ShowPath reference
        if (_showPath == null)
        {
            _showPath = ShowPath.instance;
            if (_showPath == null)
                return;
        }

        // 3. Fetch the current corners array safely
        Vector3[] corners = _showPath.PathCorners;
        if (corners == null || corners.Length < 2)
            return;   // no valid segment yet :contentReference[oaicite:2]{index=2}&#8203;:contentReference[oaicite:3]{index=3}

        // 4. Strip out vertical tilt by projecting both vectors onto the XZ plane
        Vector3 forwardXZ = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 toNextXZ = Vector3.ProjectOnPlane(corners[1] - transform.position, Vector3.up).normalized;

        // 5. Measure horizontal alignment
        float angle = Vector3.Angle(forwardXZ, toNextXZ);

        // 6. Vibrate once when under threshold and after cooldown
        if (angle <= angleThreshold && Time.time - _lastVibrateTime >= vibrationCooldown)
        {
            Handheld.Vibrate();
            _lastVibrateTime = Time.time;
        }
    }
}
