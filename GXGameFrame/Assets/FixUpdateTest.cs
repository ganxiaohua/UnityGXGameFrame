using UnityEngine;

public class FixUpdateTest : MonoBehaviour
{
    private float FixedTime;
    private float UpdateTime;

    void Start()
    {
        QualitySettings.vSyncCount = 0;
        //0.5秒一次
        Time.fixedDeltaTime = 0.5f;     
        //1秒一次.
        Application.targetFrameRate = 1;
    }

    private void FixedUpdate()
    {
        FixedTime += Time.deltaTime;
        Debug.Log("Fixed: " + FixedTime);
    }

    void Update()
    {
        UpdateTime += Time.deltaTime;
        Debug.LogWarning("update: " + UpdateTime);
    }
}