using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor.AddressableAssets.Diagnostics.Data;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.Diagnostics;

namespace UnityEditor.AddressableAssets.Tests.Diagnostics
{
    public class EventDataPlayerSessionTests
    {
        private DiagnosticEvent CreateNewGenericEvent(ResourceManager.DiagnosticEventType eventType, int frame, int value)
        {
            return new DiagnosticEvent(null, "GenericEvent", 1000, (int) eventType, frame, value, null);
        }

        private DiagnosticEvent CreateNewInstantiationEvent(int frame, int value)
        {
            return new DiagnosticEvent(null, "InstanceEvent", 1000, (int) ResourceManager.DiagnosticEventType.AsyncOperationCreate, frame, value, null);
        }

        private DiagnosticEvent CreateNewGenericEvent(ResourceManager.DiagnosticEventType eventType, int frame, int value, int id)
        {
            return new DiagnosticEvent(null, "GenericEvent", id, (int) eventType, frame, value, null);
        }

        private DiagnosticEvent CreateNewGenericEvent(ResourceManager.DiagnosticEventType eventType, int frame, int value, int id, int[] dependencies)
        {
            return new DiagnosticEvent(null, "GenericEvent", id, (int) eventType, frame, value, dependencies);
        }
        
        
        [Test]
        public void EventDataPlayerSession_GetFrameEvents_TestSimpleCase()
        {
            EventDataPlayerSession edps = new EventDataPlayerSession();
            
            DiagnosticEvent evt1 = CreateNewGenericEvent(ResourceManager.DiagnosticEventType.AsyncOperationCreate, 0, 5);
            DiagnosticEvent evt2 = CreateNewGenericEvent(ResourceManager.DiagnosticEventType.AsyncOperationCreate, 0, 6);
            
            bool entryCreated = false;
            
            edps.AddSample(evt1, true, ref entryCreated);
            edps.AddSample(evt2, true, ref entryCreated);
            
            
            Assert.AreEqual(new List<DiagnosticEvent>{evt1, evt2} , edps.GetFrameEvents(0), "Events were not properly added together in FrameEvents");
            Assert.AreEqual(null, edps.GetFrameEvents(1), "FrameEvents for frame 1 should be null. ");
        }

