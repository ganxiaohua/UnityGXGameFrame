namespace UnityEngine.ResourceManagement.Util
{
    internal class PlatformUtilities
    {
        internal static bool PlatformUsesMultiThreading(RuntimePlatform platform)
        {
            return platform != RuntimePlatform.WebGLPlayer;
        }
    }
}
