using System.Collections.Generic;
using GameFrame.Runtime;

namespace GameFrame.Runtime
{
    public static class ListPool<T>
    {
        internal static readonly Stack<List<T>> pool = new Stack<List<T>>();

        public static List<T> Get(int capacity = 0)
        {
            if (pool.Count == 0)
                return new List<T>(capacity);
            var list = pool.Pop();
            if (list.Capacity < capacity)
                list.Capacity = capacity;
            return list;
        }

        public static void Release(List<T> list)
        {
            Assert.IsFalse(pool.Contains(list), "Repeat enqueue occurred");
            if (list != null)
            {
                list.Clear();
                if (pool.Count <= 20)
                    pool.Push(list);
            }
        }
    }

    public static class QueuePool<T>
    {
        internal static readonly Stack<Queue<T>> pool = new Stack<Queue<T>>();

        public static Queue<T> Get(int capacity = 0)
        {
            if (pool.Count == 0)
                return new Queue<T>(capacity);
            var q = pool.Pop();
            return q;
        }

        public static void Release(Queue<T> q)
        {
            Assert.IsFalse(pool.Contains(q), "Repeat enqueue occurred");
            if (q != null)
            {
                q.Clear();
                if (pool.Count <= 20)
                    pool.Push(q);
            }
        }
    }

    public static class StackPool<T>
    {
        internal static readonly Stack<Stack<T>> pool = new Stack<Stack<T>>();

        public static Stack<T> Get(int capacity = 0)
        {
            if (pool.Count == 0)
                return new Stack<T>(capacity);
            var q = pool.Pop();
            return q;
        }

        public static void Release(Stack<T> q)
        {
            Assert.IsFalse(pool.Contains(q), "Repeat enqueue occurred");
            if (q != null)
            {
                q.Clear();
                if (pool.Count <= 20)
                    pool.Push(q);
            }
        }
    }

    public static class DictPool<TKey, TValue>
    {
        internal static readonly Stack<Dictionary<TKey, TValue>> pool = new Stack<Dictionary<TKey, TValue>>();

        public static Dictionary<TKey, TValue> Get(int capacity = 0)
        {
            if (pool.Count == 0)
                return new Dictionary<TKey, TValue>(capacity);
            var dict = pool.Pop();
            return dict;
        }

        public static void Release(Dictionary<TKey, TValue> dict)
        {
            Assert.IsFalse(pool.Contains(dict), "Repeat enqueue occurred");
            if (dict != null)
            {
                dict.Clear();
                if (pool.Count <= 20)
                    pool.Push(dict);
            }
        }
    }

    public static class HashSetPool<T>
    {
        internal static readonly Stack<HashSet<T>> pool = new Stack<HashSet<T>>();

        public static HashSet<T> Get()
        {
            if (pool.Count == 0)
                return new HashSet<T>();
            var hs = pool.Pop();
            return hs;
        }

        public static void Release(HashSet<T> hs)
        {
            Assert.IsFalse(pool.Contains(hs), "Repeat enqueue occurred");
            if (hs != null)
            {
                hs.Clear();
                if (pool.Count <= 20)
                    pool.Push(hs);
            }
        }
    }
}