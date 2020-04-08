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
    public Texture2D tex;
    private MeshCollider coll;
    
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
            ScanColor(tex);
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
           //Debug.Log("msg: " + sd);
           /*int loop1;
           HttpFileCollection Files;
 
           var Files =response.Headers; // Load File collection into HttpFileCollection variable.
           arr1 = Files.AllKeys;  // This will get names of all files into a string array.
           for (loop1 = 0; loop1 < arr1.Length; loop1++) 
           {
               Response.Write("File: " + Server.HtmlEncode(arr1[loop1]) + "<br />");
               Response.Write("  size = " + Files[loop1].ContentLength + "<br />");
               Response.Write("  content type = " + Files[loop1].ContentType + "<br />");
           }*/
        }

   
   
    static async void Download() // Download mask from the server
    {    
        
    }

    
    
    //Checking downloaded mask from the server for a bottles
    private void ScanColor(Texture2D Frame)
    { //Checking every pixel of the downloaded mask for a mask pixel with RGBA(0,0,0,1)
        //Here comes x,y,width,height
        int maskX = 500, maskY = 500, width = 500, height = 500;
       // for (int i = 0; i < 10; i++)
        //{   
            Debug.Log("Mask "+" downloaded");
            
            TrackableHit hit;
            TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
                                              TrackableHitFlags.FeaturePointWithSurfaceNormal;
            
            
            if (GoogleARCore.Frame.Raycast(maskX + width / 2, maskY + height, raycastFilter, out hit))
            {
                Debug.Log("Bottle has been found and placed on X: " + hit.Pose.position.x +", Y: " + hit.Pose.position.y + ", Z: " + hit.Pose.position.z);
                Debug.Log("Bottle has been found and placed. Distance to it: "+ hit.Distance + "m.");
            }
            else
            {
                Debug.Log("Bottle has not been found!");
            }
            /* boxes = ... //tuple of XYWH
                masks = ... //images arrays
                image // input image from camera
                    boxes, masks = request_from_server(image)
                // len(boxes) == len(masks)
                n = len(boxes)
                centers = [] // XY
                for (i = 0; i < n; ++i)
                    centers = [boxes[0] + boxes[2] // 2, boxes[1] + boxes[3] // 2]*/
        //}  
    }
    }
    
    
    

