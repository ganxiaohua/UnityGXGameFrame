using System;
using UnityEngine;

namespace GameFrame.Runtime.Editor
{
    public abstract class VirtualBaseDialog : VirtualDialog
    {
        protected Action callbackOK = null;
        protected Action callbackCancel = null;
        protected Action callbackApply = null;

        public void SetModified()
        {
            modified = true;
        }

        public static void Error(string error)
        {
            Debug.LogError(error);
        }

        public bool ButtonOK()
        {
            GUI.color = top ? Color.green : Color.white;
            bool ret = GUILayout.Button("确定", GUILayout.Width(80), GUILayout.Height(24));
            if (ret)
            {
                callbackOK?.Invoke();
            }

            return ret;
        }

        public bool ButtonCancel()
        {
            GUI.color = top ? Color.red : Color.white;
            bool ret = GUILayout.Button("取消", GUILayout.Width(80), GUILayout.Height(24));
            if (ret)
            {
                callbackCancel?.Invoke();
            }

            return ret;
        }

        public bool ButtonApply()
        {
            GUI.color = top ? Color.yellow : Color.white;
            bool ret = GUILayout.Button("应用", GUILayout.Width(80), GUILayout.Height(24));
            if (ret)
            {
                callbackApply?.Invoke();
            }

            return ret;
        }
    }
}