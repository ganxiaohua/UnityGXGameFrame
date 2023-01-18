using NUnit.Framework;
using UnityEditor.AddressableAssets.Diagnostics.Data;
using UnityEngine.ResourceManagement.Diagnostics;

namespace UnityEditor.AddressableAssets.Tests.Diagnostics
{
    public class EventDataPlayerSessionCollectionTests
    {
        [Test]
        public void EventDataPlayerSessionCollection_RecordEvent_ReturnsFalseOnNullEventHandler()
        {
            EventDataPlayerSessionCollection edpsc = new EventDataPlayerSessionCollection(null);
            
            Assert.IsFalse(edpsc.RecordEvent(new DiagnosticEvent()), "RecordEvent should return null if m_OnRecordEvent is null");
        }

        [Test]
        public void EventDataPlayerSessionCollection_GetPlayerSession_ProperlyCreatesWhenCreateIsTrue()
        {
            EventDataPlayerSessionCollection edpsc = new EventDataPlayerSessionCollection((DiagnosticEvent x) => true);
            EventDataPlayerSession edps = edpsc.GetPlayerSession(0, true);
            
            Assert.NotNull(edps, "New EventDataPlayerSession should have been created.");
            Assert.AreEqual("Player 0", edps.EventName);
        }

        [Test]
        public void EventDataPlayerSessionCollection_GetPlayerSession_ReturnsNullOnNoIdMatch()
        {
            EventDataPlayerSessionCollection edpsc = new EventDataPlayerSessionCollection((DiagnosticEvent x) => true);
            EventDataPlayerSession edps = edpsc.GetPlayerSession(0, false);
            
            Assert.IsNull(edps, "New EventDataPlayerSession should not be created when create = false and there is no id match. ");
        }

        [Test]
        public void EventDataPlayerSessionCollection_GetSessionByIndex_ReturnsNullOnInvalidInput()
        {
            EventDataPlayerSessionCollection edpsc = new EventDataPlayerSessionCollection((DiagnosticEvent x) => true);
            Assert.IsNull(edpsc.GetSessionByIndex(0), "Trying to request a session with a nonexistent index should return null. ");
            
            edpsc.AddSession("test session", 0);
            Assert.IsNull(edpsc.GetSessionByIndex(2), "Trying to request a session with a nonexistent index should return null. ");
            Assert.NotNull(edpsc.GetSessionByIndex(0), "Session not returned properly on valid index. ");
        }

        [Test]
        public void EventDataPlayerSessionCollection_GetSessionIndexById_SimpleCase()
        {
            EventDataPlayerSessionCollection edpsc = new EventDataPlayerSessionCollection((DiagnosticEvent x) => true);
            edpsc.AddSession("Test session", 0);
            edpsc.AddSession("Test session 2", 1);
            
            Assert.AreEqual(1, edpsc.GetSessionIndexById(1), "Session index not properly returned. ");
        }

        [Test]
        public void EventDataPlayerSessionCollection_GetSessionIndexById_NullCase()
        {
            EventDataPlayerSessionCollection edpsc = new EventDataPlayerSessionCollection((DiagnosticEvent x) => true);
            edpsc.AddSession("Test session", 0);
            edpsc.AddSession("Test session 2", 1);
            
            Assert.AreEqual(-1, edpsc.GetSessionIndexById(10000000), "Incorrect value returned, GetSessionIndexById should return -1 when the queried id does not exist.");
        }

        [Test]
        public void EventDataPlayerSessionCollection_RemoveSession_SimpleCase()
        {
            EventDataPlayerSessionCollection edpsc = new EventDataPlayerSessionCollection((DiagnosticEvent x) => true);
            edpsc.AddSession("Test session", 0);
            edpsc.RemoveSession(0);
            
            Assert.AreEqual(0, edpsc.GetSessionCount(), "Session was not properly removed.");
        }

        [Test]
        public void EventDataPlayerSessionCollection_TestPlayerConnection()
        {
            EventDataPlayerSessionCollection edpsc = new EventDataPlayerSessionCollection((DiagnosticEvent x) => true);
            
            edpsc.AddSession("Default", 0);
            edpsc.GetPlayerSession(1000, true).IsActive = true;
            Assert.AreEqual(2, edpsc.GetSessionCount(), "Session not properly added. ");
            
            int connectedSessionIndex = edpsc.GetSessionIndexById(1000);
            Assert.AreEqual(1, connectedSessionIndex, "Session index not properly set. ");
        }
    }
}
