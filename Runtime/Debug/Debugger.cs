using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace GameFrame.Runtime
{
    public static class Debugger
    {
        public static ILogger logger = (ILogger) null;
        // private static CString sb = new CString(256);

        static Debugger()
        {
            // for (int size = 24; size < 70; ++size)
            // StringPool.PreAlloc(size, 2);
        }
        
        [Conditional("UNITY_EDITOR")]
        public static void Log(object message)
        {
            Debugger.Log(message.ToString());
        }
        
        [Conditional("UNITY_EDITOR")]
        public static void Log(string str, object arg0)
        {
            Debugger.Log(string.Format(str, arg0));
        }

        [Conditional("UNITY_EDITOR")]
        public static void Log(string str, object arg0, object arg1)
        {
            Debugger.Log(string.Format(str, arg0, arg1));
        }

        [Conditional("UNITY_EDITOR")]
        public static void Log(string str, object arg0, object arg1, object arg2)
        {
            Debugger.Log(string.Format(str, arg0, arg1, arg2));
        }

        [Conditional("UNITY_EDITOR")]
        public static void Log(string str, params object[] param)
        {
            if (param != null)
            {
                Debug.Log(string.Format(str, param));
            }
            else
            {
                Debug.Log(str);
            }
        }

        [Conditional("UNITY_EDITOR")]
        public static void LogWarning(string str)
        {
            Debug.LogWarning((object) str);
        }

        [Conditional("UNITY_EDITOR")]
        public static void LogWarning(object message)
        {
            Debugger.LogWarning(message.ToString());
        }

        [Conditional("UNITY_EDITOR")]
        public static void LogWarning(string str, object arg0)
        {
            Debugger.LogWarning(string.Format(str, arg0));
        }

        [Conditional("UNITY_EDITOR")]
        public static void LogWarning(string str, object arg0, object arg1)
        {
            Debugger.LogWarning(string.Format(str, arg0, arg1));
        }
        
        [Conditional("UNITY_EDITOR")]
        public static void LogWarning(string str, object arg0, object arg1, object arg2)
        {
            Debugger.LogWarning(string.Format(str, arg0, arg1, arg2));
        }

        [Conditional("UNITY_EDITOR")]
        public static void LogWarning(string str, params object[] param)
        {
            Debugger.LogWarning(string.Format(str, param));
        }

        [Conditional("UNITY_EDITOR")]
        public static void LogError(string str)
        {
            Debug.LogError((object) str);
        }

        [Conditional("UNITY_EDITOR")]
        public static void LogError(object message) => Debugger.LogError(message.ToString());

        [Conditional("UNITY_EDITOR")]
        public static void LogError(string str, object arg0) => Debugger.LogError(string.Format(str, arg0));

        [Conditional("UNITY_EDITOR")]
        public static void LogError(string str, object arg0, object arg1) => Debugger.LogError(string.Format(str, arg0, arg1));

        [Conditional("UNITY_EDITOR")]
        public static void LogError(string str, object arg0, object arg1, object arg2) => Debugger.LogError(string.Format(str, arg0, arg1, arg2));

        [Conditional("UNITY_EDITOR")]
        public static void LogError(string str, params object[] param) => Debugger.LogError(string.Format(str, param));

    }
}