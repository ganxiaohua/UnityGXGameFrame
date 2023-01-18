using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.Diagnostics;

namespace UnityEditor.AddressableAssets.Diagnostics.Data
{
    [Serializable]
    class EventDataPlayerSession
    {
        internal EventDataSet m_RootStreamEntry = new EventDataSet(0, null, null, -1);
        internal EventDataSet m_EventCountDataSet;
        internal EventDataSet m_InstantitationCountDataSet;
        
        internal Dictionary<int, EventDataSet> m_DataSets = new Dictionary<int, EventDataSet>();
        Dictionary<int, HashSet<int>> m_objectToParents = new Dictionary<int, HashSet<int>>();
        internal Dictionary<int, List<DiagnosticEvent>> m_FrameEvents = new Dictionary<int, List<DiagnosticEvent>>();
        
        internal List<EvtQueueData> m_Queue = new List<EvtQueueData>();
        
        
        string m_EventName;
        int m_PlayerId;
        bool m_IsActive;
        int m_LatestFrame;
        int m_StartFrame;
        int m_LastInstantiationFrame = -1;
        int m_LastFrameInstantiationCount = 0;
        int m_LastFrameWithEvents = -1;
        
        internal bool NeedsReload = false;

        const int k_DestroyEventFrameDelay = 300;
        
        int m_FrameCount = 300;
        
        public EventDataSet RootStreamEntry { get { return m_RootStreamEntry; } }
        public string EventName { get { return m_EventName; } }
        public int PlayerId { get { return m_PlayerId; } }
        public bool IsActive { get { return m_IsActive; } set { m_IsActive = value; } }
        public int LatestFrame { get { return m_LatestFrame; } }
        public int StartFrame { get { return m_StartFrame; } }
        public int FrameCount { get { return m_FrameCount; } }
      
        public EventDataPlayerSession() { }

        public EventDataPlayerSession(string eventName, int playerId)
        {
            m_EventName = eventName;
            m_PlayerId = playerId;
            m_IsActive = true;
        }

        internal void Clear()
        {
            RootStreamEntry.Clear();
            m_FrameEvents.Clear();
            m_LastFrameWithEvents = -1;
            m_LastFrameInstantiationCount = 0;
            m_EventCountDataSet = null;
            m_InstantitationCountDataSet = null;
            m_DataSets.Clear();
            m_objectToParents.Clear();
        }

        internal List<DiagnosticEvent> GetFrameEvents(int frame)
        {
            List<DiagnosticEvent> frameEvents;
            if (m_FrameEvents.TryGetValue(frame, out frameEvents))
                return frameEvents;
            return null;
        }

        internal class EvtQueueData
        {
            public DiagnosticEvent Event;
            public int DestroyFrame;
        }

        internal void AddSample(DiagnosticEvent evt, bool recordEvent, ref bool entryCreated)
        {
            m_LatestFrame = evt.Frame;
            m_StartFrame = m_LatestFrame - m_FrameCount;
            entryCreated = false;
            
            bool countedAllRecordedEventsOnCurrentFrame = evt.Frame > m_LastFrameWithEvents
                && m_LastFrameWithEvents >= 0
                && m_FrameEvents[m_LastFrameWithEvents] != null 
                && m_FrameEvents[m_LastFrameWithEvents].Count > 0
                && m_EventCountDataSet != null;

            //once all recorded events on a given frame have been logged in m_FrameEvents, create a sample for alevents
            if (countedAllRecordedEventsOnCurrentFrame)
            {
                m_EventCountDataSet.AddSample(0, m_LastFrameWithEvents, m_FrameEvents[m_LastFrameWithEvents].Count);
            }

            // Registers events under "Event Counts" (excluding instantiations)
            if (recordEvent && !evt.DisplayName.StartsWith("Instance"))
            {
                HandleRecordedEvent(evt);
            }
            
            bool countedAllInstantiationEventsOnCurrentFrame = evt.Frame > m_LastInstantiationFrame 
                && m_LastInstantiationFrame >= 0 
                && m_LastFrameInstantiationCount > 0
                && m_InstantitationCountDataSet != null;
            
            if (countedAllInstantiationEventsOnCurrentFrame)
            {
                m_InstantitationCountDataSet.AddSample(0, m_LastInstantiationFrame, m_LastFrameInstantiationCount);
                m_LastFrameInstantiationCount = 0;
            }
            
            if (evt.DisplayName.StartsWith("Instance"))
            {
                if ((ResourceManager.DiagnosticEventType) evt.Stream == ResourceManager.DiagnosticEventType.AsyncOperationCreate)
                    HandleInstantiationEvent(evt);
                return;
            }

            //if creation event, create a data set and update all dependecies
            if (!m_DataSets.ContainsKey(evt.ObjectId))
            {
                HandleEventDataSetCreation(evt);
                entryCreated = true;
            }
            
            EventDataSet data = null;
            if (m_DataSets.TryGetValue(evt.ObjectId, out data))
            {
                data.AddSample(evt.Stream, evt.Frame, evt.Value);
            }
            
            if ((ResourceManager.DiagnosticEventType) evt.Stream == ResourceManager.DiagnosticEventType.AsyncOperationDestroy)
            {
                m_Queue.Add(new EvtQueueData { Event = evt, DestroyFrame = evt.Frame});
            }
        }
        
