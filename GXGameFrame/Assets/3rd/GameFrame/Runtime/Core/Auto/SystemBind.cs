using System;
using System.Collections.Generic;
using GameFrame;

public  class BindSystem : Singleton<BindSystem>
{
    private DDoubleMap<Type, Type, Type> m_SystemBind = new();

    public DDoubleMap<Type, Type, Type> SystemBind => m_SystemBind;


    public List<Type> GetEnitityAllSystem(Type entityType)
    {
        return m_SystemBind.GetAllV(entityType);
    }

    public Type GetEnitiySystem(Type entityType, Type Isystem)
    {
        return m_SystemBind.GetVValue(entityType, Isystem);
    }

    public Type GetIsystem(Type entityType, Type entitySystemType)
    {
        return m_SystemBind.GetVKey(entityType, entitySystemType);
    }

    public Type GetStartSystemParamP1<P1>(Type entityType)
    {
        Type systemType = typeof(IStartSystem<P1>);
        Type entitysystemtype = GetEnitiySystem(entityType, systemType);
        if (GetEnitiySystem(entitysystemtype, systemType) == default(Type))
        {
            throw new Exception($"not have IStartSystem<P1>");
        }

        return entitysystemtype;
    }

    public Type GetStartSystemParamP2<P1, P2>(Type entityType)
    {
        Type systemType = typeof(IStartSystem<P1, P2>);
        Type entitysystemtype = GetEnitiySystem(entityType, systemType);
        if (GetEnitiySystem(entitysystemtype, systemType) == default(Type))
        {
            throw new Exception($"not have IStartSystem<P1>");
        }

        return entitysystemtype;
    }
}