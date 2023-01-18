using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor.AddressableAssets.GUI;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnityEditor.AddressableAssets.Tests
{
    public class ProfileGroupTypeTests
    {
        [Test]
        public void CreateEmptyProfileGroupType_Returns_InvalidProfileGroupType()
        {
            ProfileGroupType profileGroupType = new ProfileGroupType();
            Assert.False(profileGroupType.IsValidGroupType());
        }

        [Test]
        public void CreatePrefixedProfileGroupType_Returns_InvalidProfileGroupType()
        {
            ProfileGroupType profileGroupType = new ProfileGroupType("prefix");
            Assert.False(profileGroupType.IsValidGroupType());
        }

        [Test]
        public void CreateValidProfileGroupType_Returns_ValidProfileGroupType()
        {
            ProfileGroupType.GroupTypeVariable buildPath = new ProfileGroupType.GroupTypeVariable("BuildPath", "Test Build Path");
            ProfileGroupType.GroupTypeVariable loadPath = new ProfileGroupType.GroupTypeVariable("LoadPath", "Test Load Path");
            ProfileGroupType profileGroupType = new ProfileGroupType("prefix");
            bool aAdded = profileGroupType.AddVariable(buildPath);
            bool bAdded = profileGroupType.AddVariable(loadPath);
            Assert.IsTrue(aAdded && bAdded, "Failed to Add variables");
            Assert.True(profileGroupType.IsValidGroupType());
        }

        [Test]
        public void GetPathValuesBySuffix_Returns_ExpectedPathValues()
        {
            ProfileGroupType.GroupTypeVariable buildPath = new ProfileGroupType.GroupTypeVariable("BuildPath", "Test Build Path");
            ProfileGroupType profileGroupType = new ProfileGroupType("prefix");
            bool variableAdded = profileGroupType.AddVariable(buildPath);
            Assert.IsTrue(variableAdded, "Failed to add GroupType variable");
            Assert.AreEqual("Test Build Path", profileGroupType.GetVariableBySuffix(buildPath.Suffix).Value);
        }

        [Test]
        public void GetName_Returns_ExpectedVariableName()
        {
            ProfileGroupType.GroupTypeVariable buildPath = new ProfileGroupType.GroupTypeVariable("BuildPath", "Test Build Path");
            ProfileGroupType profileGroupType = new ProfileGroupType("prefix");
            bool gtAdded = profileGroupType.AddVariable(buildPath);
            Assert.IsTrue(gtAdded, $"Failed to add groupType {gtAdded}");
            Assert.AreEqual("prefix.BuildPath", profileGroupType.GetName(buildPath));
        }

        [Test]
        public void AddVariableToGroupType_AddsVariable()
        {
            ProfileGroupType.GroupTypeVariable buildPath = new ProfileGroupType.GroupTypeVariable("BuildPath", "Test Build Path");
            ProfileGroupType.GroupTypeVariable loadPath = new ProfileGroupType.GroupTypeVariable("LoadPath", "Test Load Path");
            ProfileGroupType profileGroupType = new ProfileGroupType("prefix");

            Assert.IsTrue(profileGroupType.AddVariable(buildPath));
            Assert.IsTrue(profileGroupType.AddVariable(loadPath));
            Assert.True(profileGroupType.Variables.Count == 2);
        }

        [Test]
        public void AddDuplicateVariableToGroupType_FailsToAddVariable()
        {
            ProfileGroupType.GroupTypeVariable buildPath = new ProfileGroupType.GroupTypeVariable("BuildPath", "Test Build Path");
            ProfileGroupType profileGroupType = new ProfileGroupType("prefix");

            Assert.IsTrue(profileGroupType.AddVariable(buildPath));
            LogAssert.Expect(LogType.Error, "prefix.BuildPath already exists.");
            Assert.IsFalse(profileGroupType.AddVariable(buildPath));
        }
        
        [Test]
        public void AddOrUpdateVariableToGroupType_AddsVariable()
        {
            ProfileGroupType profileGroupType = new ProfileGroupType("TestPrefix");
            var bp = profileGroupType.GetVariableBySuffix("TestSuffix");
            Assert.IsNull(bp);
            
            profileGroupType.AddOrUpdateVariable(new ProfileGroupType.GroupTypeVariable("TestSuffix", "TestValue"));
            
            bp = profileGroupType.GetVariableBySuffix("TestSuffix");
            Assert.IsNotNull(bp);
            Assert.AreEqual(bp.m_Value, "TestValue", "Unexpected GroupTypeVariable, variable value should be TestValue");
            Assert.True(profileGroupType.Variables.Count == 1);
        }
        
        [Test]
        public void AddOrUpdateVariableToGroupType_UpdatesVariable()
        {
            ProfileGroupType profileGroupType = new ProfileGroupType("TestPrefix");
            var bp = profileGroupType.GetVariableBySuffix("TestSuffix");
            Assert.IsNull(bp);
            
            profileGroupType.AddOrUpdateVariable(new ProfileGroupType.GroupTypeVariable("TestSuffix", "TestValue"));
            
            bp = profileGroupType.GetVariableBySuffix("TestSuffix");
            Assert.IsNotNull(bp);
            Assert.AreEqual(bp.m_Value, "TestValue", "Unexpected GroupTypeVariable, variable value should be TestValue");
            Assert.True(profileGroupType.Variables.Count == 1);
            
            profileGroupType.AddOrUpdateVariable(new ProfileGroupType.GroupTypeVariable("TestSuffix", "UpdatedTestValue"));
            
            bp = profileGroupType.GetVariableBySuffix("TestSuffix");
            Assert.IsNotNull(bp);
            Assert.AreEqual(bp.m_Value, "UpdatedTestValue", "Unexpected GroupTypeVariable, variable value should be UpdatedTestValue");
            Assert.True(profileGroupType.Variables.Count == 1);
        }

        [Test]
        public void RemoveVariable_Returns_ExpectedAction()
        {
            ProfileGroupType.GroupTypeVariable buildPath = new ProfileGroupType.GroupTypeVariable("BuildPath", "Test Build Path");
            ProfileGroupType profileGroupType = new ProfileGroupType("prefix");
            profileGroupType.AddVariable(buildPath);
            profileGroupType.RemoveVariable(buildPath);
            Assert.True(profileGroupType.Variables.Count == 0);
        }

        [Test]
        public void RemoveNonExistentVariable_Returns_ExpectedAction()
        {
            ProfileGroupType.GroupTypeVariable buildPath = new ProfileGroupType.GroupTypeVariable("BuildPath", "Test Build Path");
            ProfileGroupType profileGroupType = new ProfileGroupType("prefix");
            profileGroupType.RemoveVariable(buildPath);
            LogAssert.Expect(LogType.Error, "prefix.BuildPath does not exist.");
        }

        [Test]
        public void DoesContainVariable_Returns_True()
        {

            ProfileGroupType.GroupTypeVariable buildPath = new ProfileGroupType.GroupTypeVariable("BuildPath", "Test Build Path");
            ProfileGroupType profileGroupType = new ProfileGroupType("prefix");
            profileGroupType.AddVariable(buildPath);
            Assert.True(profileGroupType.ContainsVariable(buildPath));
        }

        [Test]
        public void DoesContainVariable_Returns_False()
        {

            ProfileGroupType.GroupTypeVariable buildPath = new ProfileGroupType.GroupTypeVariable("BuildPath", "Test Build Path");
            ProfileGroupType profileGroupType = new ProfileGroupType("prefix");
            Assert.False(profileGroupType.ContainsVariable(buildPath));
        }
    }
}
