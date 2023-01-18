using System;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.Diagnostics;

namespace UnityEditor.AddressableAssets.Diagnostics.Data
{
    [Serializable]
    class EventDataPlayerSessionCollection
    {
        List<EventDataPlayerSession> m_PlayerSessions = new List<EventDataPlayerSession>();
        Func<DiagnosticEvent, bool> m_OnRecordEvent;

        public EventDataPlayerSessionCollection(Func<DiagnosticEvent, bool> onRecordEvent)
        {
            m_OnRecordEvent = onRecordEvent;
        }

        internal bool RecordEvent(DiagnosticEvent e)
        {
            if (m_OnRecordEvent != null)
                return m_OnRecordEvent(e);
            return false;
        }

        public bool ProcessEvent(DiagnosticEvent diagnosticEvent, int sessionId)
        {
            var session = GetPlayerSession(sessionId, true);
            bool entryCreated = false;
            session.AddSample(diagnosticEvent, RecordEvent(diagnosticEvent), ref entryCreated);
            return entryCreated;
        }

        public EventDataPlayerSession GetSessionByIndex(int index)
        {
            if (m_PlayerSessions.Count == 0 || m_PlayerSessions.Count <= index)
                return null;

            return m_PlayerSessions[index];
        }

        internal int GetSessionIndexById(int playerId)
        {
            return m_PlayerSessions.FindIndex(edps => edps.PlayerId == playerId);
        }

        public EventDataPlayerSession GetPlayerSession(int playerId, bool create)
        {
            foreach (var c in m_PlayerSessions)
                if (c.PlayerId == playerId)
                    return c;
            if (create)
            {
                var c = new EventDataPlayerSession("Player " + playerId, playerId);
                m_PlayerSessions.Add(c);
                return c;
            }
            return null;
        }

        internal void RemoveSession(int playerId)
        {
            m_PlayerSessions.RemoveAll(edps => edps.PlayerId == playerId);
        }
        
        public string[] GetConnectionNames()
        {
            string[] names = new string[m_PlayerSessions.Count];
            for (int i = 0; i < m_PlayerSessions.Count; i++)
                names[i] = m_PlayerSessions[i].EventName;
            return names;
        }

        internal int GetSessionCount()
        {
            return m_PlayerSessions.Count;
        }

        public void AddSession(string name, int id)
        {
            m_PlayerSessions.Add(new EventDataPlayerSession(name, id));
        }

        public void Update()
        {
            foreach (var s in m_PlayerSessions)
                s.Update();
        }
    }
}
