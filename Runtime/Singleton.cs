using GameFrame.Runtime;
using UnityEngine;

public class Singleton<T> where T : class, new()
{
    private static T sInstance;

    // 加锁
    private static object _lock = new object();

    public static T Instance
    {
        get
        {
            if (sInstance == null)
            {
                lock (_lock)
                {
                    if (sInstance == null)
                    {
                        sInstance = new T();
                    }
                }
            }

            return sInstance;
        }
    }
}

public class SingletonInit<T> where T : class, IInitializeSystem, new()
{
    private static T sInstance;

    // 加锁
    private static object sLock = new object();

    public static T Instance
    {
        get
        {
            if (sInstance == null)
            {
                lock (sLock)
                {
                    if (sInstance == null)
                    {
                        sInstance = new T();
                        sInstance.OnInitialize();
                    }
                }
            }

            return sInstance;
        }
    }
}

public class SingletonMono<T> : MonoBehaviour where T : Component
{
    private static T sInstance;

    // 加锁
    private static object sLock = new object();

    public static T Instance
    {
        get
        {
            if (sInstance == null)
            {
                lock (sLock)
                {
                    if (sInstance == null)
                    {
                        GameObject a = new GameObject();
                        a.name = typeof(T).Name;
                        sInstance = a.AddComponent<T>();
                        DontDestroyOnLoad(a);
                    }
                }
            }

            return sInstance;
        }
    }
}