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
    public class ProfileSettingsTests : AddressableAssetTestBase
    {
        [Test]
        public void AddRemoveProfile()
        {
            //Arrange
            Assert.IsNotNull(Settings.profileSettings);
            Settings.activeProfileId = null;
            var mainId = Settings.profileSettings.Reset();

            //Act
            var secondId = Settings.profileSettings.AddProfile("TestProfile", mainId);

            //Assert
            bool foundIt = false;
            foreach (var prof in Settings.profileSettings.profiles)
            {
                if (prof.profileName == "TestProfile")
                    foundIt = true;
            }
            Assert.IsTrue(foundIt);
            Assert.IsNotEmpty(secondId);

            //Act again
            Settings.profileSettings.RemoveProfile(secondId);

            //Assert again
            foundIt = false;
            foreach (var prof in Settings.profileSettings.profiles)
            {
                if (prof.profileName == "TestProfile")
                    foundIt = true;
            }
            Assert.IsFalse(foundIt);
        }

        [Test]
        public void CreateValuePropagatesValue()
        {
            //Arrange
            Assert.IsNotNull(Settings.profileSettings);
            Settings.activeProfileId = null;
            var mainId = Settings.profileSettings.Reset();
            var secondId = Settings.profileSettings.AddProfile("TestProfile", mainId);

            //Act
            string path = "/Assets/Important";
            Settings.profileSettings.CreateValue("SomePath", path);

            //Assert
            Assert.AreEqual(path, Settings.profileSettings.GetValueByName(mainId, "SomePath"));
            Assert.AreEqual(path, Settings.profileSettings.GetValueByName(secondId, "SomePath"));
        }

        [Test]
        public void SetValueOnlySetsDesiredProfile()
        {
            //Arrange
            Assert.IsNotNull(Settings.profileSettings);
            Settings.activeProfileId = null;
            var mainId = Settings.profileSettings.Reset();
            string originalPath = "/Assets/Important";
            Settings.profileSettings.CreateValue("SomePath", originalPath);
            var secondId = Settings.profileSettings.AddProfile("TestProfile", mainId);

            //Act
            string newPath = "/Assets/LessImportant";
            Settings.profileSettings.SetValue(secondId, "SomePath", newPath);

            //Assert
            Assert.AreEqual(originalPath, Settings.profileSettings.GetValueByName(mainId, "SomePath"));
            Assert.AreEqual(newPath, Settings.profileSettings.GetValueByName(secondId, "SomePath"));
        }

        [Test]
        public void CanGetValueById()
        {
            //Arrange
            Assert.IsNotNull(Settings.profileSettings);
            Settings.activeProfileId = null;
            var mainId = Settings.profileSettings.Reset();
            string originalPath = "/Assets/Important";
            Settings.profileSettings.CreateValue("SomePath", originalPath);

            //Act
            string varId = null;
            foreach (var variable in Settings.profileSettings.profileEntryNames)
            {
                if (variable.ProfileName == "SomePath")
                {
                    varId = variable.Id;
                    break;
                }
            }

            //Assert
            Assert.AreEqual(originalPath, Settings.profileSettings.GetValueById(mainId, varId));
        }

        [Test]
        public void EvaluatingUnknownIdReturnsIdAsResult()
        {
            //Arrange
            Assert.IsNotNull(Settings.profileSettings);
            Settings.activeProfileId = null;
            Settings.profileSettings.Reset();

            //Act
            string badIdName = "BadIdName";


            //Assert
            string baseValue = Settings.profileSettings.GetValueById(Settings.activeProfileId, badIdName);
            Assert.AreEqual(badIdName, Settings.profileSettings.EvaluateString(Settings.activeProfileId, baseValue));
        }

        [Test]
        public void MissingVariablesArePassThrough()
        {
            //Arrange
            Assert.IsNotNull(Settings.profileSettings);
            Settings.activeProfileId = null;

            //Act
            Settings.profileSettings.Reset();

            //Assert
            Assert.AreEqual("VariableNotThere", Settings.profileSettings.GetValueById("invalid key", "VariableNotThere"));
        }

        [Test]
        public void CanRenameEntry()
        {
            //Arrange
            Assert.IsNotNull(Settings.profileSettings);
            Settings.activeProfileId = null;
            Settings.profileSettings.Reset();
            string entryName = "SomeName";
            string newName = "NewerName";
            string originalPath = "/Assets/Important";
            Settings.profileSettings.CreateValue(entryName, originalPath);

            AddressableAssetProfileSettings.ProfileIdData currEntry = null;
            foreach (var entry in Settings.profileSettings.profileEntryNames)
            {
                if (entry.ProfileName == entryName)
                {
                    currEntry = entry;
                    break;
                }
            }

            //Act
            Assert.NotNull(currEntry);
            currEntry.SetName(newName, Settings.profileSettings);

            //Assert
            Assert.AreEqual(currEntry.ProfileName, newName);
        }

        [Test]
        public void RenameProfileFailsOnNullProfile()
        {
            Assert.IsNotNull(Settings.profileSettings, "Profile settings should not be null");
            Settings.activeProfileId = null;
            var baseid = Settings.profileSettings.Reset();
            var profile1Id = Settings.profileSettings.AddProfile("Profile1", baseid);

            AddressableAssetProfileSettings.BuildProfile nullProfile = null;
            //Act
            bool renameSuccessful = Settings.profileSettings.RenameProfile(nullProfile, "invalidId");

            //Assert
            LogAssert.Expect(LogType.Error, "Profile rename failed because profile passed in is null");
            Assert.AreEqual(false, renameSuccessful, "Rename succeeded when it should have failed because of null profile.");
            Assert.AreEqual("Profile1", Settings.profileSettings.GetProfileName(profile1Id), "Profile name was changed when rename should have failed.");
        }

        [Test]
        public void RenameProfileFailsOnExternallyCreatedProfile()
        {
            //Arrange
            Assert.IsNotNull(Settings.profileSettings, "Profile settings should not be null");
            Settings.activeProfileId = null;
            var baseid = Settings.profileSettings.Reset();
            var baseProfile = Settings.profileSettings.GetProfile(baseid);
            var externalProfile = new AddressableAssetProfileSettings.BuildProfile("Bad profile", baseProfile, Settings.profileSettings);
            //Act
            bool renameSuccessful = Settings.profileSettings.RenameProfile(externalProfile, "new name");

            //Assert
            Assert.AreEqual(true, renameSuccessful, "Rename was unsuccessful when it should have succeeded.");
            Assert.AreEqual("new name", externalProfile.profileName, "Profile name was not changed despite rename succeeding. ");
            Assert.AreEqual(1, Settings.profileSettings.profiles.Count, "Number of profiles changed when should be left the same");
            Assert.AreEqual(null, Settings.profileSettings.GetProfile(externalProfile.id), "Externally created profile was added to profile settings despite not being created properly.");
        }

        [Test]
        public void RenameProfileFailsOnInvalidId()
        {
            Assert.IsNotNull(Settings.profileSettings, "Profile settings should not be null");
            Settings.activeProfileId = null;
            var baseid = Settings.profileSettings.Reset();
            var profile1Id = Settings.profileSettings.AddProfile("Profile1", baseid);

            //Act
            bool renameSuccessful = Settings.profileSettings.RenameProfile("invalidId", "invalidId");

            //Assert
            LogAssert.Expect(LogType.Error, "Profile rename failed because profile with sought id does not exist.");
            Assert.AreEqual(false, renameSuccessful, "Rename succeeded when it should have failed because of invalid id.");
            Assert.AreEqual("Profile1", Settings.profileSettings.GetProfileName(profile1Id), "Profile name was changed when rename should have failed.");
        }

        [Test]
        public void RenameProfileFailsOnDuplicateName()
        {
            //Arrange
            Assert.IsNotNull(Settings.profileSettings, "Profile settings should not be null");
            Settings.activeProfileId = null;
            var baseid = Settings.profileSettings.Reset();
            var profile1Id = Settings.profileSettings.AddProfile("Profile1", baseid);
            var profile2Id = Settings.profileSettings.AddProfile("Profile2", baseid);

            //Act
            bool renameSuccessful = Settings.profileSettings.RenameProfile(profile1Id, "Profile2");

            //Assert
            LogAssert.Expect(LogType.Error, "Profile rename failed because new profile name is not unique.");
            Assert.AreEqual(false, renameSuccessful, "Rename succeeded when failure should have occured from duplicate name");
            Assert.AreEqual("Profile1", Settings.profileSettings.GetProfileName(profile1Id), "Profile name was changed when rename should have failed.");
        }

        [Test]
        public void RenameProfileFailsOnRenameDefault()
        {
            //Arrange
            Assert.IsNotNull(Settings.profileSettings, "Profile settings should not be null. ");
            Settings.activeProfileId = null;
            var defaultId = Settings.profileSettings.Reset();

            //Act
            bool renameSuccessful = Settings.profileSettings.RenameProfile(defaultId, "Profile2");
            LogAssert.Expect(LogType.Error, "Profile rename failed because default profile cannot be renamed.");
            Assert.AreEqual(false, renameSuccessful, "Rename succeeded when failure should have occured because default is not renamable.");
            Assert.AreEqual("Default", Settings.profileSettings.GetProfileName(defaultId), "Name for default profile was changed when default should be prevented from changing.");
        }

        [Test]
        public void RenameProfileFailsOnInvalidName()
        {
            //Arrange
            Assert.IsNotNull(Settings.profileSettings, "Profile settings should not be null");
            Settings.activeProfileId = null;
            var baseid = Settings.profileSettings.Reset();
            var profile1Id = Settings.profileSettings.AddProfile("Profile1", baseid);

            //Act
            bool renameSuccessful = Settings.profileSettings.RenameProfile(profile1Id, "          ");

            //Assert
            LogAssert.Expect(LogType.Error, "Profile rename failed because new profile name must not be only spaces.");
            Assert.AreEqual(false, renameSuccessful, "Rename succeeded when failure should have occured from invalid name");
            Assert.AreEqual("Profile1", Settings.profileSettings.GetProfileName(profile1Id), "Profile name was changed when rename should have failed.");
        }

        [Test]
        public void RenameProfileFailsOnUnchangedName()
        {
            //Arrange
            Assert.IsNotNull(Settings.profileSettings, "Profile settings is null");
            Settings.activeProfileId = null;
            var baseid = Settings.profileSettings.Reset();
            var profile1Id = Settings.profileSettings.AddProfile("Profile1", baseid);

            //Act
            bool renameSuccessful = Settings.profileSettings.RenameProfile(profile1Id, "Profile1");

            //Assert
            Assert.AreEqual(false, renameSuccessful, "Rename succeeded when failure should have occured from unchanged name");
            Assert.AreEqual("Profile1", Settings.profileSettings.GetProfileName(profile1Id), "Profile name was changed when rename should have failed.");
        }

        [Test]
        public void RenameProfileSucceedsOnValidName()
        {
            //Arrange
            Assert.IsNotNull(Settings.profileSettings, "Profile settings is null");
            Settings.activeProfileId = null;
            var baseid = Settings.profileSettings.Reset();
            var profile1Id = Settings.profileSettings.AddProfile("Profile1", baseid);
            var profile2Id = Settings.profileSettings.AddProfile("Profile2", baseid);

            //Act
            bool renameSuccessful = Settings.profileSettings.RenameProfile(profile1Id, "Profile3");

            //Assert
            Assert.AreEqual(true, renameSuccessful, "Rename failed when name change should have been successful.");
            Assert.AreEqual("Profile3", Settings.profileSettings.GetProfile(profile1Id).profileName, "Rename was successful, but name was not correctly changed.");
            Assert.AreEqual("Profile2", Settings.profileSettings.GetProfile(profile2Id).profileName, "Rename was successful, but other profile name was changed when it shouldn't have been.");
            Assert.IsTrue(ProfileWindow.m_Reload, "m_Reload should be set to true after a successful change is made. ");
        }

        [Test]
        public void CannotRenameEntryToDuplicateName()
        {
            //Arrange
            Assert.IsNotNull(Settings.profileSettings);
            Settings.activeProfileId = null;
            Settings.profileSettings.Reset();
            string entryName = "SomeName";
            string newName = "NewerName";
            string originalPath = "/Assets/Important";
            Settings.profileSettings.CreateValue(entryName, originalPath);
            Settings.profileSettings.CreateValue(newName, originalPath);

            AddressableAssetProfileSettings.ProfileIdData currEntry = null;
            foreach (var entry in Settings.profileSettings.profileEntryNames)
            {
                if (entry.ProfileName == entryName)
                {
                    currEntry = entry;
                    break;
                }
            }

            //Act
            Assert.NotNull(currEntry);
            currEntry.SetName(newName, Settings.profileSettings);

            //Assert
            Assert.AreNotEqual(currEntry.ProfileName, newName);
        }

        [Test]
        public void CustomValuesWithDeprecatedFormatAreStripped()
        {
            Assert.IsNotNull(Settings.profileSettings);
            Settings.activeProfileId = null;
            Settings.profileSettings.Reset();

            Settings.profileSettings.CreateValue(AddressableAssetProfileSettings.customEntryString, "/Assets/ShouldBeStripped");

            var tmp = Settings.profileSettings.m_ProfileVersion;
            Settings.profileSettings.m_ProfileVersion = 0;
            var names = Settings.profileSettings.profileEntryNames.Select(e => e.ProfileName).ToList();
            Settings.profileSettings.m_ProfileVersion = tmp;

            Assert.False(names.Contains(AddressableAssetProfileSettings.customEntryString));
        }

        [Test]
        public void GenerateUniqueName_AlwaysReturnUniqueNames()
        {
            int count = 5;
            var list = new List<string>(count);
            for (int i = 0; i < count; i++)
                list.Add(AddressableAssetProfileSettings.GenerateUniqueName("base", list));

            Assert.AreEqual(count, list.Distinct().Count());
        }
    }
}
