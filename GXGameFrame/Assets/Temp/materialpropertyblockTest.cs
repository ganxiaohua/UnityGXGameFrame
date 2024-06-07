using System.Collections.Generic;
using UnityEngine;

public class materialpropertyblockTest : MonoBehaviour
{
    public List<GameObject> goLIST;

    void Start()
    {
        foreach (var item in goLIST)
        {
            MaterialPropertyBlock m = new MaterialPropertyBlock();
            var renderer = item.GetComponent<MeshRenderer>();
            m.SetColor("_Color", new Color(Random.Range(0, 1.0f), Random.Range(0, 1.0f), Random.Range(0, 1.0f), 1));
            m.SetVector("_MiningTex_ST",new Vector4(Random.Range(0, 1.0f),Random.Range(0, 1.0f),Random.Range(0, 1.0f),Random.Range(0, 1.0f)));
            renderer.SetPropertyBlock(m);
        }
    }

    void Update()
    {
    }
}