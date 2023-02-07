using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Additive : MonoBehaviour
{
    // Start is called before the first frame update
    private Material a;
    void Start()
    {
        a = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            a.EnableKeyword("CLIPPED");
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            a.DisableKeyword("CLIPPED");
        }
    }
}
