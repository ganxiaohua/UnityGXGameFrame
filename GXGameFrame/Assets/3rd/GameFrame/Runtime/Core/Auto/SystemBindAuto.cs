using System.Collections.Generic;
using System;
using GameFrame;
using GXGame;

public partial class AutoBindSystem
{
    public void AddSystem()
    {
        
        m_SystemBind.Add(typeof(Entity1),typeof(IStartSystem<Int32>),typeof(Entity1System.Entity1StartSystem));
        m_SystemBind.Add(typeof(Entity1),typeof(IUpdateSystem),typeof(Entity1System.Entity1UpdateSystem));
        m_SystemBind.Add(typeof(Entity1),typeof(IClearSystem),typeof(Entity1System.Entity1ClearSystem));
        m_SystemBind.Add(typeof(CreateComponent),typeof(IStartSystem<Int32>),typeof(CreateComponentSystem.CreateComponentStartSystem));
        m_SystemBind.Add(typeof(CreateComponent),typeof(IClearSystem),typeof(CreateComponentSystem.CreateComponentClearSystem));
        m_SystemBind.Add(typeof(UIHomeMainPanel),typeof(IStartSystem),typeof(GXGame.UIHomeMainPanelSystem.UIHomeMainPanelStartSystem));
        m_SystemBind.Add(typeof(UIHomeMainPanel),typeof(IShowSystem),typeof(GXGame.UIHomeMainPanelSystem.UIHomeMainPanelShowSystem));
        m_SystemBind.Add(typeof(UIHomeMainPanel),typeof(IHideSystem),typeof(GXGame.UIHomeMainPanelSystem.UIHomeMainPanelHideSystem));
        m_SystemBind.Add(typeof(UIHomeMainPanel),typeof(IUpdateSystem),typeof(GXGame.UIHomeMainPanelSystem.UIHomeMainPanelUpdateSystem));
        m_SystemBind.Add(typeof(SceneEntity),typeof(IStartSystem<Type>),typeof(GameFrame.SceneEntitySystem.SceneEntityStartSystem));
        m_SystemBind.Add(typeof(SceneEntity),typeof(IUpdateSystem),typeof(GameFrame.SceneEntitySystem.SceneEntityUpdateSystem));
        m_SystemBind.Add(typeof(SceneEntity),typeof(IClearSystem),typeof(GameFrame.SceneEntitySystem.SceneEntityClearSystem));
        m_SystemBind.Add(typeof(DependentResources),typeof(IStartSystem<List<String>>),typeof(GameFrame.DependentResourcesSystem.DependentResourcesStartSystem));
        m_SystemBind.Add(typeof(DependentResources),typeof(IClearSystem),typeof(GameFrame.DependentResourcesSystem.DependentResourcesClearSystem));
    }
}

