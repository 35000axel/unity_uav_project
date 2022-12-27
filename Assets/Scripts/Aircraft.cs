using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Aircraft : MonoBehaviour
{


private bool start = false;
public float speed = 100f;
float elapsedTime = 0;

void Update()
{
    if(start)
    {
        elapsedTime += Time.deltaTime;

        if (elapsedTime < 1.2)
        {
            // move forward only for the first 2 seconds
            transform.Translate(Vector3.forward * Time.deltaTime * speed);
        }
        else
        {
            // move both forward and up after 2 seconds
            transform.Translate(Vector3.forward * Time.deltaTime * speed);
            transform.Translate(Vector3.up * Time.deltaTime * speed/15);
        }
    }
}

    public void fly()
    {
        start = true;
    }
}
