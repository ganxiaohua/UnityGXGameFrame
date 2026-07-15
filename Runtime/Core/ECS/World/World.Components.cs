using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace GameFrame.Runtime
{
    public unsafe partial class World
    {
        //TODO: Cut into groups and chunks …… be like unity ecs?
        private void** components;
        private int* structSizes;
        private int* structAlign;
        private int componentsCapacity;
        private int componentsChildrenSize;

        private void InitComponents(int childCount)
        {
            if (components != null)
                throw new System.InvalidOperationException("Components have already been initialized.");
            if (MaxComponentCount <= 0)
                throw new System.ArgumentOutOfRangeException(nameof(MaxComponentCount), MaxComponentCount,
                    "Max component count must be greater than zero.");

            componentsCapacity = MaxComponentCount;
            long componentsSize = sizeof(void*) * (long) componentsCapacity;
            long structMetadataSize = sizeof(int) * (long) componentsCapacity;
#if !Tracked
            components = (void**) UnsafeUtility.Malloc(componentsSize, UnsafeUtility.AlignOf<System.IntPtr>(), Allocator.Persistent);
            structSizes = (int*) UnsafeUtility.Malloc(structMetadataSize, UnsafeUtility.AlignOf<int>(), Allocator.Persistent);
            structAlign = (int*) UnsafeUtility.Malloc(structMetadataSize, UnsafeUtility.AlignOf<int>(), Allocator.Persistent);
#else
            components = (void**) UnsafeUtility.MallocTracked(componentsSize, UnsafeUtility.AlignOf<System.IntPtr>(), Allocator.Persistent,
                ConstTrackEdId.Components);
            structSizes = (int*) UnsafeUtility.MallocTracked(structMetadataSize, UnsafeUtility.AlignOf<int>(), Allocator.Persistent,
                ConstTrackEdId.Components);
            structAlign = (int*) UnsafeUtility.MallocTracked(structMetadataSize, UnsafeUtility.AlignOf<int>(), Allocator.Persistent,
                ConstTrackEdId.Components);
#endif
            UnsafeUtility.MemClear(components, componentsSize);
            UnsafeUtility.MemClear(structSizes, structMetadataSize);
            UnsafeUtility.MemClear(structAlign, structMetadataSize);
            componentsChildrenSize = childCount > 0 ? childCount : 1;
#if UNITY_EDITOR
            InitCompSize();
#endif
#if Tracked
            UnsafeUtility.SetLeakDetectionMode(NativeLeakDetectionMode.EnabledWithStackTrace);
#endif
        }

        public void AddComp<T>() where T : unmanaged, EffComponent
        {
            var cid = ComponentsID<T>.TID;
            ValidateComponentId(cid);
            if (components[cid] != null)
                return;
            int structSize = UnsafeUtility.SizeOf<T>();
            int alignment = UnsafeUtility.AlignOf<T>();
            structSizes[cid] = structSize;
            structAlign[cid] = alignment;
            long size = (long) structSize * componentsChildrenSize;
#if !Tracked
            void* ptr = UnsafeUtility.Malloc(size, alignment, Allocator.Persistent);
#else
            void* ptr = UnsafeUtility.MallocTracked(size, alignment, Allocator.Persistent, ConstTrackEdId.Components);
#endif
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
            int count = componentsCapacity;
#if UNITY_EDITOR
            InitCompSize();
#endif
            for (int i = 0; i < count; i++)
            {
                var structsize = structSizes[i];
                long oldSize = (long) structsize * componentsChildrenSize;
                long size = (long) structsize * Children.AllCount;
                long appendSize = size - oldSize;
                var compPtr = components[i];
                if (compPtr == null)
                    continue;
#if !Tracked
                void* ptr = UnsafeUtility.Malloc(size, structAlign[i], Allocator.Persistent);
                UnsafeUtility.MemCpy(ptr, compPtr, oldSize);
                UnsafeUtility.MemClear((byte*) ptr + oldSize, appendSize);
                UnsafeUtility.Free(compPtr, Allocator.Persistent);
#else
                void* ptr = UnsafeUtility.MallocTracked(size, structAlign[i], Allocator.Persistent, ConstTrackEdId.Components);
                UnsafeUtility.MemCpy(ptr, compPtr, oldSize);
                UnsafeUtility.MemClear((byte*)ptr + oldSize, appendSize);
                UnsafeUtility.FreeTracked(compPtr, Allocator.Persistent);
#endif
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


        public ref T GetComp<T>(int entityIndex, int id) where T : unmanaged, EffComponent
        {
            ValidateComponentId(id);
            T* ptr = (T*) components[id];
            return ref ptr[entityIndex];
        }

        public T* GetCompPtr<T>(int entityIndex, int id) where T : unmanaged, EffComponent
        {
            ValidateComponentId(id);
            T* ptr = (T*) components[id];
            ptr += entityIndex;
            return ptr;
        }

        public byte* GetCompBytes(int entityIndex, int id)
        {
            ValidateComponentId(id);
            var ptr = (byte*) components[id];
            ptr += structSizes[id] * entityIndex;
            return ptr;
        }

        public unsafe T* GetComponentColumnPtr<T>() where T : unmanaged, EffComponent
        {
            int cid = ComponentsID<T>.TID;
            ValidateComponentId(cid);
            return (T*) components[cid];
        }

        public void ClearComp(int entityIndex, int id)
        {
            var ptr = GetCompBytes(entityIndex, id);
            UnsafeUtility.MemClear(ptr, structSizes[id]);
        }

        public void ClearEntityAllComponent(int entityIndex)
        {
            var componentsList = Children[entityIndex].ComponentsList;
            for (int i = 0; i < componentsList.Count; i++)
            {
                int cid = componentsList[i];
                ValidateComponentId(cid);
                var ptr = (byte*) components[cid];
                ptr += structSizes[cid] * entityIndex;
                UnsafeUtility.MemClear(ptr, structSizes[cid]);
            }
        }

        private void DestroyComp()
        {
            if (components == null)
                return;
            for (int i = 0; i < componentsCapacity; i++)
            {
                var item = components[i];
                if (item != null)
#if !Tracked
                    UnsafeUtility.Free(item, Allocator.Persistent);
#else
                    UnsafeUtility.FreeTracked(item, Allocator.Persistent);
#endif
            }

#if !Tracked
            UnsafeUtility.Free(components, Allocator.Persistent);
            UnsafeUtility.Free(structSizes, Allocator.Persistent);
            UnsafeUtility.Free(structAlign, Allocator.Persistent);
#else
            UnsafeUtility.FreeTracked(components, Allocator.Persistent);
            UnsafeUtility.FreeTracked(structSizes, Allocator.Persistent);
            UnsafeUtility.FreeTracked(structAlign, Allocator.Persistent);
#endif
            components = null;
            structSizes = null;
            structAlign = null;
            componentsCapacity = 0;
            componentsChildrenSize = 0;
        }

        [System.Diagnostics.Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private void ValidateComponentId(int id)
        {
            if (components == null)
                throw new System.InvalidOperationException("Components have not been initialized.");
            if ((uint) id >= (uint) componentsCapacity)
                throw new System.IndexOutOfRangeException($"Component id {id} is outside the allocated range [0, {componentsCapacity}).");
        }
    }
}
