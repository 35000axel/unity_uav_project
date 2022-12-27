// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using System.IO;

// public class Scan : MonoBehaviour
// {
//     #region Drone Information
//         public float speed = 20f;
//         public bool oddDrone;  
//         private bool isAlive = true;
//         public Camera droneCamera;
//     #endregion Drone Information

//     #region Swarm Information
//         // Used to synchonize drone to wait each other
//         private bool reachSyncPosition = false;
//         public Scan otherDrone;
//     #endregion Swarm Information

//     #region Destinations
//         // Station Gameobject and coordonates
//         public GameObject stationObject;
//         private Vector3 station;
        
//         // List of setpoints objects
//         public List<GameObject> setpointsObjects = new List<GameObject>();
        
//         // current start and end setpoint
//         private Vector3 start;
//         private Vector3 end;

//         // Ending place Gameobject coordonates
//         private Vector3 destination = Vector3.zero;
//     #endregion Destinations

//     #region Others
//         // Height used to detected if the ground has any anomalies, initialized when starting scanning
//         private float scanningHeight = 0;

//         [SerializeField] private DroneState currentState = DroneState.MovingToStart;
//         [SerializeField] private bool obstacleDetected = false;
        
//         // A dictionary to store the captured images and their coordinates
//         private Dictionary<Vector2, Texture2D> images = new Dictionary<Vector2, Texture2D>();
//     #endregion Others

//     private enum DroneState
//     {
//         MovingToStart,
//         Scanning,
//         MovingBackToStation,
//         BackToStation,
//         Finished,
//     }

//     void Start()
//     {
//         station = stationObject.transform.position;
//         InitStartAndEndPoints();

//         // Adjust camera to look downward
//         droneCamera = GetComponent<Camera>();
//         droneCamera.transform.rotation = Quaternion.Euler(90, 0, 0);
//     }

//     void Update()
//     {
//         // No more strips to check
//         if (setpointsObjects.Count == 0 && currentState == DroneState.BackToStation)
//         {
//             return;
//         }

//         if (transform.position == station)
//         {
//             SaveImagesToPNG();
//             images.Clear();
//             return;
//         }

//         // Scanning or moving to Strips
//         if (currentState <= DroneState.BackToStation)
//         {
//             UpdateDestination();
            
//             // Waits the other drone to reach its starting point
//             if (currentState == DroneState.Scanning && !otherDrone.ReachSyncPosition)
//             {
//                 return;
//             }

//             // waits the other drone to start processing a new strip
//             if (currentState == DroneState.BackToStation && !otherDrone.ReachSyncPosition)
//             {
//                 return;
//             }

//             MoveTowardDestination();

//             if (currentState == DroneState.Scanning)
//             {
//                 ScanGround();
//             }
//         }
//         else
//         {
//             if(currentState == DroneState.BackToStation)
//             {
//                 InitStartAndEndPoints();
//                 currentState = DroneState.MovingToStart;
//                 reachSyncPosition = false;
//             }
//         }
//     }

//     void InitStartAndEndPoints()
//     {
//         if(setpointsObjects.Count != 0)
//         {
//             Transform[] children = setpointsObjects[0].GetComponentsInChildren<Transform>();
//             if (oddDrone)
//             {
//                 start = children[1].transform.position;
//                 end = children[2].transform.position;
//             }
//             else
//             {
//                 end = children[1].transform.position;
//                 start = children[2].transform.position;
//             }
//             setpointsObjects.RemoveAt(0);
//         }
//         else
//         {
//             currentState = DroneState.Finished;            
//         }
//     }

//     void MoveTowardDestination()
//     {
//         if (!obstacleDetected)
//         {
//             transform.position = Vector3.MoveTowards(transform.position, destination, Time.deltaTime * speed);
//             if (transform.position == destination)
//             {
//                 currentState++;
//             }
//         }
//     }

//     void OnTriggerEnter(Collider other)
//     {
//         Debug.Log("Collision");
//         // Collision range with another drone while scanning
//         if(other.gameObject.layer == 6 && currentState == DroneState.Scanning)
//         {
//             currentState++;
//         }
//     }


//     void UpdateDestination()
//     {
//         switch (currentState)
//         {
//             case DroneState.MovingToStart:
//                 destination = start;
//                 break;
            
//             case DroneState.Scanning:
//                 destination = end;
//                 reachSyncPosition = true;
//                 break;

//             case DroneState.MovingBackToStation:
//                 reachSyncPosition = true;
//                 destination = station;
//                 break;

//         }
//     }

//     void SaveImagesToPNG()
//     {
//         Debug.Log("Save PNG");
//         // Iterate through the dictionary and save each image to a file
//         foreach (var entry in images)
//         {
//             // Get the coordinates and image from the dictionary
//             Vector2 coordinates = entry.Key;
//             Texture2D image = entry.Value;

//             // Save the image to a file
//             byte[] bytes = image.EncodeToPNG();
//             File.WriteAllBytes("Screenshot_" + gameObject.name + "_" + coordinates.x + "_" + coordinates.y + ".png", bytes);
//         }
//     }

//     void CaptureImage(Vector2 position)
//     {
//         // Check if the coordinates already exist in the dictionary
//         if (images.ContainsKey(position))
//         {
//             // The coordinates already exist in the dictionary - do nothing
//             return;
//         }

//         // Create a render texture to render the camera's view to
//         RenderTexture renderTexture = new RenderTexture(droneCamera.pixelWidth, droneCamera.pixelHeight, 24);
//         droneCamera.targetTexture = renderTexture;

//         // Render the camera's view to the render texture
//         droneCamera.Render();

//         // Store the render texture as a Texture2D object
//         RenderTexture.active = renderTexture;
//         Texture2D screenshot = new Texture2D(renderTexture.width, renderTexture.height);
//         screenshot.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);

//         // Add the image and its coordinates to the dictionary
//         images.Add(position, screenshot);

//         // Clean up
//         RenderTexture.active = null;
//         droneCamera.targetTexture = null;
//         // RenderTexture.Release(renderTexture, true);
//         Destroy(renderTexture);
//     }


//     void ScanGround()
//     {
//         Ray ray = new Ray(transform.position, Vector3.down);
//         RaycastHit hit;

//         float radius = 5f;
//         float maxDistance = 5f;

//         if (Physics.SphereCast(ray, radius, out hit, maxDistance))
//         {
//             // Initialize scanning height once
//             if (scanningHeight == 0f)
//             {
//                 scanningHeight = hit.distance;
//             }
            
//             // Check if the ground is not plain
//             float acceptedError = 0.1f;
//             obstacleDetected = Mathf.Abs(hit.distance - scanningHeight) >= acceptedError;
//             if (obstacleDetected)
//             {
//                 hit.collider.gameObject.SendMessage("OnScan", this, SendMessageOptions.DontRequireReceiver);
//                 CaptureImage(transform.position);
//             }
//         }
//     }

//     public bool IsAlive => isAlive;

//     public bool ReachSyncPosition => reachSyncPosition;
// }
