using NUnit.Framework;
using UnityEditor.AddressableAssets.Diagnostics.Data;
using UnityEngine.ResourceManagement;

namespace UnityEditor.AddressableAssets.Tests.Diagnostics
{
    public class EventDataSetTests
    {
        //Note, we use EventDataSetStream.GetValue for these tests so that we can avoid testing multiple 
        //EventDataSet methods in the same test
        [Test]
        public void EventDataSet_AddSample_AddsToCorrectStream()
        {
            EventDataSet testSet = new EventDataSet();
            testSet.AddSample(2, 0, 2);
            Assert.NotNull(testSet.m_Streams, "List of streams not properly initialized.");
            Assert.AreEqual(true, testSet.m_Streams.Count > 0, "List of streams was initialized, but not assigned to.");
            Assert.NotNull(testSet.m_Streams[2], "Stream 2 should not be null, as a sample should've been added to it.");
            Assert.AreEqual(2, testSet.m_Streams[2].GetValue(0), "Value retrieved from stream was incorrect.");
        }

        [Test]
        public void EventDataSet_AddSample_ProperlyCreatesNullStreams()
        {
            EventDataSet testSet = new EventDataSet();
            testSet.AddSample(4, 0, 2);
            Assert.NotNull(testSet.m_Streams, "List of streams not properly initialized.");
            Assert.AreEqual(true, testSet.m_Streams.Count > 0, "List of streams was initialized, but not assigned to.");
            
            Assert.IsNull(testSet.m_Streams[0], "0th stream was not null initialized like expected.");
            Assert.IsNull(testSet.m_Streams[1], "1th stream was not null initialized like expected.");
            Assert.IsNull(testSet.m_Streams[2], "2nd stream was not null initialized like expected.");
            Assert.IsNull(testSet.m_Streams[3], "3rd stream was not null initialized like expected.");
            
            Assert.NotNull(testSet.m_Streams[4], "4th stream was null initialized when it should have had a value added to it.");
            Assert.AreEqual(2, testSet.m_Streams[4].GetValue(0), "Value retrieved from stream was incorrect.");
        }

        //this tests both HasChildren and AddChild
        [Test]
        public void EventDataSet_AddChild_SimpleAdd()
        {
            EventDataSet testSet = new EventDataSet();
            EventDataSet childSet = new EventDataSet();
            testSet.AddChild(childSet);
            Assert.AreEqual(true, testSet.HasChildren, "hasChildren not properly updated.");
            Assert.AreEqual(childSet.ObjectId, testSet.m_Children[childSet.ObjectId].ObjectId, "Child not successfully retrieved from dictionary. ");
        }
        
        [Test]
        public void EventDataSet_AddChild_ConfirmChildObjectConsistentWithReference()
        {
            EventDataSet parentSet1 = new EventDataSet();
            EventDataSet parentSet2 = new EventDataSet();
            EventDataSet childSet = new EventDataSet();
            
            parentSet1.AddChild(childSet);
            parentSet2.AddChild(childSet);

            childSet.DisplayName = "SameChild";
            
            Assert.AreEqual("SameChild", parentSet1.m_Children[childSet.ObjectId].DisplayName, "Display name not changed in m_Children despite being changed on child EventDataSet itself.");
            Assert.AreEqual("SameChild", parentSet2.m_Children[childSet.ObjectId].DisplayName, "Display name not changed in m_Children despite being changed on child EventDataSet itself.");
        }

        [Test]
        public void EventDataSet_HasDataAfterFrame_ProperBehaviorOnNoChildCase()
        {
            EventDataSet parentSet = new EventDataSet();
            parentSet.AddSample(0,0, 1);
            Assert.AreEqual(true, parentSet.HasDataAfterFrame(0), "Returned false despite the fact that the last frame of the stream should be nonzero.");
        }

        [Test]
        public void EventDataSet_HasDataAfterFrame_ProperBehaviorOnChildHasStreamCase()
        {
            EventDataSet parentSet = new EventDataSet();
            EventDataSet childSet = new EventDataSet();
            
            childSet.AddSample(0, 0, 1);
            
            Assert.AreEqual(false, parentSet.HasDataAfterFrame(0), "HasDataAfterFrame should always be false for a dataset with no streams and no children.");
            Assert.AreEqual(true, childSet.HasDataAfterFrame(0), "HasDataAfterFrame should return true because the last sample of one of it's streams is nonzero.");
            
            parentSet.AddChild(childSet);
            
            Assert.AreEqual(true, parentSet.HasDataAfterFrame(0), "HasDataAfterFrame returned false even though a child EventDataSet has data past frame 0.");
        }

