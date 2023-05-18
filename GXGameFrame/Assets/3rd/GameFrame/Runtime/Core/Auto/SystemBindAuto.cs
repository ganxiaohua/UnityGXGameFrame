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
        m_SystemBind.Add(typeof(BattleGroudScene),typeof(IStartSystem),typeof(GXGame.BattleGroudSceneSystem.BattleGroudSceneStartSystem));
        m_SystemBind.Add(typeof(BattleGroudScene),typeof(IShowSystem),typeof(GXGame.BattleGroudSceneSystem.BattleGroudSceneShowSystem));
        m_SystemBind.Add(typeof(BattleGroudScene),typeof(IUpdateSystem),typeof(GXGame.BattleGroudSceneSystem.BattleGroudSceneUpdateSystem));
        m_SystemBind.Add(typeof(UICardListWindow),typeof(IStartSystem),typeof(GXGame.UICardListWindowSystem.UICardListWindowStartSystem));
        m_SystemBind.Add(typeof(UICardListWindow),typeof(IShowSystem),typeof(GXGame.UICardListWindowSystem.UICardListWindowShowSystem));
        m_SystemBind.Add(typeof(UICardListWindow),typeof(IHideSystem),typeof(GXGame.UICardListWindowSystem.UICardListWindowHideSystem));
        m_SystemBind.Add(typeof(UICardListWindow),typeof(IUpdateSystem),typeof(GXGame.UICardListWindowSystem.UICardListWindowUpdateSystem));
        m_SystemBind.Add(typeof(UICardListWindow),typeof(IClearSystem),typeof(GXGame.UICardListWindowSystem.UICardListWindowClearSystem));
        m_SystemBind.Add(typeof(UICardWindow),typeof(IStartSystem),typeof(GXGame.UICardWindowSystem.UICardWindowStartSystem));
        m_SystemBind.Add(typeof(UICardWindow),typeof(IShowSystem),typeof(GXGame.UICardWindowSystem.UICardWindowShowSystem));
        m_SystemBind.Add(typeof(UICardWindow),typeof(IHideSystem),typeof(GXGame.UICardWindowSystem.UICardWindowHideSystem));
        m_SystemBind.Add(typeof(UICardWindow),typeof(IUpdateSystem),typeof(GXGame.UICardWindowSystem.UICardWindowUpdateSystem));
        m_SystemBind.Add(typeof(UICardWindow),typeof(IClearSystem),typeof(GXGame.UICardWindowSystem.UICardWindowClearSystem));
        m_SystemBind.Add(typeof(UIHomeMainPanel),typeof(IStartSystem),typeof(GXGame.UIHomeMainPanelSystem.UIHomeMainPanelStartSystem));
        m_SystemBind.Add(typeof(UIHomeMainPanel),typeof(IShowSystem),typeof(GXGame.UIHomeMainPanelSystem.UIHomeMainPanelShowSystem));
        m_SystemBind.Add(typeof(UIHomeMainPanel),typeof(IHideSystem),typeof(GXGame.UIHomeMainPanelSystem.UIHomeMainPanelHideSystem));
        m_SystemBind.Add(typeof(UIHomeMainPanel),typeof(IUpdateSystem),typeof(GXGame.UIHomeMainPanelSystem.UIHomeMainPanelUpdateSystem));
        m_SystemBind.Add(typeof(UICardChoiceWindowData),typeof(IStartSystem),typeof(GXGame.UICardChoiceWindowDataSystem.UICardChoiceWindowDataStartSystem));
        m_SystemBind.Add(typeof(UICardChoiceWindowData),typeof(IClearSystem),typeof(GXGame.UICardChoiceWindowDataSystem.UICardChoiceWindowDataClearSystem));
        m_SystemBind.Add(typeof(UICardChoiceWindow),typeof(IStartSystem),typeof(GXGame.UICardChoiceWindowSystem.UICardChoiceWindowStartSystem));
        m_SystemBind.Add(typeof(UICardChoiceWindow),typeof(IShowSystem),typeof(GXGame.UICardChoiceWindowSystem.UICardChoiceWindowShowSystem));
        m_SystemBind.Add(typeof(UICardChoiceWindow),typeof(IHideSystem),typeof(GXGame.UICardChoiceWindowSystem.UICardChoiceWindowHideSystem));
        m_SystemBind.Add(typeof(UICardChoiceWindow),typeof(IUpdateSystem),typeof(GXGame.UICardChoiceWindowSystem.UICardChoiceWindowUpdateSystem));
        m_SystemBind.Add(typeof(UICardChoiceWindow),typeof(IClearSystem),typeof(GXGame.UICardChoiceWindowSystem.UICardChoiceWindowClearSystem));
        m_SystemBind.Add(typeof(AssetInitComponent),typeof(IStartSystem),typeof(GameFrame.AssetInitComponentSystem.AssetInitComponentStartSystem));
        m_SystemBind.Add(typeof(AssetInitComponent),typeof(IClearSystem),typeof(GameFrame.AssetInitComponentSystem.AssetInitComponentClearSystem));
        m_SystemBind.Add(typeof(MainScene),typeof(IStartSystem<Type>),typeof(GameFrame.SceneEntitySystem.SceneEntityStartSystem));
        m_SystemBind.Add(typeof(MainScene),typeof(IUpdateSystem),typeof(GameFrame.SceneEntitySystem.SceneEntityUpdateSystem));
        m_SystemBind.Add(typeof(MainScene),typeof(IClearSystem),typeof(GameFrame.SceneEntitySystem.SceneEntityClearSystem));
        m_SystemBind.Add(typeof(WaitComponent),typeof(IStartSystem<Type>),typeof(GameFrame.WaitComponentSystem.WaitComponentStartSystem));
        m_SystemBind.Add(typeof(WaitComponent),typeof(IClearSystem),typeof(GameFrame.WaitComponentSystem.WaitComponentClearSystem));
        m_SystemBind.Add(typeof(DependentResources),typeof(IStartSystem<String>),typeof(GameFrame.DependentResourcesSystem.DependentResourcesStartSystem));
        m_SystemBind.Add(typeof(DependentResources),typeof(IClearSystem),typeof(GameFrame.DependentResourcesSystem.DependentResourcesClearSystem));
        m_SystemBind.Add(typeof(DependentUIResources),typeof(IStartSystem<List<String>>),typeof(GameFrame.DependentUIResourcesSystem.DependentUIResourcesStartSystem));
        m_SystemBind.Add(typeof(DependentUIResources),typeof(IClearSystem),typeof(GameFrame.DependentUIResourcesSystem.DependentUIResourcesClearSystem));
        m_SystemBind.Add(typeof(GameObjectPoolComponent),typeof(IStartSystem),typeof(GameFrame.GameObjectPoolComponentSystem.GameObjectPoolComponentStartSystem));
        m_SystemBind.Add(typeof(GameObjectPoolComponent),typeof(IClearSystem),typeof(GameFrame.GameObjectPoolComponentSystem.GameObjectPoolComponentClearSystem));
    }
}

