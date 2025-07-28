using System.Diagnostics;

namespace GameFrame.Runtime
{
    public class Assert
    {
#if !ShowAssert
        private const string ShowAssert = "NotShowAssert";
#elif ShowAssert
        private const string ShowAssert = "UNITY_EDITOR";
#endif

        [Conditional(ShowAssert)]
        public static void IsNotNull(object obj, string explain)
        {
            UnityEngine.Assertions.Assert.IsNotNull(obj, explain);
        }

        [Conditional(ShowAssert)]
        public static void IsNull(object obj, string explain)
        {
            UnityEngine.Assertions.Assert.IsNull(obj, explain);
        }

        [Conditional(ShowAssert)]
        public static void IsTrue(bool b, string explain)
        {
            UnityEngine.Assertions.Assert.IsTrue(b, explain);
        }

        [Conditional(ShowAssert)]
        public static void IsFalse(bool b, string explain)
        {
            UnityEngine.Assertions.Assert.IsFalse(b, explain);
        }

        [Conditional(ShowAssert)]
        public static void AreNotEqual(int expected, int actual, string explain)
        {
            UnityEngine.Assertions.Assert.AreNotEqual(expected, actual, explain);
        }
        
        [Conditional(ShowAssert)]
        public static void AreEqual<T>(T expected, T actual, string message)
        {
            UnityEngine.Assertions.Assert.AreEqual(expected, actual, message);
        }
    }
}