        [Test]
        public void EventDataPlayerSession_AddSample_SimpleRecordEventCase()
        {
            EventDataPlayerSession edps = new EventDataPlayerSession();

            DiagnosticEvent evt1 = CreateNewGenericEvent(ResourceManager.DiagnosticEventType.AsyncOperationReferenceCount, 0, 1);
            DiagnosticEvent evt2 = CreateNewGenericEvent(ResourceManager.DiagnosticEventType.AsyncOperationCreate, 0, 2);
            DiagnosticEvent evt3 = CreateNewGenericEvent(ResourceManager.DiagnosticEventType.AsyncOperationReferenceCount, 1, 1);

            bool entryCreated = false;
            
            edps.AddSample(evt1, true, ref entryCreated);
            Assert.IsTrue(entryCreated, "Either a new entry was supposed to be created, but was not, or the value of entryCreated was not properly updated.");
            
            edps.AddSample(evt2, true, ref entryCreated);
            Assert.IsFalse(entryCreated, "evt2: Either a new entry wasn't supposed to be created, but was, or the value of entryCreated was not properly updated.");
            
            edps.AddSample(evt3, true, ref entryCreated);
            Assert.IsFalse(entryCreated, "evt3: Either a new entry wasn't supposed to be created for, but was, or the value of entryCreated was not properly updated.");
            
            Assert.AreEqual(new List <DiagnosticEvent> {evt1, evt2}, edps.m_FrameEvents[0], "evt1 and evt2 were not properly added to m_FrameEvents");
            Assert.AreEqual(2, edps.m_EventCountDataSet.GetStreamValue(0, 0), "Value of the stream for m_EventCountDataSet was not properly set.");
            Assert.AreEqual(new List<DiagnosticEvent> {evt3}, edps.m_FrameEvents[1], "evt3 was not properly added to m_FrameEvents");
            Assert.IsTrue(edps.m_DataSets.ContainsKey(evt1.ObjectId), "The corresponding EventDataSet for evt1-3 was not added to m_DataSets");

            EventDataSet eds = null;
            
            bool edsFound = edps.m_DataSets.TryGetValue(evt1.ObjectId, out eds);

            Assert.IsTrue(edsFound);
            Assert.AreEqual(2, eds.GetStream((int) ResourceManager.DiagnosticEventType.AsyncOperationReferenceCount).samples.Count);
            Assert.AreEqual(1, eds.GetStream((int) ResourceManager.DiagnosticEventType.AsyncOperationCreate).samples.Count);
        }
        
        
        [Test]
        public void EventDataPlayerSession_AddSample_SimpleInstantiationCase()
        {
            EventDataPlayerSession edps = new EventDataPlayerSession();

            DiagnosticEvent evt1 = CreateNewInstantiationEvent(0, 1);
            DiagnosticEvent evt2 = CreateNewInstantiationEvent(0, 1);
            DiagnosticEvent evt3 = CreateNewInstantiationEvent(1, 1);
            DiagnosticEvent evt4 = CreateNewInstantiationEvent(2, 0);

            bool entryCreated = false;

            edps.AddSample(evt1, false, ref entryCreated);
            Assert.IsFalse(entryCreated, "evt1: Either a new entry was created when it shouldn't have been, or the value of entryCreated was not properly updated.");
            
            edps.AddSample(evt2, false, ref entryCreated);
            Assert.IsFalse(entryCreated, "evt2: Either a new entry was created when it shouldn't have been, or the value of entryCreated was not properly updated.");
            
            edps.AddSample(evt3, false, ref entryCreated);
            Assert.IsFalse(entryCreated, "evt3: Either a new entry was created when it shouldn't have been, or the value of entryCreated was not properly updated.");

            Assert.AreEqual(2, edps.m_InstantitationCountDataSet.GetStreamValue(0, 0), "Stream value for frame 0 not properly set to 2.");
            Assert.AreEqual(2, edps.m_InstantitationCountDataSet.GetStreamValue(0, 1), "Stream value for frame 1 was updated prematurely - stream values for instantiation counts should not be updated until it is certain that all events for a given frame have been collected. ");
            
            edps.AddSample(evt4, false, ref entryCreated);
            Assert.IsFalse(entryCreated, "evt3: Either a new entry was created when it shouldn't have been, or the value of entryCreated was not properly updated.");
            
            Assert.AreEqual(1, edps.m_InstantitationCountDataSet.GetStreamValue(0, 1), "Stream value for frame 1 was not updated properly, should be updated once the sample is added for the following frame.");
        }

        [Test]
        public void EventDataPlayerSession_AddSample_MultipleObjectCase()
        {
            EventDataPlayerSession edps = new EventDataPlayerSession();

            DiagnosticEvent evt1 = CreateNewGenericEvent(ResourceManager.DiagnosticEventType.AsyncOperationReferenceCount, 0, 1, 1000);
            DiagnosticEvent evt2 = CreateNewGenericEvent(ResourceManager.DiagnosticEventType.AsyncOperationReferenceCount, 1, 1, 1001);

            bool entryCreated = false;
            
            edps.AddSample(evt1, false, ref entryCreated);
            Assert.IsTrue(entryCreated, "evt1: Either a new entry was supposed to be created, but was not, or the value of entryCreated was not properly updated.");
            Assert.IsTrue(edps.m_DataSets.ContainsKey(evt1.ObjectId), "Entry for evt1 should have been created, but was not added to m_DataSets");
            Assert.AreEqual(1, edps.m_DataSets[evt1.ObjectId].GetStreamValue((int) ResourceManager.DiagnosticEventType.AsyncOperationReferenceCount, 0), "Value was not correctly set within EventDataSet object for evt1. ");
            
            edps.AddSample(evt2, false, ref entryCreated);
            Assert.IsTrue(entryCreated, "evt2: Either a new entry was supposed to be created, but was not, or the value of entryCreated was not properly updated.");
            Assert.IsTrue(edps.m_DataSets.ContainsKey(evt2.ObjectId), "Entry for evt2 should have been created, but was not added to m_DataSets");
            Assert.AreEqual(1, edps.m_DataSets[evt2.ObjectId].GetStreamValue((int) ResourceManager.DiagnosticEventType.AsyncOperationReferenceCount, 1), "Value was not correctly set within EventDataSet object for evt2. ");
        }

