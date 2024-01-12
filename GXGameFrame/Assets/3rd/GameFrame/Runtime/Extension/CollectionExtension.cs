using System.Collections.Generic;

namespace GameFrame
{
    public static class CollectionExtension
    {
        public static void RemoveAtSwapBack<T>(this List<T> list, int index)
        {
            int tail = list.Count - 1;
            if (index != tail)
                list[index] = list[tail];
            list.RemoveAt(tail);
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

    }
}