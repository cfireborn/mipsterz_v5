using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controls : MonoBehaviour
{
    public GameObject Controls1;
    public GameObject Controls2;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    private bool enabled = true;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (enabled)
            {
                Controls1.SetActive(false);
                Controls2.SetActive(false);
                enabled = false;
            }
            else
            {
                Controls1.SetActive(true);
                Controls2.SetActive(true);
                enabled = true;
            }
        }
    }
}
