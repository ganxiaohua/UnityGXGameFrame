using System;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable DelegateSubtraction

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Networking.PlayerConnection;
#endif

using UnityEngine.Networking.PlayerConnection;
using UnityEngine.ResourceManagement.Util;

namespace UnityEngine.ResourceManagement.Diagnostics
{
    /// <summary>
    /// Collects ResourceManager events and passed them on the registered event handlers.  In editor play mode, events are passed directly to the ResourceManager profiler window.
    /// In player builds, events are sent to the editor via the EditorConnection API.
    /// </summary>
    public class DiagnosticEventCollectorSingleton : ComponentSingleton<DiagnosticEventCollectorSingleton>
    {
        static Guid s_editorConnectionGuid;

        internal Dictionary<int, DiagnosticEvent> m_CreatedEvents = new Dictionary<int, DiagnosticEvent>();
        internal List<DiagnosticEvent> m_UnhandledEvents = new List<DiagnosticEvent>();

        internal DelegateList<DiagnosticEvent> s_EventHandlers = DelegateList<DiagnosticEvent>.CreateWithGlobalCache();

        /// <summary>
        /// The guid used for the PlayerConnect messaging system.
        /// </summary>
        public static Guid PlayerConnectionGuid
        {
            get
            {
                if (s_editorConnectionGuid == Guid.Empty)
                    s_editorConnectionGuid = new Guid(1, 2, 3, new byte[] { 20, 1, 32, 32, 4, 9, 6, 44 });
                return s_editorConnectionGuid;
            }
        }

        /// <inheritdoc/>
        protected override string GetGameObjectName() => "EventCollector";

        /// <summary>
        /// Register for diagnostic events.  If there is no collector, this will fail and return false.
        /// </summary>
        /// <param name="handler">The handler method action.</param>
        /// <param name="register">Register or unregister.</param>
        /// <param name="create">If true, the event collector will be created if needed.</param>
        /// <returns>True if registered, false if not.</returns>
        public static bool RegisterEventHandler(Action<DiagnosticEvent> handler, bool register, bool create)
        {
            if (register && (create || Exists))
            {
                Instance.RegisterEventHandler(handler);
                return true;
            }

            if (!register && Exists)
            {
                Instance.UnregisterEventHandler(handler);
            }

            return false;
        }

        internal void RegisterEventHandler(Action<DiagnosticEvent> handler)
        {
            Debug.Assert(m_UnhandledEvents != null, "DiagnosticEventCollectorSingleton.RegisterEventHandler - s_unhandledEvents == null.");
            if (handler == null)
                throw new ArgumentNullException("handler");
            s_EventHandlers.Add(handler);

            //Ensure that events are handled in frame order
            var combinedAndSortedList = m_UnhandledEvents.Concat(m_CreatedEvents.Values).OrderBy(evt => evt.Frame);
            foreach (var evt in combinedAndSortedList)
                handler(evt);
            m_UnhandledEvents.Clear();
        }

        /// <summary>
        /// Unregister event hander
        /// </summary>
        /// <param name="handler">Method or delegate that will handle the events</param>
        public void UnregisterEventHandler(Action<DiagnosticEvent> handler)
        {
            if (handler == null)
                throw new ArgumentNullException("handler");
            s_EventHandlers.Remove(handler);
        }

        /// <summary>
        /// Send a <see cref="DiagnosticEvent"/> event to all registered handlers
        /// </summary>
        /// <param name="diagnosticEvent">The event to send</param>
        public void PostEvent(DiagnosticEvent diagnosticEvent)
        {
            if (diagnosticEvent.Stream == (int)ResourceManager.DiagnosticEventType.AsyncOperationCreate && !m_CreatedEvents.ContainsKey(diagnosticEvent.ObjectId))
                m_CreatedEvents.Add(diagnosticEvent.ObjectId, diagnosticEvent);
            else if (diagnosticEvent.Stream == (int)ResourceManager.DiagnosticEventType.AsyncOperationDestroy)
                m_CreatedEvents.Remove(diagnosticEvent.ObjectId);

            Debug.Assert(m_UnhandledEvents != null, "DiagnosticEventCollectorSingleton.PostEvent - s_unhandledEvents == null.");

            if (s_EventHandlers.Count > 0)
                s_EventHandlers.Invoke(diagnosticEvent);
            else
                m_UnhandledEvents.Add(diagnosticEvent);
        }

