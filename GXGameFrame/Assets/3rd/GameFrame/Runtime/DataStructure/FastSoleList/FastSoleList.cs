using System.Collections.Generic;

namespace GameFrame
{
    public class FastSoleList<T> where T : IContinuousID
    {
        private List<DirtyBlock> blocks;

        private List<T> datas;

        private int capacity;

        public int Count => datas.Count;

        public FastSoleList(int capacity)
        {
            this.capacity = capacity;
            datas = new List<T>(capacity);
            blocks = new List<DirtyBlock>(1);
        }

        public void SetCapacity(int capacity)
        {
            this.capacity = capacity;
            datas.Capacity = capacity;
            Assert.IsTrue(blocks.Count == 0, "快内已经有成员,不允许修改");
        }

        public bool Add(T data)
        {
            int blockIndex = data.ID / capacity;
            int subBlockIndex = data.ID % capacity;
            if (blocks.Count <= blockIndex)
            {
                blocks.Add(new DirtyBlock(capacity));
            }

            var block = blocks[blockIndex];
            if (block.Dirty(subBlockIndex))
            {
                datas.Add(data);
                return true;
            }

            return false;
        }

        public bool Remove(T data)
        {
            int blockIndex = data.ID / capacity;
            int subBlockIndex = data.ID % capacity;
            if (blockIndex >= blocks.Count || subBlockIndex > blocks[blockIndex].pointer)
            {
                return false;
            }

            var block = blocks[blockIndex];
            if (block.Erase(subBlockIndex))
            {
                datas.RemoveSwapBack(data);
                return true;
            }

            return false;
        }


        public void Clear()
        {
            int count = blocks.Count;
            for (int i = 0; i < count; i++)
            {
                blocks[i].Clear();
            }

            datas.Clear();
        }

        public List<T>.Enumerator GetEnumerator() => this.datas.GetEnumerator();
    }
}