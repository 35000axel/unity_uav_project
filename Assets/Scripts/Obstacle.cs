using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public void OnScan(Scan scanObject)
    {
        Debug.Log("Object scanned by: " + scanObject.name);
        StartCoroutine(DeleteObject());
    }

    IEnumerator DeleteObject()
    {
        yield return new WaitForSeconds(Random.Range(1f, 5f));
        Destroy(gameObject);
    }
}
