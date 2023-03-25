using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame
{
    using DDictionaryETS = DDictionary<IEntity, Type, SystemObject>;
    using DDictionaryET = Dictionary<Type, SystemObject>;

    public static class SystemAction
    {
        public static bool SystemInit(this ISystem system, IEntity entity)
        {
            if (system is IStartSystem initsys)
            {
                initsys.Run(entity);
                return true;
            }
            else if (system is IECSStartSystem ecsinitsystem)
            {
                if (entity is not Context context)
                {
                    throw new Exception($"{entity.GetType()} not has Content");
                }

                ecsinitsystem.Start(context);
                return true;
            }

            return false;
        }

        public static bool IsUpdateSystem(this ISystem system)
        {
            if (system is IUpdateSystem or IECSUpdateSystem)
            {
                return true;
            }

            return false;
        }

        public static void SystemUpdate(this List<SystemEnitiy> ets, float elapseSeconds, float realElapseSeconds)
        {
            int count = ets.Count;
            for (int i = 0; i < count; i++)
            {
                SystemEnitiy es = ets[i];
                if (es.SystemObject.System is IUpdateSystem updatesystem)
                {
                    updatesystem.Run(es.Entity,elapseSeconds,realElapseSeconds);
                }
                else if (es.SystemObject.System is IECSUpdateSystem ecsupdatesystem)
                {
                    ecsupdatesystem.Update();
                }
            }
        }

        public static void SystemDestroy(this DDictionaryET system, IEntity entity)
        {
            foreach (var vt in system)
            {
                if (vt.Value.System is IClearSystem dessys)
                {
                    dessys.Run(entity);
                }
            }
        }
    }
}