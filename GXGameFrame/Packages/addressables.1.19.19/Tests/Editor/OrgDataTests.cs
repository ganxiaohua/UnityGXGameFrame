using System;
using NUnit.Framework;
using UnityEditor.AddressableAssets.Settings;

namespace UnityEditor.AddressableAssets.Tests
{
    public class OrgDataTests
    {
        [Test]
        public void ParseOrgData_ReturnsValidOrgData()
        {
            const string data =
                @"{
                    ""id"" : ""test_id"",
                    ""name"" : ""test_name"",
                    ""foreign_key"" : ""12345678909876"",
                    ""billable_user_fk"" : 12345678909876,
                    ""org_identifier"" : ""72f505cf-7149-44be-0000-12401583906f"",
                    ""orgIdentifier"" : ""72f505cf-7149-44be-0000-12401583906f""
                }";
            var result = OrgData.ParseOrgData(data);
            Assert.AreEqual("test_id", result.id);
            Assert.AreEqual("test_name", result.name);
            Assert.AreEqual("12345678909876", result.foreign_key);
            Assert.AreEqual("12345678909876", result.billable_user_fk);
            Assert.AreEqual("72f505cf-7149-44be-0000-12401583906f", result.org_identifier);
            Assert.AreEqual("72f505cf-7149-44be-0000-12401583906f", result.orgIdentifier);
        }

        [Test]
        public void ParseOrgData_ThrowsException()
        {
            const string data = "invalid data";
            Assert.Throws<ArgumentException>(() => { OrgData.ParseOrgData(data); });
        }
    }
}
