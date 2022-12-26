using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scan : MonoBehaviour
{
    #region Drone Information
        public float speed = 20f;
        public bool oddDrone;  
        private bool isAlive = true;
    #endregion Drone Information

    #region Swarm Information
        // Used to synchonize drone to wait each other
        private bool reachSyncPosition = false;
        public Scan otherDrone;
    #endregion Swarm Information

    #region Destinations
        // Station Gameobject and coordonates
        public GameObject stationObject;
        private Vector3 station;
        
        // List of setpoints objects
        public List<GameObject> setpointsObjects = new List<GameObject>();
        
        // current start and end setpoint
        private Vector3 start;
        private Vector3 end;

        // Ending place Gameobject coordonates
        private Vector3 destination = Vector3.zero;
    #endregion Destinations

    #region Others
        // Height used to detected if the ground has any anomalies, initialized when starting scanning
        private float scanningHeight = 0;

        [SerializeField] private DroneState currentState = DroneState.MovingToStart;
        [SerializeField] private bool obstacleDetected = false;
    #endregion Others

    private enum DroneState
    {
        MovingToStart,
        Scanning,
        MovingBackToStation,
        BackToStation,
        Finished,
    }

    void Start()
    {
        station = stationObject.transform.position;
        InitStartAndEndPoints();
    }

    void Update()
    {
        // No more strips to check
        if (setpointsObjects.Count == 0 && currentState == DroneState.BackToStation)
        {
            return;
        }

        // Scanning or moving to Strips
        if (currentState <= DroneState.BackToStation)
        {
            UpdateDestination();
            
            // Waits the other drone to reach its starting point
            if (currentState == DroneState.Scanning && !otherDrone.ReachSyncPosition)
            {
                return;
            }

            // waits the other drone to start processing a new strip
            if (currentState == DroneState.BackToStation && !otherDrone.ReachSyncPosition)
            {
                return;
            }

            MoveTowardDestination();

            if (currentState == DroneState.Scanning)
            {
                ScanGround();
            }
        }
        else
        {
            if(currentState == DroneState.BackToStation)
            {
                InitStartAndEndPoints();
                currentState = DroneState.MovingToStart;
                reachSyncPosition = false;
            }
        }
    }

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

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collision");
        // Collision range with another drone while scanning
        if(other.gameObject.layer == 6 && currentState == DroneState.Scanning)
        {
            currentState++;
        }
    }


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

    void ScanGround()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit;

        float radius = 5f;
        float maxDistance = 5f;

        if (Physics.SphereCast(ray, radius, out hit, maxDistance))
        {
            // Initialize scanning height once
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
