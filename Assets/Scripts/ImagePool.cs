using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.IO;
using System.Threading;
using UnityEngine.Serialization;

//Thanks Jerimiah https://gamedev.stackexchange.com/questions/102431/how-to-create-gui-image-with-script
static class MyExtensions
{
    private static System.Random rnd = new System.Random();
    public static void Shuffle<T>(this List<T> list)  
    {  
        int n = list.Count;  
        while (n > 1) {  
            n--;  
            int k = rnd.Next(n + 1);  
            T value = list[k];  
            list[k] = list[n];  
            list[n] = value;  
        }  
    }
    public static Vector2 SizeToParent(this RawImage image, float padding = 0) {
        float w = 0, h = 0;
        var parent = image.transform.parent.GetComponent<RectTransform>();
        var imageTransform = image.GetComponent<RectTransform>();
 
        // check if there is something to do
        if (image.texture != null) {
            if (!parent) { return imageTransform.sizeDelta; } //if we don't have a parent, just return our current width;
            padding = 1 - padding;
            float ratio = image.texture.width / (float)image.texture.height;
            var bounds = new Rect(0, 0, parent.rect.width, parent.rect.height);
            if (Mathf.RoundToInt(imageTransform.eulerAngles.z) % 180 == 90) {
                //Invert the bounds if the image is rotated
                bounds.size = new Vector2(bounds.height, bounds.width);
            }
            //Size by height first
            h = bounds.height * padding;
            w = h * ratio;
            if (w > bounds.width * padding) { //If it doesn't fit, fallback to width;
                w = bounds.width * padding;
                h = w / ratio;
            }
        }
        imageTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
        imageTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
        return imageTransform.sizeDelta;
    }
}
public class ImagePool : MonoBehaviour
{
    private List<string> FilePaths;
    private Queue<string> FileQueue;
    public GameObject Grid; //Grid panel you want to add all your images and videos to
    public int NumImagesPerRow = 5;
    public ImagePool()
    {
        FilePaths = new List<string>();
    }

    private int rectWidth;
    public int rectHeight = 20000;
    private int gridElementY = 0;

    private void Awake()
    {
        rectWidth = Screen.width / NumImagesPerRow;
        rectHeight = (int) (rectWidth * 1920.0 / 1080.0);
    }

    // Start is called before the first frame update
    void Start()
    {
        rectWidth = Screen.width / NumImagesPerRow;
        rectHeight = (int) (rectWidth * 1920.0 / 1080.0);
        gridElementY += rectHeight * 2;
        //sets squares to appropriate size for images to size themselves to
        RenderTexture tex = new RenderTexture(RenderTexture.GetTemporary(rectWidth, rectHeight));
        RawImage img = Grid.AddComponent<RawImage>();
        img.texture = tex;
        img.SetNativeSize();

        string path = Directory.GetCurrentDirectory();
        path = path + "/Feed";
        var info = new DirectoryInfo(path);
        //Get every image and append to list
        var fileInfo = info.GetFiles("*.jpg");
        foreach (FileInfo file in fileInfo)
        {
            FilePaths.Add(file.ToString());
            print(file.ToString());
        }
        //Get every image and append to list
        fileInfo = info.GetFiles("*.png");
        foreach (FileInfo file in fileInfo)
        {
            FilePaths.Add(file.ToString());
            print(file.ToString());
        }
        fileInfo = info.GetFiles("*.mp4");
        //Get every video and append to list
        foreach (FileInfo file in fileInfo)
        {
            FilePaths.Add(file.ToString());
            print(file.ToString());
        }
        foreach (string file in FilePaths)
        {
             print(file.ToString());
        }
        FilePaths.Shuffle();
        foreach (string file in FilePaths)
        {
             print(file.ToString());
        }
        FileQueue = new Queue<string>(FilePaths);
        GenerateRow();
        GenerateRow();
        GenerateRow();
        GenerateRow();
        GenerateRow();
        GenerateRow();
    }
    
    IEnumerator SelfDestruct(GameObject element)
    {
        float lifespan = scrollbaby.timeToGoFromOffscreenToOffScreen;
        //Debug.Log("lifespan of object is " + lifespan);
        yield return new WaitForSeconds(lifespan);
        Destroy(element);
    }
    