        [Test]
        public void EventDataPlayerSession_HandleOperationDestroy_NoDependencyCase()
        {
            EventDataPlayerSession edps = new EventDataPlayerSession();
            
            DiagnosticEvent creationEvt = CreateNewGenericEvent(ResourceManager.DiagnosticEventType.AsyncOperationCreate, 0, 1, 1000);
            DiagnosticEvent deletionEvt = CreateNewGenericEvent(ResourceManager.DiagnosticEventType.AsyncOperationDestroy, 1, 1, 1000);
            
            bool entryCreated = false;
            
            edps.AddSample(creationEvt, false, ref entryCreated);
            Assert.IsTrue(entryCreated, "Either a new entry was supposed to be created, but was not, or the value of entryCreated was not properly updated.");

            edps.AddSample(deletionEvt, false, ref entryCreated);
            Assert.IsFalse(entryCreated, "Either a new entry was created when it shouldn't have been, or the value of entryCreated was not properly updated.");
            
            Assert.AreEqual(1, edps.m_Queue.Count, "Deletion event should've been added to the removal queue.");

            edps.HandleOperationDestroy(deletionEvt);
            
            Assert.IsFalse(edps.m_DataSets.ContainsKey(creationEvt.ObjectId), "Dataset was not properly removed on deletion.");
        }
        
        [Test]
        public void EventDataPlayerSession_HandleOperationDestroy_DoesNotDestroyOnNonzeroRefcount()
        {
            EventDataPlayerSession edps = new EventDataPlayerSession();
            
            DiagnosticEvent creationEvt = CreateNewGenericEvent(ResourceManager.DiagnosticEventType.AsyncOperationCreate, 0, 1, 1000);
            DiagnosticEvent refcountEvt = CreateNewGenericEvent(ResourceManager.DiagnosticEventType.AsyncOperationReferenceCount, 0, 1, 1000);
            DiagnosticEvent deletionEvt = CreateNewGenericEvent(ResourceManager.DiagnosticEventType.AsyncOperationDestroy, 1, 1, 1000);
            
            bool entryCreated = false;
            
            edps.AddSample(creationEvt, false, ref entryCreated);
            Assert.IsTrue(entryCreated, "creationEvt: Either a new entry was supposed to be created, but was not, or the value of entryCreated was not properly updated.");
            
            edps.AddSample(refcountEvt, false, ref entryCreated);
            Assert.IsFalse(entryCreated, "refcountEvt: Either a new entry was created when it shouldn't have been, or the value of entryCreated was not properly updated.");

            edps.AddSample(deletionEvt, false, ref entryCreated);
            Assert.IsFalse(entryCreated, "deletionEvt: Either a new entry was created when it shouldn't have been, or the value of entryCreated was not properly updated.");
            
            Assert.AreEqual(1, edps.m_Queue.Count, "Deletion event should've been added to the removal queue.");

            edps.HandleOperationDestroy(deletionEvt);
            
            Assert.IsTrue(edps.m_DataSets.ContainsKey(creationEvt.ObjectId), "Dataset should not have been removed because it's refcount is greater than 0. ");
        }
        
        
        
