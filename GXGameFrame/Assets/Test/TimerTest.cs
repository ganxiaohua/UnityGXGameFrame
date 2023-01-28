using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameFrame.Timer;
using LuaInterface;
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
           id =  TimerComponent.Instance.AddOnceTimer(2000, () =>
            {
                Debug.Log("延迟调用");
            });
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
        await TimerComponent.Instance.OnceTimerAsync(3000,(x)=>
        {
            id = x;
        }, () => { Debug.Log("取消的调用");});
        Debug.Log("异步延迟调用");
    }
}
