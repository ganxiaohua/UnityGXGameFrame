#if UNITY_EDITOR || UNITY_STANDALONE_WIN
using UnityEngine;

namespace GameFrame.Runtime
{
    public class DefaultWindowsKeyboardHandler : IKeyboardHandler
    {
        public void OnUpdateKeyboardEvent()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnEscape();
            }
        }

        private void OnEscape()
        {
            Debug.Log("退出游戏！");
        }
    }
}
#endif