        void Awake()
        {
#if !UNITY_EDITOR
            RegisterEventHandler((DiagnosticEvent diagnosticEvent) => {PlayerConnection.instance.Send(DiagnosticEventCollectorSingleton.PlayerConnectionGuid, diagnosticEvent.Serialize()); });
#endif
        }

        float m_lastTickSent = 0;
        int m_lastFrame = 0;
        float fpsAvg = 30;
        void Update()
        {
            if (s_EventHandlers.Count > 0)
            {
                var elapsed = Time.realtimeSinceStartup - m_lastTickSent;
                if (elapsed > .25f)
                {
                    var fps = (Time.frameCount - m_lastFrame) / elapsed;
                    m_lastFrame = Time.frameCount;
                    fpsAvg = (fpsAvg + fps) * .5f;
                    m_lastTickSent = Time.realtimeSinceStartup;
                    int heapKB = (int)(Profiling.Profiler.GetMonoUsedSizeLong() / 1024);
                    PostEvent(new DiagnosticEvent("FrameCount", "FPS", 2, 1, Time.frameCount, (int)fpsAvg, null));
                    PostEvent(new DiagnosticEvent("MemoryCount", "MonoHeap", 3, 2, Time.frameCount, heapKB, null));
                }
            }
        }
    }

    /// <summary>
    /// Collects ResourceManager events and passed them on the registered event handlers.  In editor play mode, events are passed directly to the ResourceManager profiler window.
    /// In player builds, events are sent to the editor via the EditorConnection API.
    /// </summary>
    public class DiagnosticEventCollector : MonoBehaviour
    {
        static DiagnosticEventCollector s_Collector;

        /// <summary>
        /// The guid used for the PlayerConnect messaging system.
        /// </summary>
        public static Guid PlayerConnectionGuid => DiagnosticEventCollectorSingleton.PlayerConnectionGuid;

        /// <summary>
        /// Retrieves the global event collector.  A new one is created if needed.
        /// </summary>
        /// <returns>The event collector global instance.</returns>
        public static DiagnosticEventCollector FindOrCreateGlobalInstance()
        {
            if (s_Collector == null)
            {
                var go = new GameObject("EventCollector", typeof(DiagnosticEventCollector));
                s_Collector = go.GetComponent<DiagnosticEventCollector>();
                go.hideFlags = HideFlags.DontSave;// HideFlags.HideAndDontSave;
            }
            return s_Collector;
        }

        /// <summary>
        /// Register for diagnostic events.  If there is no collector, this will fail and return false.
        /// </summary>
        /// <param name="handler">The handler method action.</param>
        /// <param name="register">Register or unregister.</param>
        /// <param name="create">If true, the event collector will be created if needed.</param>
        /// <returns>True if registered, false if not.</returns>
        public static bool RegisterEventHandler(Action<DiagnosticEvent> handler, bool register, bool create) => DiagnosticEventCollectorSingleton.RegisterEventHandler(handler, register, create);

        /// <summary>
        /// Unregister event hander
        /// </summary>
        /// <param name="handler">Method or delegate that will handle the events</param>
        public void UnregisterEventHandler(Action<DiagnosticEvent> handler) => DiagnosticEventCollectorSingleton.Instance.UnregisterEventHandler(handler);

        /// <summary>
        /// Send a <see cref="DiagnosticEvent"/> event to all registered handlers
        /// </summary>
        /// <param name="diagnosticEvent">The event to send</param>
        public void PostEvent(DiagnosticEvent diagnosticEvent) => DiagnosticEventCollectorSingleton.Instance.PostEvent(diagnosticEvent);

#if UNITY_EDITOR
        public static class PlayStateNotifier
        {
        }
#endif
    }
}
