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
        m_SystemBind.Add(typeof(CubeScene),typeof(IStartSystem),typeof(GXGame.CubeSceneSystem.CubeSceneStartSystem));
        m_SystemBind.Add(typeof(CubeScene),typeof(IShowSystem),typeof(GXGame.CubeSceneSystem.CubeSceneShowSystem));
        m_SystemBind.Add(typeof(CubeScene),typeof(IHideSystem),typeof(GXGame.CubeSceneSystem.CubeSceneHideSystem));
        m_SystemBind.Add(typeof(CubeScene),typeof(IUpdateSystem),typeof(GXGame.CubeSceneSystem.CubeSceneUpdateSystem));
        m_SystemBind.Add(typeof(CubeScene),typeof(IClearSystem),typeof(GXGame.CubeSceneSystem.CubeSceneClearSystem));
        m_SystemBind.Add(typeof(BattleGroudScene),typeof(IStartSystem),typeof(GXGame.BattleGroudSceneSystem.BattleGroudSceneStartSystem));
        m_SystemBind.Add(typeof(BattleGroudScene),typeof(IShowSystem),typeof(GXGame.BattleGroudSceneSystem.BattleGroudSceneShowSystem));
        m_SystemBind.Add(typeof(BattleGroudScene),typeof(IUpdateSystem),typeof(GXGame.BattleGroudSceneSystem.BattleGroudSceneUpdateSystem));
        m_SystemBind.Add(typeof(UIHomeMainPanelData),typeof(IStartSystem),typeof(GXGame.UIHomeMainPanelDataSystem.UIHomeMainPanelDataStartSystem));
        m_SystemBind.Add(typeof(UIHomeMainPanelData),typeof(IUpdateSystem),typeof(GXGame.UIHomeMainPanelDataSystem.UIHomeMainPanelDataUpdateSystem));
        m_SystemBind.Add(typeof(UIHomeMainPanel),typeof(IStartSystem),typeof(GXGame.UIHomeMainPanelSystem.UIHomeMainPanelStartSystem));
        m_SystemBind.Add(typeof(UIHomeMainPanel),typeof(IPreShowSystem),typeof(GXGame.UIHomeMainPanelSystem.UIHomeMainPanelPreShowSystem));
        m_SystemBind.Add(typeof(UIHomeMainPanel),typeof(IShowSystem),typeof(GXGame.UIHomeMainPanelSystem.UIHomeMainPanelShowSystem));
        m_SystemBind.Add(typeof(UIHomeMainPanel),typeof(IHideSystem),typeof(GXGame.UIHomeMainPanelSystem.UIHomeMainPanelHideSystem));
        m_SystemBind.Add(typeof(UIHomeMainPanel),typeof(IUpdateSystem),typeof(GXGame.UIHomeMainPanelSystem.UIHomeMainPanelUpdateSystem));
        m_SystemBind.Add(typeof(UIHomeMainPanel),typeof(IClearSystem),typeof(GXGame.UIHomeMainPanelSystem.UIHomeMainPanelClearSystem));
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

