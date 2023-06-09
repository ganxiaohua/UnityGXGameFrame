using System.Collections.Generic;
using System;
using GameFrame;
using GXGame;
public partial class AutoBindSystem
{
    public void AddSystem()
    {
        

        m_SystemBind.Add(typeof(CubeScene),typeof(IStartSystem),typeof(GXGame.CubeSceneSystem.CubeSceneStartSystem));
        m_SystemBind.Add(typeof(CubeScene),typeof(IShowSystem),typeof(GXGame.CubeSceneSystem.CubeSceneShowSystem));
        m_SystemBind.Add(typeof(CubeScene),typeof(IHideSystem),typeof(GXGame.CubeSceneSystem.CubeSceneHideSystem));
        m_SystemBind.Add(typeof(CubeScene),typeof(IUpdateSystem),typeof(GXGame.CubeSceneSystem.CubeSceneUpdateSystem));
        m_SystemBind.Add(typeof(CubeScene),typeof(IClearSystem),typeof(GXGame.CubeSceneSystem.CubeSceneClearSystem));
        m_SystemBind.Add(typeof(UICardListWindowData),typeof(IStartSystem),typeof(GXGame.UICardListWindowDataSystem.UICardListWindowDataStartSystem));
        m_SystemBind.Add(typeof(UICardListWindowData),typeof(IUpdateSystem),typeof(GXGame.UICardListWindowDataSystem.UICardListWindowDataUpdateSystem));
        m_SystemBind.Add(typeof(UICardListWindow),typeof(IStartSystem),typeof(GXGame.UICardListWindowSystem.UICardListWindowStartSystem));
        m_SystemBind.Add(typeof(UICardListWindow),typeof(IPreShowSystem),typeof(GXGame.UICardListWindowSystem.UICardListWindowPreShowSystem));
        m_SystemBind.Add(typeof(UICardListWindow),typeof(IShowSystem),typeof(GXGame.UICardListWindowSystem.UICardListWindowShowSystem));
        m_SystemBind.Add(typeof(UICardListWindow),typeof(IHideSystem),typeof(GXGame.UICardListWindowSystem.UICardListWindowHideSystem));
        m_SystemBind.Add(typeof(UICardListWindow),typeof(IUpdateSystem),typeof(GXGame.UICardListWindowSystem.UICardListWindowUpdateSystem));
        m_SystemBind.Add(typeof(UICardListWindow),typeof(IClearSystem),typeof(GXGame.UICardListWindowSystem.UICardListWindowClearSystem));
        m_SystemBind.Add(typeof(UICardListWindow2Data),typeof(IStartSystem),typeof(GXGame.UICardListWindow2DataSystem.UICardListWindow2DataStartSystem));
        m_SystemBind.Add(typeof(UICardListWindow2Data),typeof(IUpdateSystem),typeof(GXGame.UICardListWindow2DataSystem.UICardListWindow2DataUpdateSystem));
        m_SystemBind.Add(typeof(UICardListWindow2),typeof(IStartSystem),typeof(GXGame.UICardListWindow2System.UICardListWindow2StartSystem));
        m_SystemBind.Add(typeof(UICardListWindow2),typeof(IPreShowSystem),typeof(GXGame.UICardListWindow2System.UICardListWindow2PreShowSystem));
        m_SystemBind.Add(typeof(UICardListWindow2),typeof(IShowSystem),typeof(GXGame.UICardListWindow2System.UICardListWindow2ShowSystem));
        m_SystemBind.Add(typeof(UICardListWindow2),typeof(IHideSystem),typeof(GXGame.UICardListWindow2System.UICardListWindow2HideSystem));
        m_SystemBind.Add(typeof(UICardListWindow2),typeof(IUpdateSystem),typeof(GXGame.UICardListWindow2System.UICardListWindow2UpdateSystem));
        m_SystemBind.Add(typeof(UICardListWindow2),typeof(IClearSystem),typeof(GXGame.UICardListWindow2System.UICardListWindow2ClearSystem));
        m_SystemBind.Add(typeof(AssetInitComponent),typeof(IStartSystem),typeof(GameFrame.AssetInitComponentSystem.AssetInitComponentStartSystem));
        m_SystemBind.Add(typeof(AssetInitComponent),typeof(IUpdateSystem),typeof(GameFrame.AssetInitComponentSystem.AssetInitComponentUpdateSystem));
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

