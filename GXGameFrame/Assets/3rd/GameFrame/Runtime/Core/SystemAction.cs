﻿#if UNITY_EDITOR
using UnityEngine.Profiling;
#endif

namespace GameFrame
{
    public static class SystemAction
    {
        public static bool SystemStart(this ISystem system)
        {
            if (system is IStartSystem ecsinitsystem)
            {
                ecsinitsystem.Start();
                return true;
            }

            return false;
        }

        public static bool SystemStart<P1>(this ISystem system, P1 p1)
        {
            if (system is IStartSystem<P1> ecsinitsystem)
            {
                ecsinitsystem.Start(p1);
                return true;
            }

            return false;
        }

        public static bool SystemStart<P1, P2>(this ISystem system, P1 p1, P2 p2)
        {
            if (system is IStartSystem<P1, P2> ecsinitsystem)
            {
                ecsinitsystem.Start(p1, p2);
                return true;
            }

            return false;
        }


        public static bool SystemPreShow(this ISystem system, bool p1)
        {
            if (system is IPreShowSystem showsystem)
            {
                showsystem.PreShow(p1);
                return true;
            }

            return false;
        }


        public static bool SystemShow(this ISystem system)
        {
            if (system is IShowSystem showsystem)
            {
                showsystem.Show();
                return true;
            }

            return false;
        }


        public static bool SystemHide(this ISystem system)
        {
            if (system is IHideSystem showsystem)
            {
                showsystem.Hide();
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
                ((IUpdateSystem) es.SystemObject.System).Update(elapseSeconds, realElapseSeconds);
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