        internal void HandleRecordedEvent(DiagnosticEvent evt)
        {
            List<DiagnosticEvent> frameEvents;
            if (!m_FrameEvents.TryGetValue(evt.Frame, out frameEvents))
            {
                if (m_EventCountDataSet == null)
                {
                    m_EventCountDataSet = new EventDataSet(0, "EventCount", "Event Counts", EventDataSet.k_EventCountSortOrder);
                    RootStreamEntry.AddChild(m_EventCountDataSet);
                }
                m_LastFrameWithEvents = evt.Frame;
                m_FrameEvents.Add(evt.Frame, frameEvents = new List<DiagnosticEvent>());
            }
            frameEvents.Add(evt);
        }

        internal void HandleInstantiationEvent(DiagnosticEvent evt)
        {
            if (m_InstantitationCountDataSet == null)
            {
                    m_InstantitationCountDataSet = new EventDataSet(1, "InstantiationCount", "Instantiation Counts", EventDataSet.k_InstanceCountSortOrder);
                    RootStreamEntry.AddChild(m_InstantitationCountDataSet);
            }
            m_LastInstantiationFrame = evt.Frame;
            m_LastFrameInstantiationCount++;
        }

        internal void HandleEventDataSetCreation(DiagnosticEvent evt)
        {
            var ds = new EventDataSet(evt);
            m_DataSets.Add(evt.ObjectId, ds);
            if (evt.Dependencies != null)
            {
                foreach (var d in evt.Dependencies)
                {
                    EventDataSet depDS;
                    if (m_DataSets.TryGetValue(d, out depDS))
                    {
                        ds.AddChild(depDS);
                        HashSet<int> depParents = null;
                        if (!m_objectToParents.TryGetValue(d, out depParents))
                        {
                            RootStreamEntry.RemoveChild(d);
                            m_objectToParents.Add(d, depParents = new HashSet<int>());
                        }
                        depParents.Add(evt.ObjectId);
                    }
                }
            }

            if (!m_objectToParents.ContainsKey(evt.ObjectId))
            {
                RootStreamEntry.AddChild(ds);
            }
              
        }  
        
        public void Update()
        {
            foreach (var q in m_Queue)
            {
                if (m_LatestFrame - q.DestroyFrame >= k_DestroyEventFrameDelay)
                {
                    HandleOperationDestroy(q.Event);
                }
            }

            m_Queue.RemoveAll(q => m_LatestFrame - q.DestroyFrame >= k_DestroyEventFrameDelay);
        }

        internal void HandleOperationDestroy(DiagnosticEvent evt)
        {
            EventDataSet ds;

            if (m_DataSets.TryGetValue(evt.ObjectId, out ds))
            {
                if (ds.HasRefcountAfterFrame(evt.Frame, false))
                    return;
            }

            NeedsReload = true;
            
            if (evt.Dependencies != null)
            {
                foreach (var d in evt.Dependencies)
                {
                    HashSet<int> depParents = null;
                    if (m_objectToParents.TryGetValue(d, out depParents))
                    {
                        depParents.Remove(evt.ObjectId);
                        if (depParents.Count == 0)
                        {
                            m_objectToParents.Remove(d);
                            if (m_DataSets.ContainsKey(d))
                                m_RootStreamEntry.AddChild(m_DataSets[d]);
                        }
                    }
                }
            }
            m_DataSets.Remove(evt.ObjectId);

            HashSet<int> parents = null;
            if (m_objectToParents.TryGetValue(evt.ObjectId, out parents))
            {
                foreach (var parentId in parents)
                {
                    EventDataSet parent;
                    if (m_DataSets.TryGetValue(parentId, out parent))
                        parent.RemoveChild(evt.ObjectId);
                }
            }
            else
            {
                RootStreamEntry.RemoveChild(evt.ObjectId);
            }
        }
    }
}
