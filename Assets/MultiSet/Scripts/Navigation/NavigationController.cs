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

    // Update is called once per frame
    void Update()
    {
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
