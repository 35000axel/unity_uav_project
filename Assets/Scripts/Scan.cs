using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Scan : MonoBehaviour
{
    #region Variables
        #region Uav Properties
            // Speed of the uav
            public float speed = 20f;

            // Flag used to chose which side the uav will start to scan
            public bool oddUAV;

            // Flag indicating if the uav is alive
            // False would indicate to the other uav it has to do both jobs
            private bool isAlive = true;

            // Uav's Camera
            private Camera uavCamera;

        #endregion Uav Information

        #region Swarm Information
            // Flag indicating if the uav has reached the synchronization position to start scanning a runway
            [SerializeField] private bool reachSyncPosition = false;
            // Reference to the other uav in the swarm
            public Scan otherUav;
        #endregion Swarm Information

        #region Destinations
            // Landing coordonates on the station
            public Vector3 station;
            
            // Current start and end setpoint coordinates
            public Vector3 start;
            public Vector3 end;

            // Coordinates of the current destination to go to
            private Vector3 destination = Vector3.zero;
        #endregion Destinations

        #region Others
            // Height used to detect if the ground has any anomalies, initialized when starting scanning
            private float scanningHeight = 0f;

            // The current state of the uav
            [SerializeField] private UavState currentState = UavState.MovingToSetpoint;

            // Flag indicating if an obstacle has been detected while scanning a strip
            [SerializeField] private bool obstacleDetected = false;
            
            // Dictionary to store disformity images
            public Dictionary<string, Texture2D> disformityImages = new Dictionary<string, Texture2D>();

        #endregion Others
        
        // Enum representing the possible states of the uav
        private enum UavState
        {
            MovingToSetpoint,
            Scanning,
            MovingBackToStation,
            BackToStation,
            Finished,
        }
    #endregion Variables

    void Start()
    {
        uavCamera = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        switch (currentState)
        {
            case UavState.MovingToSetpoint:
                UpdateDestination();
                MoveTowardDestination();
                break;

            case UavState.Scanning:
                UpdateDestination();
                if (!otherUav.ReachSyncPosition) return;
                MoveTowardDestination();
                ScanGround();
                break;

            case UavState.MovingBackToStation:
                UpdateDestination();
                MoveTowardDestination();
                break;

            case UavState.BackToStation:
                break;

            case UavState.Finished:
                // InitStartAndEndPoints();
                // currentState = UavState.MovingToSetpoint;
                // reachSyncPosition = false;
                break;
        }
    }

    // Move the uav towards the destination, as long as no obstacle has been detected.
    // If the uav reaches the destination, increment the current state.
    void MoveTowardDestination()
    {
        if (!obstacleDetected)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, Time.deltaTime * speed);
        }
    }

    // Handle collision within the swarm.
    // If so, increment the current state.    
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Uav") && currentState == UavState.Scanning)
        {
            currentState++;
        }
    }

    // Update the destination and synchronization based on the current state of the uav.
    void UpdateDestination()
    {
        // Update current state if close enough to destination
        const float threshold = 0.1f;
        if (Vector3.Distance(transform.position, destination) < threshold)
        {
            currentState++;
        }

        switch (currentState)
        {
            case UavState.MovingToSetpoint:
                destination = start;
                break;
            
            case UavState.Scanning:
                destination = end;
                reachSyncPosition = true;
                break;

            case UavState.MovingBackToStation:
                // reachSyncPosition = true;
                destination = station;
                break;

        }
    }

    // Scan the ground for obstacles and send a them message if detected.
    void ScanGround()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit;

        float radius = 10f;
        float maxDistance = 15f;

        if (Physics.SphereCast(ray, radius, out hit, maxDistance))
        {
            // Initialize scanning height only once
            if (scanningHeight == 0f)
            {
                scanningHeight = hit.distance;
            }

            // Tolerance for ground height variation
            float acceptedError = 0.05f;

            float detectedObjectHeight = Mathf.Abs(hit.distance - scanningHeight);
            obstacleDetected = detectedObjectHeight >= acceptedError;

            // Only send the message and add the object to the dictionary if it is under a certain height
            float heightThreshold = 1f;

            if (obstacleDetected)
            {
                // if (detectedObjectHeight < heightThreshold || hit.collider.gameObject.layer == LayerMask.NameToLayer("Holes"))
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Holes"))
                {
                    if (!disformityImages.ContainsKey(hit.collider.gameObject.name))
                    {
                        Debug.Log("Add image");
                        // Take a picture of the ground
                        Texture2D texture = TakeScreenshot();

                        // Add the disformity to the dictionary
                        disformityImages.Add(hit.collider.gameObject.name, texture);
                    }
                    obstacleDetected = false;
                }
                else
                {
                    hit.collider.gameObject.SendMessage("OnScan", this, SendMessageOptions.DontRequireReceiver);
                }
            }
        }
        else
        {
            obstacleDetected = false;
        }
    }

    Texture2D TakeScreenshot()
    {
        // Set the uavCamera to be active
        uavCamera.gameObject.SetActive(true);
        // Create a RenderTexture and set it as the current target
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
        uavCamera.targetTexture = rt;
        // Render the current frame
        uavCamera.Render();
        // Copy the rendered image to a new Texture2D
        Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        RenderTexture.active = rt;
        texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        // Reset the render target and release the RenderTexture
        uavCamera.targetTexture = null;
        RenderTexture.active = null;
        rt.Release();
        // Return the captured image as a Texture2D
        return texture;
    }

    public bool IsAlive => isAlive;

    public bool ReachSyncPosition => reachSyncPosition;
}
