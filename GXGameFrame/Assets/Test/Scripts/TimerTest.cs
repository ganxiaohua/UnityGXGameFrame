using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameFrame.Timer;
using UnityEngine;

public class TimerTest : MonoBehaviour
{
    // Start is called before the first frame update
    private int id;

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            id = TimerComponent.Instance.AddOnceTimer(2000, () => { Debug.Log("延迟调用"); });
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            TimerComponent.Instance.CancelTimer(id);
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            Test();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            TimerComponent.Instance.CancelTimer(id);
        }
    }

    async UniTaskVoid Test()
    {
        bool notcanel = await TimerComponent.Instance.OnceTimerAsync(10000, (x) => { id = x; });
        if (notcanel)
        {
            Debug.Log("异步延迟调用");
        }
        else
        {
            Debug.Log("取消调用");
        }
    }
}