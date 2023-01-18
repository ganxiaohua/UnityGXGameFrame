using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class LoadSceneForCustomBuild : MonoBehaviour
{
    public string SceneKey;
    // Start is called before the first frame update
    void Start()
    {
        Addressables.LoadSceneAsync(SceneKey);
    }
}
