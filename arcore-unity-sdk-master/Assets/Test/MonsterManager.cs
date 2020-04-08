using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterManager : MonoBehaviour
{
    
    // Start is called before the first frame update
    private GameObject Camera;
    private Object BottlePrefab;
    private Vector3 BottlePos;
    void Start()
    {
        Camera = GameObject.Find("ARCore Device");
        BottlePrefab = Resources.Load<GameObject>("BottlePref");
    }

    // Update is called once per frame
    void Update()
    {
        BottlePos = Camera.GetComponent<PhotoResolver>().BottleBot;
        BottleRecognize();
    }

    public void BottleRecognize()
    {
        Instantiate(BottlePrefab, BottlePos, Quaternion.identity);
    }
}
