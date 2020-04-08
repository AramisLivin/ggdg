using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using CollabProxy.UI;
using GoogleARCore;
using Microsoft.SqlServer.Server;
using Unity.UNetWeaver;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

public class PhotoResolver : MonoBehaviour
{
    public Material CameraMaterial;
    private Camera MainCamera;
    private Texture2D tex;
    private MeshCollider coll;
    public Vector3 BottleBot;
    
    private void Start()
    {
        MainCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        
    }


    private void Update()
    {
       
    }

    //Scene is started // BUTTON IS PRESSED
    public void Shot()
    {
        StartCoroutine(UploadPNG());
        
    }

    
    
    IEnumerator UploadPNG()
    {
        while (true)
        {
            // We should only read the screen buffer after rendering is complete
            yield return new WaitForEndOfFrame();
            // Create a texture the size of the screen, RGB24 format
            int width = Screen.width;
            int height = Screen.height;
            tex = new Texture2D(width, height, TextureFormat.RGB24, false);

            // Read screen contents into the texture
            tex = (Texture2D) CameraMaterial.mainTexture;
            byte[] bytes = tex.EncodeToPNG();
           // tex.GetPixel()
            
           // Object.Destroy(tex);
            Upload(bytes);
            
            // For testing purposes, also write to a file in the project folder
            File.WriteAllBytes(Application.dataPath + "/SavedScreen.png", bytes);
            Debug.Log("Shotted to: " + Application.dataPath + " SavedScreen.png");
            yield return new WaitForSeconds(1f);
        }
    }

    //Uploading camera view to the server to get masks
   static async void Upload(byte[] bytes) 
        {
           HttpClient httpClient = new HttpClient();
           httpClient.BaseAddress = new Uri("http://188.214.128.128:80/");
           MultipartFormDataContent form = new MultipartFormDataContent();
           form.Add(new ByteArrayContent(bytes, 0, bytes.Length), "image", "image.webp");
           var response = await httpClient.PostAsync("/api/bottle/masks", form);
           //Debug.Log("msg: " + response.StatusCode.ToString());
           response.EnsureSuccessStatusCode();
           httpClient.Dispose();
           string sd = response.Content.ReadAsStringAsync().Result;
        }

   
   
    static async void Download() // Download mask from the server
    {    
        
    }

    
    
    //Checking downloaded mask from the server for a bottles
    private void ScanColor(Texture2D Frame)
    {    //Here comes x,y,width,height
        int maskX = 500, maskY = 500, width = 500, height = 500, CountOfMasks = 1;
        //for (int i = 0; i < CountOfMasks; i++)
        //{   
            Debug.Log("Mask "+" downloaded");
            BottleBot = new Vector3();
            TrackableHit hit;
            TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
                                              TrackableHitFlags.FeaturePointWithSurfaceNormal;
            
            
            if (GoogleARCore.Frame.Raycast(maskX + width / 2, maskY + height, raycastFilter, out hit))
            {
                BottleBot = hit.Pose.position;
                Debug.Log("Bottle has been found and placed on X: " + hit.Pose.position.x +", Y: " + hit.Pose.position.y + ", Z: " + hit.Pose.position.z);
                Debug.Log("Bottle has been found and placed. Distance to it: "+ hit.Distance + "m.");
            }
            else
            {
                Debug.Log("Bottle has not been found!");
            }
            
            
        //}  
    }
    }
    
    
    

