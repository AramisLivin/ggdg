using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using GoogleARCore;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.UI;

public class PhotoResolver : MonoBehaviour
{
    public Material CameraMaterial;
    private Camera MainCamera;
    private Texture2D tex;
    private MeshCollider coll;
    public Vector3 BottleBot;
    private List<List<float>> locations;
    private List<byte[]> images;
    private GameObject btlmsk;
    private GameObject btlbot;
    private RectTransform BtlMskRectTransform;
    private RectTransform BtlBtRectTransform;
   

    private void Start()
    {
          MainCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
            btlmsk = GameObject.Find("bottlemask");
            btlbot = GameObject.Find("bottlebot");
            BtlBtRectTransform = btlbot.GetComponent<RectTransform>();
            BtlMskRectTransform = btlmsk.GetComponent<RectTransform>();
            btlmsk.SetActive(false);
            btlbot.SetActive(false);
    }
    
    public void Shot()
    {
        StartCoroutine(CapturePNG());
    }

    

    Texture2D FlipPicture(Texture2D original)
    {
        int width = original.width;
        int height = original.height;
        Texture2D snap = new Texture2D(width, height);
        Color[] pixels = original.GetPixels();
        Color[] pixelsFlipped = new Color[pixels.Length];

        for (int i = 0; i < height; i++)
        {
            Array.Copy(pixels, i*width, pixelsFlipped, (height-i-1) * width , width);
        }

        snap.SetPixels(pixelsFlipped);
        snap.Apply();
        return snap;
    }
    
    IEnumerator CapturePNG()
    {
        while (true)
        {
           // Read screen and resizing contents into the texture
            tex = (Texture2D) CameraMaterial.mainTexture;
            
            float d = Math.Max(tex.width, tex.height);
            float k = 700 / d;
            int NewWidth = Convert.ToInt32(tex.width * k);
            int NewHeight = Convert.ToInt32(tex.height * k);
                
            TextureScale.Bilinear (tex, NewWidth, NewHeight);
            byte[] bytes = FlipPicture(tex).EncodeToJPG(40);
          
            var result = GetMaskFromServer(bytes).Result;
            
            locations = result.Item1;
            images = result.Item2;
             
             MaskPositioning();
             yield return new WaitForChangedResult();
        }
    }

    //Uploading camera view to the server to get masks
    static async Task<Tuple<List<List<float>>, List<byte[]>>> GetMaskFromServer(byte[] bytes)
    {
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("http://188.214.128.128:80/");
        MultipartFormDataContent form = new MultipartFormDataContent();
        form.Add(new ByteArrayContent(bytes, 0, bytes.Length), "image", "image.webp");
        var response = httpClient.PostAsync("/api/bottle/masks", form).Result;
        response.EnsureSuccessStatusCode();
        httpClient.Dispose();
        
        var provider = await response.Content.ReadAsMultipartAsync();
        // Default empty value
        List<List<float>> result_locs = new List<List<float>>();
        List<byte[]> result_imgs = new List<byte[]>();
        
        // Parse response
        foreach (var httpContent in provider.Contents)
        {
            string fileName = httpContent.Headers.ContentDisposition.FileName;
            if (fileName.Equals("\"locations.json\""))
            {
                string locs = httpContent.ReadAsStringAsync().Result;
                JsonSerializer JS = new JsonSerializer();
                JsonTextReader reader = new JsonTextReader(new StringReader(locs));
                result_locs = JS.Deserialize<LocationsJSON>(reader).locations;
            }
            else
            {
                byte[] img = httpContent.ReadAsByteArrayAsync().Result;
                result_imgs.Add(img);
                }
        }
        return new Tuple<List<List<float>>, List<byte[]>>(result_locs, result_imgs);
    
    }    
    
    //Checking downloaded mask from the server for a bottles
    public void MaskPositioning ()
    {
        //Count of masks is writed from responce
        int CountOfMasks = locations.Count;
        if (CountOfMasks == 0)
        {
            btlmsk.SetActive(false);
            btlbot.SetActive(false);
            return;
        }
        for (int i = 0; i < CountOfMasks; i++)
            {
                //GameObject msk = new GameObject();
                Texture2D tex = new Texture2D(700,700);
                ImageConversion.LoadImage(tex, images[i]);
                btlmsk.GetComponent<RawImage>().texture = tex;
               
                //Here comes x,y,width,height
                float maskX = locations[i][0] * Screen.width;
                float maskY = locations[i][1] * Screen.height;
                float width = locations[i][2] * Screen.width;
                float height = locations[i][3] * Screen.height;
                
                //Debug saving
                //File.WriteAllBytes(Application.dataPath + "/SavedMask.png", images[i]);
                //Debug saving
                
                Debug.Log("Mask is on " + maskX +"; " + maskY+". Width " + width +"; height " + height);
                btlmsk.SetActive(true);
                btlbot.SetActive(true);
                BtlMskRectTransform.position = new Vector3(maskX,maskY);
                //BtlMskRectTransform.sizeDelta = new Vector2(width, height);
                BtlBtRectTransform.position = new Vector3(maskX,maskY);
                BottleBot = new Vector3();
                TrackableHit hit;
                TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
                                                  TrackableHitFlags.FeaturePointWithSurfaceNormal;

                if (GoogleARCore.Frame.Raycast(maskX + width / 2, maskY + height, raycastFilter, out hit))
                {
                    BottleBot = hit.Pose.position;
                    Debug.Log("Bottle has been found and placed on X: " + hit.Pose.position.x + ", Y: " +  
                              hit.Pose.position.y + ", Z: " + hit.Pose.position.z+". Distance to it: " + hit.Distance + "m.");
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
    public List<List<float>> locations { get; set; }
}
    
    

