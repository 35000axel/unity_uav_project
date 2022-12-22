using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoTo : MonoBehaviour
{
    public GameObject prefab;
    GameObject spawnedPrefab;

    AirplaneController drone;
    bool findGoal = false;
    Vector3 goal;
    // Start is called before the first frame update
    void Start()
    {
        Vector3 spawnPosition = transform.position;
        spawnPosition.y += 2;
        Quaternion spawnRotation = Quaternion.Euler(0, 0, 0);
        spawnedPrefab = Instantiate(prefab, spawnPosition, spawnRotation);
        drone = spawnedPrefab.GetComponent<AirplaneController>();
        drone.userControl = false;
        goal = spawnPosition;
        goal.x += 3;
        goal.z += 3;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(!findGoal){
            findGoal = drone.goToPosition(goal);
             Debug.Log("goal = "+goal + " ,current position = "+spawnedPrefab.transform.position);
        }else{
            Debug.Log("goal = "+goal + " ,final position = "+spawnedPrefab.transform.position);
        }
    }
}
