namespace GameFrame.Runtime
{
    public class DirtyBlock
    {
        private bool[] Alignment;
        private int size;
        public int pointer { get; private set; }

        public DirtyBlock(int size)
        {
            this.size = size;
            pointer = 0;
            Alignment = new bool[size];
        }

        public bool Dirty(int index)
        {
            if (index > pointer)
                pointer = index;
            Assert.IsTrue(index < size, "超出快的大小");
            if (Alignment[index])
                return false;
            Alignment[index] = true;
            return true;
        }

        public bool Erase(int index)
        {
            Assert.IsTrue(index < size, "超出快的大小");
            if (!Alignment[index])
                return false;
            Alignment[index] = false;
            return true;
        }

        public void Clear()
        {
            pointer = 0;
            int count = Alignment.Length;
            for (int i = 0; i < count; i++)
            {
                Alignment[i] = false;
            }
        }
    }
}