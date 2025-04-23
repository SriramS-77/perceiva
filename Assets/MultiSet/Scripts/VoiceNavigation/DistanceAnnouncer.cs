using UnityEngine;

public class DistanceAnnouncer : MonoBehaviour
{
    [Tooltip("Seconds between each distance announcement")]
    public float announceInterval = 10f;

    float announceTimer = 0f;

    void Update()
    {
        // Only run when we’re actively navigating
        if (NavigationController.instance != null
            && NavigationController.instance.IsCurrentlyNavigating())
        {
            announceTimer += Time.deltaTime;
            if (announceTimer >= announceInterval)
            {
                // Grab destination and remaining distance
                var poi = NavigationController.instance.currentDestination;
                int dist = PathEstimationUtils.instance.getRemainingDistanceMeters();

                if (poi != null)
                {
                    // Speak it
                    TTSManager.Instance.Speak(
                      $"Reaching {poi.poiName} in {dist} metres",
                      true
                    );
                }

                announceTimer = 0f;
            }
        }
        else
        {
            // Reset timer whenever navigation stops
            announceTimer = 0f;
        }
    }
}
