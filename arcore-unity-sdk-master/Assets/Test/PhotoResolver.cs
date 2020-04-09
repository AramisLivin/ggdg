using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using CollabProxy.UI;
using Google.Protobuf;
using GoogleARCore;
using Microsoft.SqlServer.Server;
using Unity.UNetWeaver;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;
using System.Text.Json;

public class PhotoResolver : MonoBehaviour
{
    public Material CameraMaterial;
    private Camera MainCamera;
    private Texture2D tex;
    private MeshCollider coll;
    public Vector3 BottleBot;
    private List<List<int>> locations;
    private List<byte[]> images;
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
        StartCoroutine(CapturePNG());
        
    }

    
    
    IEnumerator CapturePNG()
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
           
           var result = GetMaskFromServer(bytes).Result;
           locations = result.Item1;
           images = result.Item2;
           
           
            // For testing purposes, also write to a file in the project folder
            File.WriteAllBytes(Application.dataPath + "/SavedScreen.png", bytes);
            Debug.Log("Shotted to: " + Application.dataPath + " SavedScreen.png");
            yield return new WaitForSeconds(1f);
        }
    }

    //Uploading camera view to the server to get masks
    static async Task<Tuple<List<List<int>>, List<byte[]>>> GetMaskFromServer(byte[] bytes) 
    {
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("http://188.214.128.128:80/");
        MultipartFormDataContent form = new MultipartFormDataContent();
        form.Add(new ByteArrayContent(bytes, 0, bytes.Length), "image", "image.webp");
        var response = await httpClient.PostAsync("/api/bottle/masks", form);
        response.EnsureSuccessStatusCode();
        httpClient.Dispose();
        var provider = await response.Content.ReadAsMultipartAsync();
        // Default empty value
        List<List<int>> result_locs = new List<List<int>>();
        List<byte[]> result_imgs = new List<byte[]>();
        
        // Parse response
        foreach (var httpContent in provider.Contents)
        {
            string fileName = httpContent.Headers.ContentDisposition.FileName;
            if (fileName.Equals("\"locations.json\""))
            {
                string locs = httpContent.ReadAsStringAsync().Result;
                result_locs = JsonSerializer.Deserialize<LocationsJSON>(locs).locations;
            }
            else
            {
                byte[] img = httpContent.ReadAsByteArrayAsync().Result;
                result_imgs.Add(img);
            }
        }
        return new Tuple<List<List<int>>, List<byte[]>>(result_locs, result_imgs);
    }
    
    //Checking downloaded mask from the server for a bottles
    private void ScanColor(Texture2D Frame)
    {   
        //Count of masks is writed from responce
        int CountOfMasks = locations.Count;
        for (int i = 0; i < CountOfMasks; i++)
        {
            //Here comes x,y,width,height
            int maskX = locations[i][0];
            int maskY = locations[i][1];
            int width = locations[i][2];
            int height = locations[i][3];
            
            
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
            
            
        }  
    }
    }
    
public class LocationsJSON
{
    public List<List<int>> locations { get; set; }
}
    
    

