using System;
using System.Collections.Generic;

namespace GameFrame
{
    public static class CollectionExtension
    {
        public static void EnsureCapacity<T>(this List<T> list, int size)
        {
            if (list.Capacity < size)
                list.Capacity = size;
        }

        public static void EnsureSize<T>(this List<T> list, int size)
        {
            if (list.Capacity < size)
                list.Capacity = size;
            for (int i = list.Count; i < size; i++)
                list.Add(default(T));
        }

        public static void EnsureSize<T>(this List<T> list, int size, T defaultValue)
        {
            if (list.Capacity < size)
                list.Capacity = size;
            for (int i = list.Count; i < size; i++)
                list.Add(defaultValue);
        }

        public static List<T> Resize<T>(this List<T> list, int size)
        {
            if (list.Capacity < size)
                list.Capacity = size;
            if (size < list.Count)
                list.RemoveRange(size, list.Count - size);
            for (int i = list.Count; i < size; i++)
                list.Add(default(T));
            return list;
        }

        public static List<T> Resize<T>(this List<T> list, int size, T defaultValue)
        {
            if (list.Capacity < size)
                list.Capacity = size;
            if (size < list.Count)
                list.RemoveRange(size, list.Count - size);
            for (int i = list.Count - 1; i >= 0; i--)
                list[i] = defaultValue;
            for (int i = list.Count; i < size; i++)
                list.Add(defaultValue);
            return list;
        }

        public static T Peek<T>(this List<T> list)
        {
            return list[list.Count - 1];
        }

        public static bool TryPeek<T>(this List<T> list, out T val)
        {
            if (list.Count > 0)
            {
                val = list[list.Count - 1];
                return true;
            }

            val = default;
            return false;
        }

        public static T Pop<T>(this List<T> list)
        {
            int tail = list.Count - 1;
            T val = list[tail];
            list.RemoveAt(tail);
            return val;
        }

        public static bool TryPop<T>(this List<T> list, out T val)
        {
            if (list.Count > 0)
            {
                int tail = list.Count - 1;
                val = list[tail];
                list.RemoveAt(tail);
                return true;
            }

            val = default;
            return false;
        }

        public static void RemoveAtSwapBack<T>(this List<T> list, int index)
        {
            int tail = list.Count - 1;
            if (index != tail)
                list[index] = list[tail];
            list.RemoveAt(tail);
        }

        public static T RemoveAtSwapBack<T>(this List<T> list, ref int index)
        {
            T removed = list[index];
            int tail = list.Count - 1;
            if (index != tail)
                list[index] = list[tail];
            else
                index = -1;
            list.RemoveAt(tail);
            return removed;
        }

        public static bool RemoveSwapBack<T>(this List<T> list, T value)
        {
            int index = list.IndexOf(value);
            if (index != -1)
            {
                list.RemoveAtSwapBack(index);
                return true;
            }

            return false;
        }

        public static int MoveAt<T>(this List<T> list, int index, int step)
        {
            if (index < 0 || index >= list.Count)
                return -1;
            int goal = index + step;
            if (goal < 0 || goal >= list.Count)
                return -1;
            T value = list[index];
            list.RemoveAt(index);
            list.Insert(goal, value);
            return goal;
        }

        public static int Move<T>(this List<T> list, T value, int step)
        {
            return list.MoveAt(list.IndexOf(value), step);
        }
    }
}