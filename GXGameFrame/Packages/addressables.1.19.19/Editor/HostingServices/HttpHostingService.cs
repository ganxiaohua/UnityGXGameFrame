using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UnityEditor.AddressableAssets.HostingServices
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    /// <summary>
    /// HTTP implementation of hosting service.
    /// </summary>
    public class HttpHostingService : BaseHostingService
    {
        /// <summary>
        /// Options for standard Http result codes
        /// </summary>
        protected enum ResultCode
        {
            /// <summary>
            /// Use to indicate that the request succeeded.
            /// </summary>
            Ok = 200,
            /// <summary>
            /// Use to indicate that the requested resource could not be found.
            /// </summary>
            NotFound = 404
        }

        internal class FileUploadOperation
        {
            HttpListenerContext m_Context;
            byte[] m_ReadByteBuffer;
            FileStream m_ReadFileStream;
            long m_TotalBytesRead;
            bool m_IsDone = false;
            public bool IsDone => m_IsDone;

            public FileUploadOperation(HttpListenerContext context, string filePath)
            {
                m_Context = context;
                m_Context.Response.ContentType = "application/octet-stream";

                m_ReadByteBuffer = new byte[k_FileReadBufferSize];
                try
                {
                    m_ReadFileStream = File.OpenRead(filePath);
                }
                catch (Exception e)
                {
                    m_IsDone = true;
                    Debug.LogException(e);
                    throw;
                }
                m_Context.Response.ContentLength64 = m_ReadFileStream.Length;
            }

            public void Update(double diff, int bytesPerSecond)
            {
                if (m_Context == null || m_ReadFileStream == null)
                    return;

                int countToRead = (int)(bytesPerSecond * diff);

                try
                {
                    while (countToRead > 0)
                    {
                        int count = countToRead > m_ReadByteBuffer.Length ? m_ReadByteBuffer.Length : countToRead;
                        int read = m_ReadFileStream.Read(m_ReadByteBuffer, 0, count);
                        m_Context.Response.OutputStream.Write(m_ReadByteBuffer, 0, read);
                        m_TotalBytesRead += read;
                        countToRead -= count;

                        if (m_TotalBytesRead == m_ReadFileStream.Length)
                        {
                            Stop();
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    string url = m_Context.Request.Url.ToString();
                    Stop();
                    if (e.InnerException != null && e.InnerException is SocketException &&
                        e.InnerException.Message == "The socket has been shut down")
                    {
                        Addressables.LogWarning($"Connection lost: {url}. The socket has been shut down.");
                    }
                    else
                    {
                        Addressables.LogException(e);
                        throw;
                    }
                }
            }

            public void Stop()
            {
                if (m_IsDone)
                {
                    Debug.LogError("FileUploadOperation has already completed.");
                    return;
                }

                m_IsDone = true;
                m_ReadFileStream.Dispose();
                m_ReadFileStream = null;
                m_Context.Response.OutputStream.Close();
                m_Context = null;
            }
        }

        const string k_HostingServicePortKey = "HostingServicePort";
        const int k_FileReadBufferSize = 64 * 1024;

        private const int k_OneGBPS = 1024 * 1024 * 1024;
        const string k_UploadSpeedKey = "HostingServiceUploadSpeed";
        int m_UploadSpeed;
        double m_LastFrameTime;
        List<FileUploadOperation> m_ActiveUploads = new List<FileUploadOperation>();

        static readonly IPEndPoint k_DefaultLoopbackEndpoint = new IPEndPoint(IPAddress.Loopback, 0);
        int m_ServicePort;
        readonly List<string> m_ContentRoots;
        readonly Dictionary<string, string> m_ProfileVariables;

        GUIContent m_UploadSpeedGUI =
            new GUIContent("Upload Speed (Kb/s)", "Speed in Kb/s the hosting service will upload content. 0 for no limit");

        // ReSharper disable once MemberCanBePrivate.Global
        /// <summary>
        /// The actual Http listener used by this service
        /// </summary>
        protected HttpListener MyHttpListener { get; set; }

        /// <summary>
        /// The port number on which the service is listening
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public int HostingServicePort
        {
            get
            {
                return m_ServicePort;
            }
            protected set
            {
                if (value > 0)
                    m_ServicePort = value;
            }
        }

        /// <summary>
        /// The upload speed that files were be served at, in kbps
        /// </summary>
        public int UploadSpeed
        {
            get => m_UploadSpeed;
            set => m_UploadSpeed = value > 0 ? value > int.MaxValue / 1024 ? int.MaxValue / 1024 : value : 0;
        }

        /// <summary>
        /// Files that are currently being uploaded
        /// </summary>
        internal List<FileUploadOperation> ActiveOperations => m_ActiveUploads;

        /// <inheritdoc/>
        public override bool IsHostingServiceRunning
        {
            get { return MyHttpListener != null && MyHttpListener.IsListening; }
        }

        /// <inheritdoc/>
        public override List<string> HostingServiceContentRoots
        {
            get { return m_ContentRoots; }
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> ProfileVariables
        {
            get
            {
                m_ProfileVariables[k_HostingServicePortKey] = HostingServicePort.ToString();
                m_ProfileVariables[DisambiguateProfileVar(k_HostingServicePortKey)] = HostingServicePort.ToString();
                return m_ProfileVariables;
            }
        }

        /// <summary>
        /// Create a new <see cref="HttpHostingService"/>
        /// </summary>
        public HttpHostingService()
        {
            m_ProfileVariables = new Dictionary<string, string>();
            m_ContentRoots = new List<string>();
            MyHttpListener = new HttpListener();
        }

        /// <summary>
        /// Destroys a <see cref="HttpHostingService"/>
        /// </summary>
        ~HttpHostingService()
        {
            StopHostingService();
        }

        /// <inheritdoc/>
        public override void StartHostingService()
        {
            if (IsHostingServiceRunning)
                return;

            if (HostingServicePort <= 0)
            {
                HostingServicePort = GetAvailablePort();
                if (HostingServicePort == 0)
                {
                    LogError("Failed to get an available port, cannot start service!");
                    return;
                }
            }
            else if (!IsPortAvailable(HostingServicePort))
            {
                LogError("Port {0} is in use, cannot start service!", HostingServicePort);
                return;
            }

            if (HostingServiceContentRoots.Count == 0)
            {
                throw new Exception(
                    "ContentRoot is not configured; cannot start service. This can usually be fixed by modifying the BuildPath for any new groups and/or building content.");
            }

            ConfigureHttpListener();
            MyHttpListener.Start();
            MyHttpListener.BeginGetContext(HandleRequest, null);
            EditorApplication.update += EditorUpdate;

            var count = HostingServiceContentRoots.Count;
            Log("Started. Listening on port {0}. Hosting {1} folder{2}.", HostingServicePort, count, count > 1 ? "s" : string.Empty);
            foreach (var root in HostingServiceContentRoots)
            {
                Log("Hosting : {0}", root);
            }
        }

        private void EditorUpdate()
        {
            if (m_LastFrameTime == 0)
                m_LastFrameTime = EditorApplication.timeSinceStartup - Time.unscaledDeltaTime;
            double diff = EditorApplication.timeSinceStartup - m_LastFrameTime;
            int speed = m_UploadSpeed * 1024;
            int bps = speed > 0 ? speed : k_OneGBPS;
            Update(diff, bps);
            m_LastFrameTime = EditorApplication.timeSinceStartup;
        }

        internal void Update(double deltaTime, int bytesPerSecond)
        {
            for (int i = m_ActiveUploads.Count - 1; i >= 0; --i)
            {
                m_ActiveUploads[i].Update(deltaTime, bytesPerSecond);
                if (m_ActiveUploads[i].IsDone)
                    m_ActiveUploads.RemoveAt(i);
            }
        }

        /// <summary>
        /// Temporarily stops the service from receiving requests.
        /// </summary>
        public override void StopHostingService()
        {
            if (!IsHostingServiceRunning) return;
            Log("Stopping");
            MyHttpListener.Stop();
            // Abort() is the method we want instead of Close(), because the former frees up resources without
            // disposing the object.
            MyHttpListener.Abort();

            EditorApplication.update -= EditorUpdate;
            foreach (FileUploadOperation operation in m_ActiveUploads)
                operation.Stop();
            m_ActiveUploads.Clear();
        }

        /// <inheritdoc/>
        public override void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            {
                var newPort = EditorGUILayout.DelayedIntField("Port", HostingServicePort);
                if (newPort != HostingServicePort)
                {
                    if (IsPortAvailable(newPort))
                    {
                        ResetListenPort(newPort);
                        var settings = AddressableAssetSettingsDefaultObject.Settings;
                        if (settings != null)
                            settings.SetDirty(AddressableAssetSettings.ModificationEvent.HostingServicesManagerModified, this, false, true);
                    }
                    else
                        LogError("Cannot listen on port {0}; port is in use", newPort);
                }

                if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false)))
                    ResetListenPort();

                //GUILayout.Space(rect.width / 2f);
            }
            EditorGUILayout.EndHorizontal();

            UploadSpeed = EditorGUILayout.IntField(m_UploadSpeedGUI, UploadSpeed);
        }

        /// <inheritdoc/>
        public override void OnBeforeSerialize(KeyDataStore dataStore)
        {
            dataStore.SetData(k_HostingServicePortKey, HostingServicePort);
            dataStore.SetData(k_UploadSpeedKey, m_UploadSpeed);
            base.OnBeforeSerialize(dataStore);
        }

        /// <inheritdoc/>
        public override void OnAfterDeserialize(KeyDataStore dataStore)
        {
            HostingServicePort = dataStore.GetData(k_HostingServicePortKey, 0);
            UploadSpeed = dataStore.GetData(k_UploadSpeedKey, 0);
            base.OnAfterDeserialize(dataStore);
        }

        /// <summary>
        /// Listen on a new port then next time the server starts. If the server is already running, it will be stopped
        /// and restarted automatically.
        /// </summary>
        /// <param name="port">Specify a port to listen on. Default is 0 to choose any open port</param>
        // ReSharper disable once MemberCanBePrivate.Global
        public void ResetListenPort(int port = 0)
        {
            var isRunning = IsHostingServiceRunning;
            StopHostingService();
            HostingServicePort = port;

            if (isRunning)
                StartHostingService();
        }

        /// <summary>
        /// Handles any configuration necessary for <see cref="MyHttpListener"/> before listening for connections.
        /// </summary>
        protected virtual void ConfigureHttpListener()
        {
            try
            {
                MyHttpListener.Prefixes.Clear();
                MyHttpListener.Prefixes.Add("http://+:" + HostingServicePort + "/");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// Asynchronous callback to handle a client connection request on <see cref="MyHttpListener"/>. This method is
        /// recursive in that it will call itself immediately after receiving a new incoming request to listen for the
        /// next connection.
        /// </summary>
        /// <param name="ar">Asynchronous result from previous request. Pass null to listen for an initial request</param>
        /// <exception cref="ArgumentOutOfRangeException">thrown when the request result code is unknown</exception>
        protected virtual void HandleRequest(IAsyncResult ar)
        {
            if (!IsHostingServiceRunning)
                return;

            var c = MyHttpListener.EndGetContext(ar);
            MyHttpListener.BeginGetContext(HandleRequest, null);

            var relativePath = c.Request.Url.LocalPath.Substring(1);

            var fullPath = FindFileInContentRoots(relativePath);
            var result = fullPath != null ? ResultCode.Ok : ResultCode.NotFound;
            var info = fullPath != null ? new FileInfo(fullPath) : null;
            var size = info != null ? info.Length.ToString() : "-";
            var remoteAddress = c.Request.RemoteEndPoint != null ? c.Request.RemoteEndPoint.Address : null;
            var timestamp = DateTime.Now.ToString("o");

            Log("{0} - - [{1}] \"{2}\" {3} {4}", remoteAddress, timestamp, fullPath, (int)result, size);

            switch (result)
            {
                case ResultCode.Ok:
                    ReturnFile(c, fullPath);
                    break;
                case ResultCode.NotFound:
                    Return404(c);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Searches for the given relative path within the configured content root directores.
        /// </summary>
        /// <param name="relativePath"></param>
        /// <returns>The full system path to the file if found, or null if file could not be found</returns>
        protected virtual string FindFileInContentRoots(string relativePath)
        {
            relativePath = relativePath.TrimStart('/');
            relativePath = relativePath.TrimStart('\\');
            foreach (var root in HostingServiceContentRoots)
            {
                var fullPath = Path.Combine(root, relativePath).Replace('\\','/');
                if (File.Exists(fullPath))
                    return fullPath;
            }

            return null;
        }

        /// <summary>
        /// Sends a file to the connected HTTP client
        /// </summary>
        /// <param name="context"></param>
        /// <param name="filePath"></param>
        /// <param name="readBufferSize"></param>
        protected virtual void ReturnFile(HttpListenerContext context, string filePath, int readBufferSize = k_FileReadBufferSize)
        {
            if (m_UploadSpeed > 0)
            {
                m_ActiveUploads.Add(new FileUploadOperation(context, filePath));
            }
            else
            {
                context.Response.ContentType = "application/octet-stream";

                var buffer = new byte[readBufferSize];
                using (var fs = File.OpenRead(filePath))
                {
                    context.Response.ContentLength64 = fs.Length;
                    int read;
                    while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
                        context.Response.OutputStream.Write(buffer, 0, read);
                }

                context.Response.OutputStream.Close();
            }
        }

        /// <summary>
        /// Sets the status code to 404 on the given <c>HttpListenerContext</c> object.
        /// </summary>
        /// <param name="context">The object to modify.</param>
        protected virtual void Return404(HttpListenerContext context)
        {
            context.Response.StatusCode = 404;
            context.Response.Close();
        }

        /// <summary>
        /// Tests to see if the given port # is already in use
        /// </summary>
        /// <param name="port">port number to test</param>
        /// <returns>true if there is not a listener on the port</returns>
        protected static bool IsPortAvailable(int port)
        {
            try
            {
                if (port <= 0)
                    return false;

                using (var client = new TcpClient())
                {
                    var result = client.BeginConnect(IPAddress.Loopback, port, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(500));
                    if (!success)
                        return true;

                    client.EndConnect(result);
                }
            }
            catch
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Find an open network listen port on the local system
        /// </summary>
        /// <returns>a system assigned port, or 0 if none are available</returns>
        protected static int GetAvailablePort()
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Bind(k_DefaultLoopbackEndpoint);

                var endPoint = socket.LocalEndPoint as IPEndPoint;
                return endPoint != null ? endPoint.Port : 0;
            }
        }
    }
}
