using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Station_Behaviour : MonoBehaviour
{
    public GameObject UAV1;
    public GameObject UAV2;

    private GameObject uav1;
    private Scan uav1Script;
    private Vector3 uav1OriginalPos;
    private bool uav1Completed;

    private GameObject uav2;
    private Scan uav2Script;
    private Vector3 uav2OriginalPos;
    private bool uav2Completed;

    public GameObject setpointsObjects;
    public Aircraft aircraft;

    void Start()
    {
        InitializeUAVs();

        uav1OriginalPos = uav1.transform.position;
        uav2OriginalPos = uav2.transform.position;
        StartCoroutine(WaitForStart());
    }

    void InitializeUAVs()
    {
        Vector3 uav1Coordonates = new Vector3(transform.position.x+7, transform.position.y+8, transform.position.z);
        Vector3 uav2Coordonates = new Vector3(transform.position.x-7, transform.position.y+8, transform.position.z);

        uav1 = Instantiate(UAV1, uav1Coordonates, transform.rotation);
        uav2 = Instantiate(UAV2, uav2Coordonates, transform.rotation);

        uav2.name = "UAV1";
        uav2.name = "UAV2";
        
        uav1Script = uav1.GetComponent<Scan>();
        uav2Script = uav2.GetComponent<Scan>();

        uav1Script.station = uav1Coordonates;
        uav2Script.station = uav2Coordonates;

        uav1Script.otherUav = uav2Script;
        uav2Script.otherUav = uav1Script;

        Transform[] children = setpointsObjects.GetComponentsInChildren<Transform>();
        uav1Script.start = children[1].transform.position;
        uav1Script.end = children[2].transform.position;
        uav2Script.start = children[2].transform.position;
        uav2Script.end = children[1].transform.position;

        uav1Script.speed = 100;
        uav2Script.speed = 100;
    }

    IEnumerator WaitForStart()
    {
        // Wait 1 second before starting the WaitForUAVs coroutine.
        yield return new WaitForSeconds(1.0f);

        StartCoroutine(WaitForUAVs());
    }

    IEnumerator WaitForUAVs()
    {
        const float threshold = 0.1f;
        while (!uav1Completed || !uav2Completed)
        {
            uav1Completed = Vector3.Distance(uav1.transform.position, uav1OriginalPos) < threshold;
            uav2Completed = Vector3.Distance(uav2.transform.position, uav2OriginalPos) < threshold;

            yield return new WaitForSeconds(1.0f);
        }

        if (uav1Script.disformityImages.Count == 0 && uav2Script.disformityImages.Count == 0)
        {
            aircraft.fly();
        }
        else
        {
            SaveDisformityImages(uav1Script.disformityImages, uav1.name);
            SaveDisformityImages(uav2Script.disformityImages, uav2.name);
            
            Debug.Log("Saved " + (uav1Script.disformityImages.Count + uav1Script.disformityImages.Count) + " images." );
        }
    }

    void SaveDisformityImages(Dictionary<string, Texture2D> disformityImages, string name)
    {
        int i = 0;
        foreach (KeyValuePair<string, Texture2D> entry in disformityImages)
        {
            string key = entry.Key;
            Texture2D texture = entry.Value;
            // Convert the texture to a PNG byte array
            byte[] bytes = texture.EncodeToPNG();
            // Save the byte array to a file
            string fileName = name + "_" + i + "_" + key + ".png";
            string filePath = Path.Combine("Assets/Images/", fileName);
            File.WriteAllBytes(filePath, bytes);
            i++;
        }
    }
}
