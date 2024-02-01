using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame.Editor
{
    public interface IEditorEnitiy : IReference
    {
        public int ID { get; set; }
        public IEditorEnitiy Parent { get; set; }
    }

    public abstract class EditorEnitiy : IEditorEnitiy
    {
        private Dictionary<Type, IEditorEnitiy> m_Components = new ();
        private Dictionary<int, IEditorEnitiy> m_Childs = new();
        public int ID { get; set; }
        public IEditorEnitiy Parent { get; set; }
        private int LastID;

        public virtual void Init()
        {
            
        }

        public virtual void Show()
        {
        }

        public virtual void Hide()
        {
            foreach (EditorEnitiy component in m_Components.Values)
            {
                component.Hide();
            }

            foreach (EditorEnitiy child in m_Childs.Values)
            {
                child.Hide();
            }
        }

        public virtual void Clear()
        {
            foreach (IEditorEnitiy component in m_Components.Values)
            {
                ReferencePool.Release(component);
            }

            foreach (IEditorEnitiy child in m_Childs.Values)
            {
                ReferencePool.Release(child);
            }
            m_Components.Clear();
        }

        public T AddComponent<T>() where T : class, IEditorEnitiy, new()
        {
            if (m_Components.ContainsKey(typeof(T)))
            {
                Debug.LogWarning($"组件{typeof(T).Name}已经加入到组");
            }

            T acquireT = ReferencePool.Acquire<T>();
            acquireT.Parent = this;
            acquireT.ID = ++LastID;
            // acquireT.Init();
            m_Components.Add(typeof(T), acquireT);
            return acquireT;
        }

        public T GetComponent<T>() where T : class, IEditorEnitiy, new()
        {
            Type type = typeof(T);
            if (!m_Components.TryGetValue(type, out IEditorEnitiy enitity))
            {
                Debug.LogWarning($"组件{type.Name}不存在");
            }

            return (T) enitity;
        }

        public void RemoveComponent<T>() where T : IEditorEnitiy, new()
        {
            RemoveComponent(typeof(T));
        }

        private void RemoveComponent(Type type)
        {
            if (!m_Components.TryGetValue(type, out IEditorEnitiy enitity))
            {
                Debug.LogWarning($"组件{type.Name}不存在");
                return;
            }
            ReferencePool.Release(enitity);
            m_Components.Remove(type);
        }

        public T AddChild<T>() where T : class, IEditorEnitiy, new()
        {
            T acquireT = ReferencePool.Acquire<T>();
            acquireT.Parent = this;
            acquireT.ID = ++LastID;
            m_Childs.Add(acquireT.ID, acquireT);
            return acquireT;
        }

        public void RemoveChild(int id)
        {
            if (!m_Childs.TryGetValue(id, out IEditorEnitiy enitity))
            {
                Debug.LogWarning($"组件{id}不存在");
                return;
            }
            ReferencePool.Release(enitity);
            m_Childs.Remove(id);
        }
    }
}