using System.Collections.Generic;
using System;
using GameFrame;
public partial class AutoBindSystem
{
    public void AddSystem()
    {
        
        m_SystemBind.Add(typeof(Entity1),typeof(IStartSystem<Int32>),typeof(Entity1System.Entity1StartSystem));
        m_SystemBind.Add(typeof(Entity1),typeof(IUpdateSystem),typeof(Entity1System.Entity1UpdateSystem));
        m_SystemBind.Add(typeof(Entity1),typeof(IClearSystem),typeof(Entity1System.Entity1ClearSystem));
        m_SystemBind.Add(typeof(CreateComponent),typeof(IStartSystem<Int32>),typeof(CreateComponentSystem.CreateComponentStartSystem));
        m_SystemBind.Add(typeof(CreateComponent),typeof(IClearSystem),typeof(CreateComponentSystem.CreateComponentClearSystem));
        // m_SystemBind.Add(typeof(UIHome),typeof(IStartSystem),typeof(UIWindowSystem.UIHomeStartSystem));
        // m_SystemBind.Add(typeof(UIHome),typeof(IShowSystem),typeof(UIWindowSystem.UIHomeSystem));
        // m_SystemBind.Add(typeof(UIHome),typeof(IHideSystem),typeof(UIWindowSystem.UIHomeHideSystem));
        // m_SystemBind.Add(typeof(UIHome),typeof(IUpdateSystem),typeof(UIWindowSystem.UIHomeUpdateSystem));
        // m_SystemBind.Add(typeof(UIHome),typeof(IClearSystem),typeof(UIWindowSystem.UIHomeClearSystem));
        m_SystemBind.Add(typeof(SceneEntity),typeof(IStartSystem<Type>),typeof(GameFrame.SceneEntitySystem.SceneEntityStartSystem));
        m_SystemBind.Add(typeof(SceneEntity),typeof(IUpdateSystem),typeof(GameFrame.SceneEntitySystem.SceneEntityUpdateSystem));
        m_SystemBind.Add(typeof(SceneEntity),typeof(IClearSystem),typeof(GameFrame.SceneEntitySystem.SceneEntityClearSystem));
        m_SystemBind.Add(typeof(DependentResources),typeof(IStartSystem<List<String>>),typeof(GameFrame.DependentResourcesSystem.DependentResourcesStartSystem));
        m_SystemBind.Add(typeof(DependentResources),typeof(IClearSystem),typeof(GameFrame.DependentResourcesSystem.DependentResourcesClearSystem));
    }
}

