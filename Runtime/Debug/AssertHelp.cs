using System.Diagnostics;

namespace GameFrame
{
    public class Assert
    {
#if !SHOWASSERT
        private const string SHOWASSERT = "NOSHOWASSERT";
#elif SHOWASSERT
        private const string SHOWASSERT = "UNITY_EDITOR";
#endif

        [Conditional(SHOWASSERT)]
        public static void IsNotNull(object obj, string explain)
        {
            UnityEngine.Assertions.Assert.IsNotNull(obj, explain);
        }

        [Conditional(SHOWASSERT)]
        public static void IsNull(object obj, string explain)
        {
            UnityEngine.Assertions.Assert.IsNotNull(obj, explain);
        }

        [Conditional(SHOWASSERT)]
        public static void IsTrue(bool b, string explain)
        {
            UnityEngine.Assertions.Assert.IsTrue(b, explain);
        }

        [Conditional(SHOWASSERT)]
        public static void IsFalse(bool b, string explain)
        {
            UnityEngine.Assertions.Assert.IsFalse(b, explain);
        }

        [Conditional(SHOWASSERT)]
        public static void AreNotEqual(int expected, int actual, string explain)
        {
            UnityEngine.Assertions.Assert.AreNotEqual(expected, actual, explain);
        }
    }
}