using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace GameFrame.Runtime
{
    public unsafe partial class World
    {
        private void*[] components;
        private int[] structSizes;
        private int[] structAlign;
        private int componentsChildrenSize;


        private void InitComponents(int childCount)
        {
            components = new void*[MaxComponentCount];
            structSizes = new int[MaxComponentCount];
            structAlign = new int[MaxComponentCount];
            componentsChildrenSize = childCount;
#if UNITY_EDITOR
            InitCompSize();
#endif
        }

        public void AddComp<T>() where T : unmanaged, EffComponent
        {
            var cid = ComponentsID<T>.TID;
            if (components[cid] != null)
                return;
            int structSize = UnsafeUtility.SizeOf<T>();
            int alignment = UnsafeUtility.AlignOf<T>();
            structSizes[cid] = structSize;
            structAlign[cid] = alignment;
            long size = structSize * componentsChildrenSize;
            void* ptr = UnsafeUtility.Malloc(size, alignment, Allocator.Persistent);
            UnsafeUtility.MemClear(ptr, size);
            components[cid] = ptr;
#if UNITY_EDITOR
            CalculateSize(size);
#endif
        }

        private void Expansion()
        {
            if (ChildsCount <= componentsChildrenSize)
                return;
            int count = components.Length;
#if UNITY_EDITOR
            InitCompSize();
#endif
            for (int i = 0; i < count; i++)
            {
                var structsize = structSizes[i];
                long size = structsize * Children.AllCount;
                void* ptr = UnsafeUtility.Malloc(size, structAlign[i], Allocator.Persistent);
                UnsafeUtility.MemClear(ptr, size);
                var compPtr = components[i];
                UnsafeUtility.MemCpy(ptr, compPtr, structsize * componentsChildrenSize);
                UnsafeUtility.Free(compPtr, Allocator.Persistent);
                components[i] = ptr;
#if UNITY_EDITOR
                CalculateSize(size);
#endif
            }
#if UNITY_EDITOR
            OutputSize();
#endif
            componentsChildrenSize = Children.AllCount;
        }

        public T GetComp<T>(int entityIndex, int id) where T : unmanaged, EffComponent
        {
            T* ptr = (T*) components[id];
            return ptr[entityIndex];
        }

        public T* GetCompPtr<T>(int entityIndex, int id) where T : unmanaged, EffComponent
        {
            T* ptr = (T*) components[id];
            ptr += entityIndex;
            return ptr;
        }

        public void ClearComp(int entityIndex, int id)
        {
            var ptr = (byte*) components[id];
            ptr += structSizes[id] * entityIndex;
            UnsafeUtility.MemClear(ptr, structSizes[id]);
        }

        public void ClearEntityAllComponent(int entityIndex)
        {
            int count = components.Length;
            for (int i = 0; i < count; i++)
            {
                var ptr = (byte*) components[i];
                ptr += structSizes[i] * entityIndex;
                UnsafeUtility.MemClear(ptr, structSizes[i]);
            }
        }

        private void DestroyComp()
        {
            if (components == null)
                return;
            foreach (var item in components)
            {
                if (item != null)
                    UnsafeUtility.FreeTracked(item, Allocator.Persistent);
            }

            components = null;
            structSizes = null;
            structAlign = null;
        }
    }
}