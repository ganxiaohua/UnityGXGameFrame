using NUnit.Framework;
using UnityEditor.AddressableAssets.Diagnostics.Data;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnityEditor.AddressableAssets.Tests.Diagnostics
{
    public class EventDataSetStreamTests
    {
        [Test]
        public void EventDataSetStream_AddSample_SimpleAdd()
        {
            EventDataSetStream testStream = new EventDataSetStream();
            testStream.AddSample(0, 5);
            Assert.AreEqual(5, testStream.GetValue(0), "Value of added sample was not correctly retrieved. ");
        }

        [Test]
        public void EventDataSetStream_AddSample_OutOfOrderFrameFails()
        {
            EventDataSetStream testStream = new EventDataSetStream();
            testStream.AddSample(5, 5);
            testStream.AddSample(0, 0);
            Assert.AreEqual(1, testStream.samples.Count, "Sample still added to stream despite the fact that the event should have been ignored.");
        }

        [Test]
        public void EventDataSetStream_AddSample_ReplaceValueOnDuplicateAdd()
        {
            EventDataSetStream testStream = new EventDataSetStream();
            testStream.AddSample(0, 5);
            testStream.AddSample(0, 6);
            Assert.AreEqual(6, testStream.GetValue(0), "Sample was not properly replaced. ");
            Assert.AreEqual(1, testStream.samples.Count, "Sample was not properly replaced.");
        }

        [Test]
        public void EventDataSetStream_GetValue_ReturnsZeroOnInvalidFrame()
        {
            EventDataSetStream testStream = new EventDataSetStream();
            Assert.AreEqual(0, testStream.GetValue(0), "Expected default value of 0 because there are no samples currently in EventDataStream.");
            testStream.AddSample(0, 1);
            Assert.AreEqual(0, testStream.GetValue(-1), "Expected default value because there is no sample with frame -1.");
        }

        [Test]
        public void EventDataSetStream_GetValue_ReturnsCorrectValueOnMultipleCalls()
        {
            EventDataSetStream testStream = new EventDataSetStream();
            testStream.AddSample(0, 0);
            testStream.AddSample(1, 1);
            testStream.AddSample(2, 2);
            Assert.AreEqual(2, testStream.GetValue(2), "Incorrect value retrieved by GetValue.");
            Assert.AreEqual(1, testStream.GetValue(1), "Incorrect value retrieved by GetValue.");
            Assert.AreEqual(0, testStream.GetValue(0), "Incorrect value retrieved by GetValue.");
        }

        [Test]
        public void EventDataSetStream_GetValue_ReturnsLastValueOnTooLargeFrame()
        {
            EventDataSetStream testStream = new EventDataSetStream();
            testStream.AddSample(5, 5);
            Assert.AreEqual(5, testStream.GetValue(100), "Final contained sample was not returned on too large query frame.");
        }

        [Test]
        public void EventDataSetStream_HasDataAfterFrame_ReturnsFalseOnNoSamples()
        {
            EventDataSetStream testStream = new EventDataSetStream();
            Assert.AreEqual(false, testStream.HasDataAfterFrame(5), "Should have returned true since no samples have been added to the stream. ");
        }

        [Test]
        public void EventDataSetStream_HasDataAfterFrame_ReturnsFalseOnLastSampleEmptyData()
        {
            EventDataSetStream testStream = new EventDataSetStream();
            testStream.AddSample(0, 1);
            testStream.AddSample(5, 0);
            Assert.AreEqual(false, testStream.HasDataAfterFrame(6), "HasDataAfterFrame returned true when last sample's data is empty and queried frame is past the last sample.");
        }

        [Test]
        public void EventDataSetSTream_HasDataAfterFrame_ReturnsTrueOnLastSampleNonZero()
        {
            EventDataSetStream testStream = new EventDataSetStream();
            testStream.AddSample(0, 1);
            testStream.AddSample(2, 1);
            Assert.AreEqual(true, testStream.HasDataAfterFrame(1), "HasDataAfterFrame returned false when there should be a sample with data following frame 1.");
        }
        
        
    }
}
