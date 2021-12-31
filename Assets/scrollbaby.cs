using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class scrollbaby : MonoBehaviour
{
    private float distPerSecond;
    private float timeElapsed = 0;
    public ImagePool imgPool;
    private float distTraveled = 0;
    public static float timeToGoFromOffscreenToOffScreen = 30.0f;

    private bool pause = false;
    private bool generateRow = false;
    // Start is called before the first frame update
    void Start()
    {
        //we want the distance per second to try and remove the images from off screen to off screen in 20 seconds.
        distPerSecond = Screen.height / 16.0f;
    }

    //Update is called once per frame
    void Update()
    {
        timeElapsed += Time.deltaTime;
        if (pause)
        {
            if (timeElapsed > 2)
            {
                pause = false;
            }
            else
            {
                if (generateRow)
                {
                    //time to go from offscreen to offscreen if at edges is exactly: 1x rectangle height + screen height,
                    //+ 2 seconds for each pause.
                    //There will be a number of pauses with it on screen = number of rows on screen = screen height / image height
                    timeToGoFromOffscreenToOffScreen = ((imgPool.rectHeight + Screen.height) + (Screen.height % imgPool.rectHeight)) / (distPerSecond +
                                                       2.0f * ((int) (Screen.height / imgPool.rectHeight) + 2));
                    imgPool.GenerateRow();
                    generateRow = false;
                }
                return;
            }
        }
        Vector3 up = new Vector3(0.0f, distPerSecond, 0.0f);
        transform.position = gameObject.transform.position + up * Time.deltaTime;
        distTraveled += up.y * Time.deltaTime;
        
        //run the scroll until it has run for an image height. When it has, pause it for two seconds, generate a row off screen, and then keep running.
        if (distTraveled > imgPool.rectHeight)
        {
            pause = true;
            generateRow = true;
            timeElapsed = 0.0f;
            Debug.Log("Reached image height");
            distTraveled -= imgPool.rectHeight;
        }
    }
}