    IEnumerator FadeImage(bool fadeAway, RawImage img)
    {
        // fade from opaque to transparent 
        if (fadeAway)
        {
            // loop over 1 second backwards
            for (float i = 1; i >= 0; i -= Time.deltaTime)
            {
                // set color with i as alpha
                img.color = new Color(1, 1, 1, i);
                yield return null;
            }
        }
        // fade from transparent to opaque
        else
        {
            // loop over 2 seconds
            for (float i = 0; i <= 2; i += Time.deltaTime)
            {
                // set color with i as alpha
                img.color = new Color(1, 1, 1, i);
                yield return null;
            }
        }
    }
     
    private const double EPSILON = 0.03;
    public void GenerateRow()
    {
        Debug.Log(gridElementY + " is the Y for this row");
        int gridElementX = 0 + rectWidth/2;
        for (int i = 0; i < NumImagesPerRow; i++)
        {
            GameObject gridElement = new GameObject();
            gridElement.transform.SetParent(Grid.transform);
            gridElement.transform.SetPositionAndRotation(Grid.transform.TransformPoint(Vector3.zero), gridElement.transform.rotation);
            Vector3 localPos = gridElement.transform.position;
            Quaternion localRot = gridElement.transform.rotation;
            localPos.x = gridElementX;
            localPos.y += gridElementY;
            gridElement.transform.SetPositionAndRotation(localPos,localRot);

            //Grab file path
            string filePath = FileQueue.Dequeue();
            FileQueue.Enqueue(filePath);
            bool destroyMe = false; 
            //if video
            if (filePath.Contains(".mp4"))
            {
                RenderTexture tex = new RenderTexture(RenderTexture.GetTemporary(rectWidth, rectHeight));
                VideoPlayer videoPlayer = gridElement.AddComponent<VideoPlayer>();
                videoPlayer.renderMode = VideoRenderMode.RenderTexture;
                videoPlayer.source = VideoSource.Url;
                videoPlayer.url = filePath;
                videoPlayer.aspectRatio = VideoAspectRatio.FitInside;
                videoPlayer.targetTexture = tex;
                RawImage img = gridElement.AddComponent<RawImage>();
                img.texture = tex;
                img.SetNativeSize();
                Debug.Log("Video" + localPos.x + " and next element = " + gridElementX);
            }
            else if (filePath.Contains(".jpg") || filePath.Contains(".png")) //if image
            {
                WWW www = new WWW (filePath);
                // AspectRatioFitter aspectRatioFitter = imgElement.AddComponent<AspectRatioFitter>();
                // aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;    
                RawImage img = gridElement.AddComponent<RawImage>();
                
                { //reset to 1x1
                    RenderTexture tex = new RenderTexture(RenderTexture.GetTemporary(rectWidth, rectHeight));
                    RawImage imgparent = Grid.GetComponent<RawImage>();
                    imgparent.texture = tex;
                    imgparent.SetNativeSize();
                }
                
                img.texture = www.texture;
                //correct parent rect size before setting native size
                //compare texture height and width to determine appropriate positioning
                //if width / 1080 / height / 1920 == 2, then double width of rectangle, position it an extra .5 width over
                destroyMe = false; 
                if (Math.Abs(2 - (img.texture.width / 1080.0) / (img.texture.height / 1920.0)) < EPSILON)
                {
                    RenderTexture tex = new RenderTexture(RenderTexture.GetTemporary(rectWidth * 2, rectHeight));
                    RawImage imgparent = Grid.GetComponent<RawImage>();
                    imgparent.texture = tex;
                    imgparent.SetNativeSize();
                    if (i >= NumImagesPerRow - 1)
                    {
                        Debug.Log("skipping: Row: " + gridElementY + "col: " + i);
                        destroyMe = true;
                    }
                    i += 1;
                    gridElementX += rectWidth;
                    localPos.x += rectWidth * 0.5f;
                    gridElement.transform.SetPositionAndRotation(localPos,localRot);
                    Debug.Log("two width, x position = " + localPos.x + " and next element = " + gridElementX);
                }
                //if == 3, then triple width of rectangle, position it an extra 1 width over
                else if (Math.Abs(3 - ((img.texture.width / 1080.0) / (img.texture.height / 1920.0))) < EPSILON)
                {
                    RenderTexture tex = new RenderTexture(RenderTexture.GetTemporary(rectWidth * 3, rectHeight));
                    RawImage imgparent = Grid.GetComponent<RawImage>();
                    imgparent.texture = tex;
                    imgparent.SetNativeSize();
                    if (i >= NumImagesPerRow - 2)
                    {
                        Debug.Log("skipping: Row: " + gridElementY + "col: " + i);
                        destroyMe = true;
                    }
                    i += 2;
                    gridElementX += rectWidth * 2;
                    localPos.x += rectWidth * 1f;
                    gridElement.transform.SetPositionAndRotation(localPos, localRot);
                    Debug.Log("three width, x position = " + localPos.x + " and next element = " + gridElementX);
                }//if == 4, then quadrouple width of rectangle, move it extra 1.5 over
                else if (Math.Abs(4 - ((img.texture.width / 1080.0) / (img.texture.height / 1920.0))) < EPSILON)
                {
                    RenderTexture tex = new RenderTexture(RenderTexture.GetTemporary(rectWidth * 4, rectHeight));
                    RawImage imgparent = Grid.GetComponent<RawImage>();
                    imgparent.texture = tex;
                    imgparent.SetNativeSize();
                    if (i >= NumImagesPerRow - 3)
                    {
                        Debug.Log("skipping: Row: " + gridElementY + "col: " + i);
                        destroyMe = true;
                    }
                    i += 3;
                    gridElementX += rectWidth * 3;
                    localPos.x += rectWidth * 1.5f;
                    gridElement.transform.SetPositionAndRotation(localPos, localRot);
                    Debug.Log("four width, x position = " + localPos.x + " and next element = " + gridElementX);
                }

                // ReadImageAsync(img, filePath);
                img.SetNativeSize();
                img.SizeToParent();
                // fades the image in
                if (destroyMe)
                {
                    Destroy(gridElement);
                }
                else
                {
                    StartCoroutine(FadeImage(false, img));
                }
            }

            if (!destroyMe)
            {
                StartCoroutine(SelfDestruct(gridElement));
            }
            //Set up for the next x
            gridElementX += rectWidth;
        }
        
        //Set up for the next y
        gridElementY -= rectHeight;
    }


