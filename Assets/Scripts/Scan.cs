using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scan : MonoBehaviour
{
    #region Drone Information
        public float speed = 20f;
    #endregion Drone Information

    #region Swarm Information
        public Scan otherDrone;

        // Used to synchonize drone to wait each other
        private bool reachSyncPosition = false;
    #endregion Swarm Information

    #region Destinations
        public GameObject stationObject;
        public GameObject startObject;
        public GameObject goalObject;

        private Vector3 station;
        private Vector3 start;
        private Vector3 goal;
        private Vector3 destination;
    #endregion Destinations

    #region Others
        private DroneState currentState = DroneState.MovingToStart;
        private List<Vector2> disformities = new List<Vector2>();
        private float scanningHeight = 0;
    #endregion Others

    private enum DroneState
    {
        MovingToStart,
        MovingToGoal,
        MovingBackToStation,
        Finished,
    }

    void Start()
    {
        station = stationObject.transform.position;
        start = startObject.transform.position;
        goal = goalObject.transform.position;
        destination = Vector3.zero;
    }

    void Update()
    {
        if (currentState != DroneState.Finished)
        {

            // Update the drone's next destination
            UpdateDestination();
            
            // Waits the other drone to reach its starting point
            if (currentState == DroneState.MovingToGoal && !otherDrone.ReachSyncPosition)
            {
                return;
            }

            MoveTowardDestination();

            if (currentState == DroneState.MovingToGoal)
            {
                ScanGround();
            }
        }
        else
        {
            Debug.Log($"{currentState} {gameObject.name} has detected {disformities.Count} disformities");
            // UnityEditor.EditorApplication.isPlaying = false;
        }
        
    }

    void MoveTowardDestination()
    {
        transform.position = Vector3.MoveTowards(transform.position, destination, Time.deltaTime * speed);
        if (transform.position == destination)
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
            
            case DroneState.MovingToGoal:
                destination = goal;
                reachSyncPosition = true;
                break;

            case DroneState.MovingBackToStation:
                reachSyncPosition = false;
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
            if (Mathf.Abs(hit.distance - scanningHeight) >= acceptedError)
            {
                // disformities.Add(new Vector2(hit.point.x, hit.point.y));
                if (!disformities.Contains(new Vector2((int)hit.point.x, (int)hit.point.y)))
                {
                    disformities.Add(new Vector2((int)hit.point.x, (int)hit.point.y));
                }
            }
        }
    }

    public bool ReachSyncPosition => reachSyncPosition;
}
