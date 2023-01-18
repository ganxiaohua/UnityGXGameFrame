using NUnit.Framework;
using UnityEditor.AddressableAssets.Diagnostics.Data;
using UnityEditor.AddressableAssets.Diagnostics.GUI;

namespace UnityEditor.AddressableAssets.Tests.Diagnostics
{
    public class EventGraphListViewTests
    {
        [Test]
        public void DataStreamEntry_IdEqualsDataSetObjectId()
        {
            EventDataSet eds = new EventDataSet(100, "graph", "name", 0); 
            EventGraphListView.DataStreamEntry entry = new EventGraphListView.DataStreamEntry(eds, 0);
            
            Assert.AreEqual(eds.ObjectId, entry.id, "Event Hiding functionality relies on DataStreamEntry ids being the same as their underlying ObjectId.");
        }
    }
}
