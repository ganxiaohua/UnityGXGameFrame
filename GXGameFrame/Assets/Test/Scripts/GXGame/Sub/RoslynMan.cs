
using GameFrame;
using GXGame;
using UnityEngine;

public class RoslynMan 
{
    // Start is called before the first frame update
    public static void Start()
    {
        Debug.Log("这里是 RoslynMan!!!!!!!!!!!!!!!!!!!!!!!!");
        RoslynSub0 roslynSub0 = new RoslynSub0();
        roslynSub0.Start();
        // Compile and load code
        GXGameFrame.Instance.Start();
        // SceneEntityFactory.CreateScene<BattlegroundScene>(GXGameFrame.Instance.MainScene);
        UIManager.Instance.OpenUI(typeof(UIHomeMainPanel));
    }

    public static void Update()
    {
        GXGameFrame.Instance.Update();
    }
}
