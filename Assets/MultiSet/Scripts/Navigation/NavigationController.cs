using UnityEngine;
using UnityEngine.AI;

/**
 * Handles the agent and other controllers to navigate a user in AR to a selected destination.
 */
public class NavigationController : MonoBehaviour
{
    public static NavigationController instance;

    // AR camera of the scene
    Camera ARCamera;

    // collider of the ARCamera to detect POI arrival
    SphereCollider ARCameraCollider;

    [Tooltip("NavMesh agent child of ARCamera")]
    public NavMeshAgent agent;

    [Tooltip("Current POI for navigation")]
    public POI currentDestination;

    [Tooltip("Space that contains POIs")]
    public AugmentedSpace augmentedSpace;
    
    // track whether we've already announced localization
    // bool wasLocalized = false;
    bool destinationReached = false;

    void Awake()
    {
        instance = this;
        ARCamera = Camera.main;
    }

    // Start is called before the first frame update
    void Start()
    {
        ARCameraCollider = ARCamera.GetComponent<SphereCollider>();
        if (currentDestination)
        {
            StartNavigation();
        }
    }

    public void StartLocalization()
    {
        // 1) Get the AR camera’s world position
        Vector3 camPos = ARCamera.transform.position;

        // 2) Try to find ("sample") the nearest point on the NavMesh within 1 m
        NavMeshHit hit;
        bool found = NavMesh.SamplePosition(
            camPos,       // source point in world space
            out hit,      // returns the nearest point on the NavMesh
            1.0f,         // max search radius (meters)
            NavMesh.AllAreas
        );

        if (found)
        {
            // 3a) Warp the agent to that exact NavMesh location
            agent.Warp(hit.position);

            // 4a) Accurate success feedback via TTS
            //    We only say “successful” if SamplePosition truly found a
            //    point on the NavMesh within that radius.
            TTSManager.Instance.Speak("Localization successful", false);
        }
        else
        {
            // 3b) If no NavMesh was found within 1 m, we consider it a failure
            TTSManager.Instance.Speak("Localization failed, please move to a mapped area", false);
        }
    }

        // Update is called once per frame
    void Update()
    {
        // Announcer
/*        bool isNowLocalized = agent.isOnNavMesh;
        if (isNowLocalized && !wasLocalized)
        {
            wasLocalized = true;
            TTSManager.Instance.Speak("Localization successful");
        }
        else if (!isNowLocalized && wasLocalized)
        { 
            wasLocalized = false;
            TTSManager.Instance.Speak("Localization failed");
        }*/

        if (agent.isOnNavMesh)
        {
            // stopped the NavMesh agent to walk to destination
            agent.isStopped = true;
        }

        if (IsCurrentlyNavigating() && agent.isOnNavMesh)
        {
            agent.destination = currentDestination.poiCollider.transform.position;

            // when we are navigating and we are localized path needs to go from current agent position
            ShowPath.instance.SetPositionFrom(agent.transform);

            // enable collider to detect arrival
            ARCameraCollider.enabled = true;
        }
        else
        {
            ARCameraCollider.enabled = false;
        }
    }

    // Sets a POI for navigation and gets ready for navigation.
    public void SetPOIForNavigation(POI aPOI)
    {
        currentDestination = aPOI;
        StartNavigation();
        // Announcer
        TTSManager.Instance.Speak(
            $"Starting navigation to {aPOI.poiName}",
            false
        );
        destinationReached = false;
    }

    // Sets positions for ShowPath to start navigation.
    void StartNavigation()
    {
        ShowPath.instance.SetPositionFrom(agent.transform);
        ShowPath.instance.SetPositionTo(currentDestination.poiCollider.transform);
    }

    // Stops navigation.
    public void StopNavigation()
    {
        // Announcer
        if (!destinationReached && currentDestination != null)
        {
            TTSManager.Instance.Speak(
                $"Navigation to {currentDestination.poiName} stopped",
                false
            );
        }

        if (currentDestination != null)
        {
            currentDestination = null;
            ShowPath.instance.ResetPath();
            PathEstimationUtils.instance.ResetEstimation();
        }
    }

    // Handles destination arrival. Is called from POI.Arrived()
    public void ArrivedAtDestination()
    {
        // Announcer
        TTSManager.Instance.Speak($"You have arrived at destination, {currentDestination.poiName}");
        destinationReached = true;
        
        StopNavigation();
        NavigationUIController.instance.ShowArrivedState();
    }

    //Returns true when user is currently navigating.
    public bool IsCurrentlyNavigating()
    {
        return currentDestination != null;
    }

    //Toggles the nav mesh agent capsule visibility
    public void ToggleAgentVisibility()
    {
        agent.gameObject.GetComponent<MeshRenderer>().enabled = !agent.gameObject.GetComponent<MeshRenderer>().enabled;
    }
}
