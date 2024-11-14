#if UNITY_EDITOR
using UnityEngine.Profiling;
#endif

namespace GameFrame
{
    public static class SystemAction
    {
        public static bool SystemInitialize(this ISystem system)
        {
            if (system is IInitializeSystem ecsinitsystem)
            {
                ecsinitsystem.Initialize();
                return true;
            }

            return false;
        }

        public static bool SystemInitialize<P1>(this ISystem system, P1 p1)
        {
            if (system is IInitializeSystem<P1> ecsinitsystem)
            {
                ecsinitsystem.Initialize(p1);
                return true;
            }

            return false;
        }

        public static bool SystemInitialize<P1, P2>(this ISystem system, P1 p1, P2 p2)
        {
            if (system is IInitializeSystem<P1, P2> ecsinitsystem)
            {
                ecsinitsystem.Initialize(p1, p2);
                return true;
            }

            return false;
        }


        public static bool SystemPreShow(this ISystem system, bool p1)
        {
            if (system is IPreShowSystem showsystem)
            {
                showsystem.OnPreShow(p1);
                return true;
            }

            return false;
        }


        public static bool SystemShow(this ISystem system)
        {
            if (system is IShowSystem showsystem)
            {
                showsystem.OnShow();
                return true;
            }

            return false;
        }


        public static bool SystemHide(this ISystem system)
        {
            if (system is IHideSystem showsystem)
            {
                showsystem.OnHide();
                return true;
            }

            return false;
        }


        public static UpdateType IsUpdateSystem(this ISystem system)
        {
            if (system is IUpdateSystem)
            {
                return UpdateType.Update;
            }
            else if (system is ILateUpdateSystem)
            {
                return UpdateType.LateUpdate;
            }
            else if (system is IFixedUpdateSystem)
            {
                return UpdateType.FixedUpdate;
            }

            return UpdateType.Node;
        }


        public static void SystemUpdate(this StrongList<SystemEntity> ets, float elapseSeconds, float realElapseSeconds)
        {
            foreach (var es in ets)
            {
#if UNITY_EDITOR
                Profiler.BeginSample(es.SystemObject.System.GetType().Name);
#endif
                ((IUpdateSystem) es.SystemObject.System).OnUpdate(elapseSeconds, realElapseSeconds);
#if UNITY_EDITOR
                Profiler.EndSample();
#endif
            }
        }

        public static void SystemLateUpdate(this StrongList<SystemEntity> ets, float elapseSeconds, float realElapseSeconds)
        {
            foreach (var es in ets)
            {
#if UNITY_EDITOR
                Profiler.BeginSample(es.SystemObject.System.GetType().Name);
#endif
                ((ILateUpdateSystem) es.SystemObject.System).LateUpdate(elapseSeconds, realElapseSeconds);
#if UNITY_EDITOR
                Profiler.EndSample();
#endif
            }
        }

        public static void SystemFixedUpdate(this StrongList<SystemEntity> ets, float elapseSeconds, float realElapseSeconds)
        {
            foreach (var es in ets)
            {
#if UNITY_EDITOR
                Profiler.BeginSample(es.SystemObject.System.GetType().Name);
#endif
                ((IFixedUpdateSystem) es.SystemObject.System).FixedUpdate(elapseSeconds, realElapseSeconds);
#if UNITY_EDITOR
                Profiler.EndSample();
#endif
            }
        }
    }
}