using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame
{
    using DDictionaryETS = DDictionary<Entity, Type, SystemObject>;
    using DDictionaryET = Dictionary<Type, SystemObject>;

    public static class SystemAction
    {
        public static void SystemInit(this ISystem system, Entity entity)
        {
            if (system is IIInitSystem initsys)
            {
                initsys.Run(entity);
            }
            else if (system is IECSInitSystem ecsinitsystem)
            {
                Context context = entity as Context;
                if (context == null)
                {
                    throw new Exception($"{entity.GetType()} not has Content");
                }

                ecsinitsystem.Initialize(context);
            }
        }

        public static bool IsUpdateSystem(this ISystem system)
        {
            if (system is IUpdateSystem || system is IECSUpdateSystem)
            {
                return true;
            }
            return false;
        }

        public static void SystemUpdate(this List<EnitityHouse.SystemEnitiy> ets)
        {
            int count = ets.Count;
            for (int i = 0; i < count; i++)
            {
                EnitityHouse.SystemEnitiy es = ets[i];
                if (es.SystemObject.System is IUpdateSystem updatesystem)
                {
                    updatesystem.Run(es.Entity);
                }else if (es.SystemObject.System is IECSUpdateSystem ecsupdatesystem)
                {
                    ecsupdatesystem.Update();
                }
            }
        }

        public static void SystemDestroy(this DDictionaryET system, Entity entity)
        {
            foreach (var vt in system)
            {
                if (vt.Value.System is IDestroySystem dessys)
                {
                    dessys.Run(entity);
                }
            }
        }
    }
}