        [Test]
        public void EventDataPlayerSession_HandleOperationDestroy_DependencyHasNoRefcountCase()
        {
            EventDataPlayerSession edps = new EventDataPlayerSession();

            DiagnosticEvent dependencyEvt = CreateNewGenericEvent(ResourceManager.DiagnosticEventType.AsyncOperationReferenceCount, 0, 0, 1001);
            DiagnosticEvent creationEvt = CreateNewGenericEvent(ResourceManager.DiagnosticEventType.AsyncOperationCreate, 0, 1, 1000, new int[] {dependencyEvt.ObjectId});
            DiagnosticEvent deletionEvt = CreateNewGenericEvent(ResourceManager.DiagnosticEventType.AsyncOperationDestroy, 1, 1, 1000, new int[] {dependencyEvt.ObjectId});

            bool entryCreated = false;
            
            edps.AddSample(dependencyEvt, false, ref entryCreated);
            Assert.IsTrue(entryCreated, "Either a new entry was supposed to be created, but was not, or the value of entryCreated was not properly updated.");
            
            edps.AddSample(creationEvt, false, ref entryCreated);
            Assert.IsTrue(entryCreated, "Either a new entry was supposed to be created, but was not, or the value of entryCreated was not properly updated.");

            edps.AddSample(deletionEvt, false, ref entryCreated);
            Assert.IsFalse(entryCreated, "Either a new entry was created when it shouldn't have been, or the value of entryCreated was not properly updated.");
            Assert.AreEqual(1, edps.m_RootStreamEntry.Children.Count(), "DependencyEvt's id should have been removed from RootStreamEntry's children because it is a dependency.");
            
            Assert.AreEqual(1, edps.m_Queue.Count, "Deletion event should have been added to the removal queue.");

            edps.HandleOperationDestroy(deletionEvt);

            Assert.IsFalse(edps.m_DataSets.ContainsKey(deletionEvt.ObjectId), "DataSet was not properly removed after dependency was cleared. ");
            
            Assert.AreEqual(1, edps.m_Queue.Count, "No further deletion events should have been added to the deletion queue.");
        }

        [Test]
        public void EventDataPlayerSession_HandleOperationDestroy_DestroyedEventHasParentCase()
        {
            EventDataPlayerSession edps = new EventDataPlayerSession();

            DiagnosticEvent dependencyEvt = CreateNewGenericEvent(ResourceManager.DiagnosticEventType.AsyncOperationReferenceCount, 0, 0, 1001);
            DiagnosticEvent creationEvt = CreateNewGenericEvent(ResourceManager.DiagnosticEventType.AsyncOperationCreate, 0, 1, 1000, new int[] {dependencyEvt.ObjectId});
            DiagnosticEvent deletionEvt = CreateNewGenericEvent(ResourceManager.DiagnosticEventType.AsyncOperationDestroy, 1, 1, 1001);

            bool entryCreated = false;
            
            edps.AddSample(dependencyEvt, false, ref entryCreated);
            Assert.IsTrue(entryCreated, "Either a new entry was supposed to be created, but was not, or the value of entryCreated was not properly updated.");
            
            edps.AddSample(creationEvt, false, ref entryCreated);
            Assert.IsTrue(entryCreated, "Either a new entry was supposed to be created, but was not, or the value of entryCreated was not properly updated.");

            edps.AddSample(deletionEvt, false, ref entryCreated);
            Assert.IsFalse(entryCreated, "Either a new entry was created when it shouldn't have been, or the value of entryCreated was not properly updated.");
            Assert.AreEqual(1, edps.m_RootStreamEntry.Children.Count(), "DependencyEvt's id should have been removed from RootStreamEntry's children because it is a dependency.");
            
            Assert.AreEqual(1, edps.m_Queue.Count, "Deletion event should have been added to the removal queue.");

            edps.HandleOperationDestroy(deletionEvt);

            Assert.IsFalse(edps.m_DataSets.ContainsKey(deletionEvt.ObjectId), "DataSet was not properly removed after dependency was cleared. ");

            EventDataSet creationEvtEds = null;

            Assert.IsTrue(edps.m_DataSets.TryGetValue(creationEvt.ObjectId, out creationEvtEds), "Parent event was removed from m_DataSets incorrectly.");
            Assert.IsNull(creationEvtEds.m_Children, "dependencyEvt's dataset should have been removed from the children of creationEvt's dataset. ");

            Assert.AreEqual(1, edps.m_Queue.Count, "No further deletion events should have been added to the deletion queue.");
        }
    }
}
