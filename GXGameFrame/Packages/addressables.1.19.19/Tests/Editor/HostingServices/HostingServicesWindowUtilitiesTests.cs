using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.AddressableAssets.GUI;

namespace UnityEditor.AddressableAssets.Tests.HostingServices
{
    public class HostingServicesWindowUtilitiesTests
    {
        [Test]
        public void DictsAreEqual_ReturnsTrueOnSameValueSameRef()
        {
            var originalDict = new Dictionary<string, string>();
            originalDict.Add("a", "1");
            originalDict.Add("b", "2");
            originalDict.Add("c", "3");
            var copyDict = originalDict;

            Assert.IsTrue(HostingServicesWindow.DictsAreEqual(originalDict, copyDict), "Copy of dictionary should be equal to original, but isn't.");
        }

        [Test]
        public void DictsAreEqual_ReturnsTrueOnSameValuesDifRef()
        {
            var dict1 = new Dictionary<string, string>();
            dict1.Add("a", "1");
            dict1.Add("b", "2");
            dict1.Add("c", "3");

            var dict2 = new Dictionary<string, string>();
            dict2.Add("a", "1");
            dict2.Add("b", "2");
            dict2.Add("c", "3");
            
            Assert.IsTrue(HostingServicesWindow.DictsAreEqual(dict1, dict2), "Two identically created dictionaries should be equal, but aren't.");
        }

        [Test]
        public void DictsAreEqual_ReturnsFalseOnSameKeyDifVal()
        {
            var dict1 = new Dictionary<string, string>();
            dict1.Add("a", "x");
            dict1.Add("b", "y");
            dict1.Add("c", "z");

            var dict2 = new Dictionary<string, string>();
            dict2.Add("a", "1");
            dict2.Add("b", "2");
            dict2.Add("c", "3");
            
            Assert.IsFalse(HostingServicesWindow.DictsAreEqual(dict1, dict2), "Same keys with different values should not be considered equal.");
        }
        
        [Test]
        public void DictsAreEqual_ReturnsFalseOnSameValDifKey()
        {
            var dict1 = new Dictionary<string, string>();
            dict1.Add("x", "1");
            dict1.Add("y", "2");
            dict1.Add("z", "3");

            var dict2 = new Dictionary<string, string>();
            dict2.Add("a", "1");
            dict2.Add("b", "2");
            dict2.Add("c", "3");
            
            Assert.IsFalse(HostingServicesWindow.DictsAreEqual(dict1, dict2), "Same values with different keys should not be considered equal.");
        }
        

        [Test]
        public void DictsAreEqual_ReturnsFalseOnSubset()
        {
            var dict1 = new Dictionary<string, string>();
            dict1.Add("a", "1");
            dict1.Add("b", "2");
            
            var dict2 = new Dictionary<string, string>();
            dict2.Add("a", "1");
            dict2.Add("b", "2");
            dict2.Add("c", "3");
            
            Assert.IsFalse(HostingServicesWindow.DictsAreEqual(dict1, dict2), "Subset should not be considered equal (smaller first case)");
            Assert.IsFalse(HostingServicesWindow.DictsAreEqual(dict2, dict1), "Subset should not be considered equal (larger first case)");
        }

        [Test]
        public void DictsAreEqual_ReturnsFalseOnTriviallyUnequal()
        {
            var dict1 = new Dictionary<string, string>();
            dict1.Add("a", "1");
            
            var dict2 = new Dictionary<string, string>();
            dict2.Add("b", "2");
            
            Assert.IsFalse(HostingServicesWindow.DictsAreEqual(dict1, dict2), "Should return false on trivially false case");
        }
    }
}
