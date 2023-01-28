using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AssetsLoad : MonoBehaviour
{
    // Start is called before the first frame update
    public Image m_Image;
    void Start()
    {
        AssetManager.Instance.LoadAsync<Sprite>("Decorate_1_1", (x) =>
        {
            m_Image.sprite = x as Sprite ;
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
