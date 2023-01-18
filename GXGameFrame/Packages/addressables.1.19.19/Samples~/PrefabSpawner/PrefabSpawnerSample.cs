using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// A script that spawns and destroys a number of AssetReferences after a given delay.
/// </summary>
public class PrefabSpawnerSample : MonoBehaviour
{
    /// <summary>
    /// The prefab to spawn.
    /// </summary>
    public AssetReference SpawnablePrefab;
    /// <summary>
    /// The time, in seconds, to delay before spawning prefabs.
    /// </summary>
    public float DelayBetweenSpawns = 2.0f;
    /// <summary>
    /// The time, in seconds, to delay before destroying the spawned prefabs.
    /// </summary>
    public float DealyBeforeDestroying = 1.0f;
    /// <summary>
    /// The number of prefabs to spawn.
    /// </summary>
    public int NumberOfPrefabsToSpawn = 1;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(StartSpawner());
    }

    IEnumerator StartSpawner()
    {
        while(true)
        {
            yield return new WaitForSeconds(DelayBetweenSpawns);
            StartCoroutine(SpawnTemporaryCube());
        }
    }

    IEnumerator SpawnTemporaryCube()
    {
        List<AsyncOperationHandle<GameObject>> handles = new List<AsyncOperationHandle<GameObject>>();

        for (int i = 0; i < NumberOfPrefabsToSpawn; i++)
        {
            //Instantiates a prefab with the address "Cube".  If this isn't working make sure you have your Addressable Groups
            //window setup and a prefab with the address "Cube" exists.
            AsyncOperationHandle<GameObject> handle = SpawnablePrefab.InstantiateAsync();
            handles.Add(handle);
        }

        yield return new WaitForSeconds(DealyBeforeDestroying);

        //Release the AsyncOperationHandles which destroys the GameObject
        foreach (var handle in handles)
            Addressables.Release(handle);
    }
}
