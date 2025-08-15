using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameFrame.Runtime
{
    public sealed partial class HintPanel
    {
        public static void EasyShow(string content, bool alert = false, bool touchable = false)
        {
#if UNITY_EDITOR
            Debug.Log($"[editor]Hint: {content}");
#endif
            UISystem.Instance.ShowUniquePanelAsync<HintPanel>(new Input()
            {
                    content = content,
                    alert = alert,
                    touchable = touchable,
            }).Forget();
        }
    }
}