    // IEnumerator Load (RawImage img, WWW www, string filePath) {
    //     while(!www.isDone)
    //         yield return null;
    //     img.texture = www.texture;
    // }
    
    //https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/HOWTO-UICreateFromScripting.html
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Generating row");
            GenerateRow();
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Debug.Log("jumping backward");
            Vector3 localPos = Grid.transform.position;
            localPos.y -= Screen.height / 20.0f;
            Grid.transform.SetPositionAndRotation(localPos, Grid.transform.rotation);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Debug.Log("jumping forward");
            Vector3 localPos = Grid.transform.position;
            localPos.y += Screen.height / 20.0f;
            Grid.transform.SetPositionAndRotation(localPos, Grid.transform.rotation);
        }
    }
}
//     void Awake()
//     {
//         //Enable Callback on the main Thread
//         UnityThread.initUnityThread();
//     }
//     void ReadImageAsync(RawImage img, string filePath)
//     {
//
//         //Use ThreadPool to avoid freezing
//         ThreadPool.QueueUserWorkItem(delegate
//         {
//             bool success = false;
//
//             byte[] imageBytes;
//             FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
//
//             try
//             {
//                 int length = (int)fileStream.Length;
//                 imageBytes = new byte[length];
//                 int count;
//                 int sum = 0;
//
//                 // read until Read method returns 0
//                 while ((count = fileStream.Read(imageBytes, sum, length - sum)) > 0)
//                     sum += count;
//
//                 success = true;
//             }
//             finally
//             {
//                 fileStream.Close();
//             }
//
//             //Create Texture2D from the imageBytes in the main Thread if file was read successfully
//             if (success)
//             {
//                 UnityThread.executeInUpdate(() =>
//                 {
//                     Texture2D tex = new Texture2D(2, 2);
//                     tex.LoadImage(imageBytes);
//                     img.texture = tex;
//                 });
//             }
//         });
//     }
// }
