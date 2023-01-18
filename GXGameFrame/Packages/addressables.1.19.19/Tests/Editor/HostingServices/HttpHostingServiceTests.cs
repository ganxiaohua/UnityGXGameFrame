using System;
using System.IO;
using System.Net;
using System.Threading;
using NUnit.Framework;
using UnityEditor.AddressableAssets.HostingServices;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace UnityEditor.AddressableAssets.Tests.HostingServices
{
    using Random = System.Random;

    public class HttpHostingServiceTests
    {
        class MyWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri uri)
            {
                var w = base.GetWebRequest(uri);
                Debug.Assert(w != null);
                w.Timeout = 2000;
                return w;
            }
        }

        HttpHostingService m_Service;
        string m_ContentRoot;
        readonly WebClient m_Client;

        public HttpHostingServiceTests()
        {
            m_Client = new MyWebClient();
        }

        static byte[] GetRandomBytes(int size)
        {
            var rand = new Random();
            var buf = new byte[size];
            rand.NextBytes(buf);
            return buf;
        }

        [OneTimeSetUp]
        public void SetUp()
        {
            m_Service = new HttpHostingService();
            var dirName = Path.GetRandomFileName();
            m_ContentRoot = Path.Combine(Path.GetTempPath(), dirName);
            Assert.IsNotEmpty(m_ContentRoot);
            Directory.CreateDirectory(m_ContentRoot);
            m_Service.HostingServiceContentRoots.Add(m_ContentRoot);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            m_Service.StopHostingService();

            if (!string.IsNullOrEmpty(m_ContentRoot) && Directory.Exists(m_ContentRoot))
                Directory.Delete(m_ContentRoot, true);
        }

        [TestCase("subdir", "subdir1")] //"subdir3")]
        [TestCase("subdír☠", "subdirãúñ", TestName = "ShouldServeFilesWSpecialCharacters")] //"subdirü", 
        public void ShouldServeRequestedFiles(string subdir1, string subdir2) // string subdir3)
        {
            var fileNames = new[]
            {
                Path.GetRandomFileName(),
                Path.Combine(subdir1, Path.GetRandomFileName()),
                Path.Combine(subdir2, Path.GetRandomFileName())
            };

            foreach (var fileName in fileNames)
            {
                var filePath = Path.Combine(m_ContentRoot, fileName);
                var bytes = GetRandomBytes(1024);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                File.WriteAllBytes(filePath, bytes);
                m_Service.StartHostingService();
                Assert.IsTrue(m_Service.IsHostingServiceRunning);
                var url = string.Format("http://127.0.0.1:{0}/{1}", m_Service.HostingServicePort, fileName);
                try
                {
                    var data = m_Client.DownloadData(url);
                    Assert.AreEqual(data.Length, bytes.Length);
                    for (var i = 0; i < data.Length; i++)
                        if (bytes[i] != data[i])
                            Assert.Fail("Data does not match {0} != {1}", bytes[i], data[i]);
                }
                catch (Exception e)
                {
                    Assert.Fail(e.Message);
                }
            }
        }
        
        [Test]
        public void HttpServiceCompletesWithUploadSpeedWhenExpected()
        {
            string subdir1 = "subdir";
            //string subdir2 = "subdir1"; // Remove comment when Mono limit Fixed
            //string subdir3 = "subdir3";
            
            var fileNames = new[]
            {
                Path.GetRandomFileName(),
                Path.Combine(subdir1, Path.GetRandomFileName()),
                //Path.Combine(subdir2, Path.Combine(subdir3, Path.GetRandomFileName())) // Remove comment when Mono limit Fixed.
                //Path.Combine(subdir3, Path.GetRandomFileName()) // Remove when Mono Fix
            };

            foreach (var fileName in fileNames)
            {
                var filePath = Path.Combine(m_ContentRoot, fileName);
                var bytes = GetRandomBytes(1024);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                File.WriteAllBytes(filePath, bytes);
                m_Service.StartHostingService();
                m_Service.UploadSpeed = 1024 * 1024 * 10;
                
                Assert.IsTrue(m_Service.IsHostingServiceRunning);
                var url = string.Format("http://127.0.0.1:{0}/{1}", m_Service.HostingServicePort, fileName);
                try
                {
                    int preOpCount = m_Service.ActiveOperations.Count;
                    m_Client.DownloadDataAsync(new Uri(url));
                    while (m_Service.ActiveOperations.Count == preOpCount)
                    {
                        // wait until the client has communicated with the service
                        Thread.Sleep(1);
                    }

                    var half = (bytes.Length + 1) / 2;
                    m_Service.Update(1, half);
                    Assert.AreEqual(m_Service.ActiveOperations.Count, 1);
                    m_Service.Update(1, half);
                    Assert.AreEqual(m_Service.ActiveOperations.Count, 0);

                    while (m_Client.IsBusy)
                    {
                        // wait until the client has completed
                        Thread.Sleep(1);
                    }
                }
                catch (Exception e)
                {
                    Assert.Fail(e.Message);
                }
            }
        }

        [Test]
        public void ShouldRespondWithStatus404IfFileDoesNotExist()
        {
            m_Service.StartHostingService();
            Assert.IsTrue(m_Service.IsHostingServiceRunning);
            var url = string.Format("http://127.0.0.1:{0}/{1}", m_Service.HostingServicePort, "foo");
            try
            {
                m_Client.DownloadData(url);
            }
            catch (WebException e)
            {
                var response = (HttpWebResponse)e.Response;
                Assert.AreEqual(response.StatusCode, HttpStatusCode.NotFound);
            }
            catch (Exception)
            {
                Assert.Fail();
            }
        }

        // StartHostingService

        [Test]
        public void StartHostingServiceShould_AssignPortIfUnassigned()
        {
            m_Service.StartHostingService();
            Assert.Greater(m_Service.HostingServicePort, 0);
        }

        // OnBeforeSerialize

        [Test]
        public void OnBeforeSerializeShould_PersistExpectedDataToKeyDataStore()
        {
            m_Service.StartHostingService();
            var port = m_Service.HostingServicePort;
            var data = new KeyDataStore();
            m_Service.OnBeforeSerialize(data);
            Assert.AreEqual(port, data.GetData("HostingServicePort", 0));
        }
        
        [Test]
        public void OnBeforeSerializeShould_WasEnableCorrectToKeyDataStore()
        {
            m_Service.StartHostingService();
            var data = new KeyDataStore();
            m_Service.OnDisable();
            m_Service.OnBeforeSerialize(data);
            Assert.IsTrue(data.GetData("IsEnabled", false), "Hosting server was started before shutting down. IsEnabled expected to be true");
        }

        // OnAfterDeserialize

        [Test]
        public void OnAfterDeserializeShould_RestoreExpectedDataFromKeyDataStore()
        {
            var data = new KeyDataStore();
            data.SetData("HostingServicePort", 1234);
            m_Service.OnAfterDeserialize(data);
            Assert.AreEqual(1234, m_Service.HostingServicePort);
        }

        // ResetListenPort

        [Test]
        public void ResetListenPortShould_AssignTheGivenPort()
        {
            m_Service.ResetListenPort(1234);
            Assert.AreEqual(1234, m_Service.HostingServicePort);
        }

        [Test]
        public void ResetListenPortShould_AssignRandomPortIfZero()
        {
            m_Service.ResetListenPort();
            m_Service.StartHostingService();
            Assert.Greater(m_Service.HostingServicePort, 0);
        }

        [Test]
        public void ResetListenPortShouldNot_StartServiceIfItIsNotRunning()
        {
            m_Service.StopHostingService();
            m_Service.ResetListenPort();
            Assert.IsFalse(m_Service.IsHostingServiceRunning);
        }

        [Test]
        public void ResetListenPortShould_RestartServiceIfRunning()
        {
            m_Service.StartHostingService();
            m_Service.ResetListenPort();
            Assert.IsTrue(m_Service.IsHostingServiceRunning);
        }
    }
}
