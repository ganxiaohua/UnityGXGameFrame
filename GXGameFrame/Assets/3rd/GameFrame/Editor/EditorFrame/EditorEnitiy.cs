using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame.Editor
{
    public interface IEditorEntity: IReference
    {
        public int ID { get; set; }
        public IEditorEntity Parent { get; set; }

        public void Init(object obj);

        public void Show();

        public void Hide();
        
    }

    public abstract class EditorEntity : IEditorEntity
    {
        private Dictionary<Type, IEditorEntity> m_Components = new ();
        private Dictionary<int, IEditorEntity> m_Childs = new();
        public int ID { get; set; }
        public IEditorEntity Parent { get; set; }
        private int LastID;

        public virtual void Init(object obj = null)
        {
            
        }

        public virtual void Show()
        {
        }

        public virtual void Hide()
        {
            foreach (EditorEntity component in m_Components.Values)
            {
                component.Hide();
            }

            foreach (EditorEntity child in m_Childs.Values)
            {
                child.Hide();
            }
        }

        public virtual void Clear()
        {
            foreach (IEditorEntity component in m_Components.Values)
            {
                ReferencePool.Release(component);
            }

            foreach (IEditorEntity child in m_Childs.Values)
            {
                ReferencePool.Release(child);
            }
            m_Components.Clear();
        }

        public T AddComponent<T>(object obj) where T : class, IEditorEntity, new()
        {
            if (m_Components.ContainsKey(typeof(T)))
            {
                Debug.LogWarning($"组件{typeof(T).Name}已经加入到组");
            }

            T acquireT = ReferencePool.Acquire<T>();
            acquireT.Parent = this;
            acquireT.ID = ++LastID;
            acquireT.Init(obj);
            m_Components.Add(typeof(T), acquireT);
            return acquireT;
        }

        public T GetComponent<T>() where T : class, IEditorEntity, new()
        {
            Type type = typeof(T);
            if (!m_Components.TryGetValue(type, out IEditorEntity enitity))
            {
                Debug.LogWarning($"组件{type.Name}不存在");
            }

            return (T) enitity;
        }
        

        public void RemoveComponent<T>() where T : IEditorEntity, new()
        {
            RemoveComponent(typeof(T));
        }
        
        public void RemoveComponent(Type type)
        {
            if (!m_Components.TryGetValue(type, out IEditorEntity enitity))
            {
                Debug.LogWarning($"组件{type.Name}不存在");
                return;
            }
            ReferencePool.Release(enitity);
            m_Components.Remove(type);
        }
        
        public T AddChild<T>() where T : class, IEditorEntity, new()
        {
            T acquireT = ReferencePool.Acquire<T>();
            acquireT.Parent = this;
            acquireT.ID = ++LastID;
            m_Childs.Add(acquireT.ID, acquireT);
            return acquireT;
        }

        public void RemoveChild(int id)
        {
            if (!m_Childs.TryGetValue(id, out IEditorEntity enitity))
            {
                Debug.LogWarning($"组件{id}不存在");
                return;
            }
            ReferencePool.Release(enitity);
            m_Childs.Remove(id);
        }
    }
}