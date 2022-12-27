using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scan : MonoBehaviour
{
    #region Variables
        #region Drone Information
            // Speed of the drone
            public float speed = 20f;

            // Flag used to chose which side the drone will start to scan
            public bool oddDrone;

            // Flag indicating if the drone is alive
            // False would indicate to the other drone it has to do both jobs
            private bool isAlive = true;
        #endregion Drone Information

        #region Swarm Information
            // Flag indicating if the drone has reached the synchronization position to start scanning a runway
            private bool reachSyncPosition = false;
            // Reference to the other drone in the swarm
            public Scan otherDrone;
        #endregion Swarm Information

        #region Destinations
            // GameObject representing the station
            public GameObject stationObject;
            // Coordinates of the station
            private Vector3 station;
            
            // List of setpoint objects
            public List<GameObject> setpointsObjects = new List<GameObject>();
            
            // Current start and end setpoint coordinates
            private Vector3 start;
            private Vector3 end;

            // Coordinates of the current destination to go to
            private Vector3 destination = Vector3.zero;
        #endregion Destinations

        #region Others
            // Height used to detect if the ground has any anomalies, initialized when starting scanning
            private float scanningHeight = 0;

            // The current state of the drone
            [SerializeField] private DroneState currentState = DroneState.MovingToStart;
            // Flag indicating if an obstacle has been detected while scanning a strip
            [SerializeField] private bool obstacleDetected = false;

            // Enum representing the possible states of the drone
            private enum DroneState
            {
                MovingToStart,
                Scanning,
                MovingBackToStation,
                BackToStation,
                Finished,
            }
        #endregion Others
    #endregion Variables

    void Start()
    {
        station = stationObject.transform.position;
        InitStartAndEndPoints();
    }


    void Update()
    {
        // No more runways to check - do nothing
        if (setpointsObjects.Count == 0 && currentState == DroneState.BackToStation)
        {
            return;
        }

        // Scanning or moving to runway
        if (currentState <= DroneState.BackToStation)
        {
            UpdateDestination();
            
            // Waits the other drone to be ready if both are not ready to scan
            if (currentState == DroneState.Scanning && !otherDrone.ReachSyncPosition)
            {
                return;
            }

            // Waits the other drone to be ready if both are not ready to start a new runway
            if (currentState == DroneState.BackToStation && !otherDrone.ReachSyncPosition)
            {
                return;
            }
 
            MoveTowardDestination();

            // Scans the ground if needed
            if (currentState == DroneState.Scanning)
            {
                ScanGround();
            }
        }
        else
        {
            // Inititialize variables to start a new cycle with the next runway
            if(currentState == DroneState.BackToStation)
            {
                InitStartAndEndPoints();
                currentState = DroneState.MovingToStart;
                reachSyncPosition = false;
            }
        }
    }
    
    // Initialize the start and end points from the first set of points in the setpointsObjects list and remove them from the list.
    // If the list is empty before this method is called, set the current state to DroneState.Finished.
    void InitStartAndEndPoints()
    {
        if(setpointsObjects.Count != 0)
        {
            Transform[] children = setpointsObjects[0].GetComponentsInChildren<Transform>();
            if (oddDrone)
            {
                start = children[1].transform.position;
                end = children[2].transform.position;
            }
            else
            {
                end = children[1].transform.position;
                start = children[2].transform.position;
            }
            setpointsObjects.RemoveAt(0);
        }
        else
        {
            currentState = DroneState.Finished;            
        }
    }

    // Move the drone towards the destination, as long as no obstacle has been detected.
    // If the drone reaches the destination, increment the current state.
    void MoveTowardDestination()
    {
        if (!obstacleDetected)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, Time.deltaTime * speed);
            if (transform.position == destination)
            {
                currentState++;
            }
        }
    }

    // Handle collision within the swarm.
    // If so, increment the current state.    
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Drone") && currentState == DroneState.Scanning)
        {
            currentState++;
        }
    }

    // Update the destination and synchronization based on the current state of the drone.
    void UpdateDestination()
    {
        switch (currentState)
        {
            case DroneState.MovingToStart:
                destination = start;
                break;
            
            case DroneState.Scanning:
                destination = end;
                reachSyncPosition = true;
                break;

            case DroneState.MovingBackToStation:
                reachSyncPosition = true;
                destination = station;
                break;

        }
    }

    // Scan the ground for obstacles and send a them message if detected.
    void ScanGround()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit;

        float radius = 5f;
        float maxDistance = 5f;

        if (Physics.SphereCast(ray, radius, out hit, maxDistance))
        {
            // Initialize scanning height only once
            if (scanningHeight == 0f)
            {
                scanningHeight = hit.distance;
            }
            
            // Check if the ground is not plain
            float acceptedError = 0.1f;
            obstacleDetected = Mathf.Abs(hit.distance - scanningHeight) >= acceptedError;
            if (obstacleDetected)
            {
                hit.collider.gameObject.SendMessage("OnScan", this, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    public bool IsAlive => isAlive;

    public bool ReachSyncPosition => reachSyncPosition;
}
