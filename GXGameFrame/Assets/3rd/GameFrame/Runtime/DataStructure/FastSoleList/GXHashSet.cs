using System.Collections.Generic;

namespace GameFrame
{
    public class GXHashSet<T> where T : IContinuousID
    {
        private List<DirtyBlock> blocks;

        private StrongList<T> datas;

        private int capacity;

        public int Count => datas.Count;

        public GXHashSet(int capacity)
        {
            this.capacity = capacity;
            datas = new StrongList<T>(capacity);
            blocks = new List<DirtyBlock>(1);
        }

        public void SetCapacity(int capacity)
        {
            Assert.IsTrue(blocks.Count == 0, "快内已经有成员,不允许修改");
            this.capacity = capacity;
            datas.SetCapacity(capacity);
        }

        public bool Add(T data)
        {
            int blockIndex = data.ID / capacity;
            int subBlockIndex = data.ID % capacity;
            if (blocks.Count <= blockIndex)
            {
                for (int i = blocks.Count; i <= blockIndex; i++)
                {
                    blocks.Add(new DirtyBlock(capacity));
                }
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
                datas.Remove(data);
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

        public IEnumerator<T> GetEnumerator() => this.datas.GetEnumerator();
    }
}