        [Test]
        public void EventDataSet_HasDataAfterFrame_ProperBehaviorOnChildHasNoStreamCase()
        {
            EventDataSet parentSet = new EventDataSet();
            EventDataSet childSet = new EventDataSet();

            Assert.AreEqual(false, parentSet.HasDataAfterFrame(0), "HasDataAfterFrame should always be false for a dataset with no streams and no children.");
            Assert.AreEqual(false, childSet.HasDataAfterFrame(0), "HasDataAfterFrame should always be false for a dataset with no streams and no children.");
            
            parentSet.AddChild(childSet);
            
            Assert.AreEqual(false, parentSet.HasDataAfterFrame(0), "HasDataAfterFrame should always be false for a dataset with no streams and a child that has no samples/streams.");
        }

        [Test]
        public void EventDataSet_RemoveChild_SimpleAddRemove()
        {
            EventDataSet parentSet = new EventDataSet();
            EventDataSet childSet = new EventDataSet();
            
            parentSet.AddChild(childSet);
            
            Assert.AreEqual(true, parentSet.HasChildren, "HasChildren should be true after a child is added. ");
            
            parentSet.RemoveChild(childSet.ObjectId);
            
            Assert.AreEqual(false, parentSet.HasChildren, "HasChildren should be false after child is removed. ");
            Assert.IsNull(parentSet.m_Children, "m_Children should be set to null after last child is removed. ");
        }

        [Test]
        public void EventDataSet_GetStreamValue_SimpleGet()
        {
            EventDataSet testSet = new EventDataSet();
            testSet.AddSample(0, 0, 1);
            Assert.AreEqual(1, testSet.GetStreamValue(0,0));
        }

        [Test]
        public void EventDataSet_GetStreamValue_ReturnsZeroOnNullStreamIndex()
        {
            EventDataSet testSet = new EventDataSet();
            //stream number > stream count case
            Assert.AreEqual(0, testSet.GetStreamValue(1, 1), "GetStreamValue should return 0 when queried with a stream index greater than the count of total streams. ");
            
            testSet.AddSample(2, 0, 2);
            
            //null stream case
            Assert.AreEqual(0, testSet.GetStreamValue(1, 0), "GetStreamValue should return 0 when querying the index of an uninitialized stream. ");
        }

        [Test]
        public void EventDataSet_GetStream_ReturnsNullOnNonexistentStream()
        {
            EventDataSet testSet = new EventDataSet();
            
            Assert.IsNull(testSet.GetStream(1), "GetStream should return null when queried with a stream index greater than the count of total streams.");

            testSet.AddSample(2, 0, 2);
            
            Assert.IsNull(testSet.GetStream(1), "GetStream should return null when querying an uninitialized stream. ");
        }

        [Test]
        public void EventDataSet_GetStream_SimpleGet()
        {
            EventDataSet testSet = new EventDataSet();
            testSet.AddSample(0, 0, 1);
            var soughtStream = testSet.m_Streams[0];
            Assert.AreEqual(soughtStream, testSet.GetStream(0), "Correct stream not returned by GetStream");
        }

        [Test]
        public void EventDataSet_HasRefcountAfterFrame_ReturnsTrueOnTrivialCase()
        {
            EventDataSet testSet = new EventDataSet();
            //Add sample to the refcount stream
            testSet.AddSample((int) ResourceManager.DiagnosticEventType.AsyncOperationReferenceCount, 5, 1);
            Assert.AreEqual(true, testSet.HasRefcountAfterFrame(0, true), "Refcount after frame was considered 0 when sample should have been added.");
        }

        [Test]
        public void EventDataSet_HasRefcountAfterFrame_ReturnsTrueOnChildCase()
        {
            EventDataSet testSet = new EventDataSet();
            testSet.AddSample((int) ResourceManager.DiagnosticEventType.AsyncOperationReferenceCount, 2, 1);
            testSet.AddSample((int) ResourceManager.DiagnosticEventType.AsyncOperationReferenceCount, 4, 0);
            
            EventDataSet childSet = new EventDataSet();
            childSet.AddSample((int) ResourceManager.DiagnosticEventType.AsyncOperationReferenceCount, 5, 1);
            
            testSet.AddChild(childSet);

            Assert.AreEqual(true, testSet.HasRefcountAfterFrame(3, true), "Child refcount not properly considered when checking parent's refcount.");
        }
        
    }
}
