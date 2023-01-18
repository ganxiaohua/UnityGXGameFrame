using UnityEngine;

public class Singleton<T> where T : class, new()
{
    private static T _instance;
    // 加锁
    private static object _lock = new object();

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new T();
                    }
                }
            }
            return _instance;
        }
    }
}

public class SingletonMono<T> : MonoBehaviour where T : Component
{
    private static T _instance;
    // 加锁
    private static object _lock = new object();

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        GameObject a = new GameObject();
                        a.name = typeof(T).Name;
                        _instance = a.AddComponent<T>();
                    }
                }
            }
            return _instance;
        }
    }
}