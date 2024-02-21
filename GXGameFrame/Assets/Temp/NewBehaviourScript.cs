using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    // Start is called before the first frame update
    public List<GameObject> cubesInScene = new List<GameObject>();

    void Start()
    {
        var allObjects = FindObjectsOfType<MeshFilter>();
        foreach (var obj in allObjects)
        {
            if (obj.sharedMesh.name == "Cube") // 假设默认Cube网格的名字是"Cube"
            {
                cubesInScene.Add(obj.gameObject);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var VARIABLE in cubesInScene)
        {
            VARIABLE.transform.position += new Vector3(0.1f, 0.1f, 0.1f);
        }
    }
}
