using UnityEngine;
using Dest.Math;
using DateTime = System.DateTime;

namespace Dest.Math.Tests
{
	public class Test_Matrix4x4Ex : MonoBehaviour
	{
		private DateTime now;
		private Plane3 plane;
		private GameObject shadowObject;

		public MeshFilter MF;
		public MeshRenderer MR;
		public Light LightSource;
		
		private void Start()
		{
			//TestCorrectness();
			//TestPerformance();

			plane = new Plane3(Vector3.up, 0f);
		}

		private void Update()
		{
			Matrix4x4 shadow;
			Vector4 L;
			if (LightSource.type == LightType.Directional)
			{
				L = -LightSource.transform.forward;
				L.w = 0;
			}
			else
			{
				L = LightSource.transform.position;
				L.w = 1;
			}
			Matrix4x4ex.CreateShadow(plane, L, out shadow);
			Graphics.DrawMesh(MF.sharedMesh, shadow * MF.transform.localToWorldMatrix , MR.sharedMaterial, 0);
		}

		private void LogCorrectness(bool value, string log)
		{
			if (value)
			{
				Logger.LogWarning(log);
			}
			else
			{
				Logger.LogError(log);
			}
		}

		private void BeginTest()
		{
			now = DateTime.Now;
		}

		private double EndTest()
		{
			var diff = DateTime.Now - now;
			return diff.TotalSeconds;
		}

		private void TestCorrectness()
		{
			Vector3 position = new Vector3(1f, 2f, 3f);
			Vector3 position2 = new Vector3(5f, 4f, 3f);
			Vector3 axis = new Vector3(3f, 2f, 1f);
			Vector3 scale = new Vector3(3f, 2f, 5f);
			Vector4 vec = new Vector4(1f, 2f, 3f, 4f);
			Quaternion quat = Quaternion.AngleAxis(78, axis);
			Matrix4x4 matr = Matrix4x4.TRS(position, quat, Vector3.one);
			Matrix4x4 matr1 = Matrix4x4.TRS(-position, quat, position);
			float angle = 45f;

			Matrix4x4 trans0 = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one);
			Matrix4x4 trans1;
			Matrix4x4ex.CreateTranslation(position, out trans1);
			bool transEqual = trans0 == trans1;
			Logger.LogInfo(trans0);
			Logger.LogInfo(trans1);
			LogCorrectness(transEqual, "transEqual: " + transEqual);

			Matrix4x4 scale0 = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
			Matrix4x4 scale1;
			Matrix4x4ex.CreateScale(scale, out scale1);
			bool scaleEqual = scale0 == scale1;
			Logger.LogInfo(scale0);
			Logger.LogInfo(scale1);
			LogCorrectness(scaleEqual, "scaleEqual: " + scaleEqual);

			Matrix4x4 rotation0 = Matrix4x4.TRS(Vector3.zero, quat, Vector3.one);
			Matrix4x4 rotation1;
			Matrix4x4ex.QuaternionToRotationMatrix(quat, out rotation1);
			bool rotationConversionEqual = rotation0 == rotation1;
			Logger.LogInfo(rotation0);
			Logger.LogInfo(rotation1);
			LogCorrectness(rotationConversionEqual, "rotationConversionEqual: " + rotationConversionEqual);

			rotation0 = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(angle, axis), Vector3.one);
			Matrix4x4ex.CreateRotationAngleAxis(angle, axis, out rotation1);
			bool angleAxisEqual = rotation0 == rotation1;
			Logger.LogInfo(rotation0);
			Logger.LogInfo(rotation1);
			LogCorrectness(angleAxisEqual, "angleAxisEqual: " + angleAxisEqual);

			rotation0 = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(angle, Vector3.right), Vector3.one);
			Matrix4x4ex.CreateRotationX(angle, out rotation1);
			bool rotXEqual = rotation0 == rotation1;
			Logger.LogInfo(rotation0);
			Logger.LogInfo(rotation1);
			LogCorrectness(rotXEqual, "rotXEqual: " + rotXEqual);

			rotation0 = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(angle, Vector3.up), Vector3.one);
			Matrix4x4ex.CreateRotationY(angle, out rotation1);
			bool rotYEqual = rotation0 == rotation1;
			Logger.LogInfo(rotation0);
			Logger.LogInfo(rotation1);
			LogCorrectness(rotYEqual, "rotYEqual: " + rotYEqual);

			rotation0 = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(angle, Vector3.forward), Vector3.one);
			Matrix4x4ex.CreateRotationZ(angle, out rotation1);
			bool rotZEqual = rotation0 == rotation1;
			Logger.LogInfo(rotation0);
			Logger.LogInfo(rotation1);
			LogCorrectness(rotZEqual, "rotZEqual: " + rotZEqual);

			rotation0 =
				Matrix4x4.TRS(position2, Quaternion.identity, Vector3.one) *
				Matrix4x4.TRS(Vector3.zero, quat, Vector3.one) *
				Matrix4x4.TRS(-position2, Quaternion.identity, Vector3.one);
			Matrix4x4ex.CreateRotation(position2, quat, out rotation1);
			bool rotAroundEqual = rotation0 == rotation1;
			Logger.LogInfo(rotation0);
			Logger.LogInfo(rotation1);
			LogCorrectness(rotAroundEqual, "rotAroundEqual: " + rotAroundEqual);

			Matrix4x4 m0 = rotation0.transpose;
			Matrix4x4 m1;
			Matrix4x4ex.Transpose(ref rotation0, out m1);
			bool transpEqual = m0 == m1;
			Logger.LogInfo(m0);
			Logger.LogInfo(m1);
			LogCorrectness(transpEqual, "transpEqual: " + transpEqual);

			m0 = matr.inverse;
			Matrix4x4ex.Inverse(ref matr, out m1);
			bool invEqual = m0 == m1;
			Logger.LogInfo(m0);
			Logger.LogInfo(m1);
			LogCorrectness(invEqual, "invEqual: " + invEqual);

			m0 = matr * matr1;
			Matrix4x4ex.Multiply(ref matr, ref matr1, out m1);
			bool matrMultEqual = m0 == m1;
			Logger.LogInfo(m0);
			Logger.LogInfo(m1);
			LogCorrectness(matrMultEqual, "matrMultEqual: " + matrMultEqual);

			m0 = matr * matr1;
			m1 = matr;
			Matrix4x4ex.MultiplyRight(ref m1, ref matr1);
			bool matrMultRightEqual = m0 == m1;
			Logger.LogInfo(m0);
			Logger.LogInfo(m1);
			LogCorrectness(matrMultRightEqual, "matrMultRightEqual: " + matrMultRightEqual);

			m0 = matr1 * matr;
			m1 = matr;
			Matrix4x4ex.MultiplyLeft(ref m1, ref matr1);
			bool matrMultLeftEqual = m0 == m1;
			Logger.LogInfo(m0);
			Logger.LogInfo(m1);
			LogCorrectness(matrMultLeftEqual, "matrMultLeftEqual: " + matrMultLeftEqual);

			Vector4 v0 = matr * vec;
			Vector4 v1 = Matrix4x4ex.Multiply(ref matr, vec);
			bool vecMultEqual = v0 == v1;
			Logger.LogInfo(v0);
			Logger.LogInfo(v1);
			LogCorrectness(vecMultEqual, "vecMultEqual: " + vecMultEqual);

			m0 = Matrix4x4.TRS(position, quat, scale);
			Matrix4x4ex.CreateSRT(scale, quat, position, out m1);
			bool srt = m0 == m1;
			Logger.LogInfo(m0);
			Logger.LogInfo(m1);
			LogCorrectness(srt, "srt: " + srt);

			m0 =
				Matrix4x4.TRS(position, Quaternion.identity, Vector3.one) *
				Matrix4x4.TRS(position2, Quaternion.identity, Vector3.one) *
				Matrix4x4.TRS(Vector3.zero, quat, Vector3.one) *
				Matrix4x4.TRS(-position2, Quaternion.identity, Vector3.one);
			Matrix4x4ex.CreateRT(position2, quat, position, out m1);
			bool rtOrigin = m0 == m1;
			Logger.LogInfo(m0);
			Logger.LogInfo(m1);
			LogCorrectness(rtOrigin, "rtOrigin: " + rtOrigin);

			m0 =
				Matrix4x4.TRS(position, Quaternion.identity, Vector3.one) *
				Matrix4x4.TRS(position2, Quaternion.identity, Vector3.one) *
				Matrix4x4.TRS(Vector3.zero, quat, Vector3.one) *
				Matrix4x4.TRS(-position2, Quaternion.identity, Vector3.one) *
				Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
			Matrix4x4ex.CreateSRT(scale, position2, quat, position, out m1);
			bool srtOrigin = m0 == m1;
			Logger.LogInfo(m0);
			Logger.LogInfo(m1);
			LogCorrectness(srtOrigin, "srtOrigin: " + srtOrigin);

			m0 = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one) * Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
			Matrix4x4ex.CreateST(scale, position, out m1);
			bool st = m0 == m1;
			Logger.LogInfo(m0);
			Logger.LogInfo(m1);
			LogCorrectness(st, "st: " + st);
		}

		/*private void TestPerformance()
		{
			int iter = 1000000;
			Vector3 position = new Vector3(1f, 2f, 3f);
			Vector3 position2 = new Vector3(5f, 4f, 3f);
			Vector3 scale = new Vector3(1f, 2f, 3f);
			Vector4 vec = new Vector4(1f, 2f, 3f, 4f);
			Quaternion quat = Quaternion.AngleAxis(78, position);
			Matrix4x4 matr = Matrix4x4.TRS(position, quat, Vector3.one);
			Matrix4x4 matr1 = Matrix4x4.TRS(-position, quat, position);

			System.Text.StringBuilder sb = new System.Text.StringBuilder();

			BeginTest();
			for (int i = 0; i < iter; ++i)
			{
				Matrix4x4 m = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one);
			}
			sb.AppendLine("Unity Translation: " + EndTest());
			BeginTest();
			for (int i = 0; i < iter; ++i)
			{
				Matrix4x4 m;
				Matrix4x4Ex.CreateTranslation(position, out m);
			}
			sb.AppendLine("Dest Translation: " + EndTest());
			sb.AppendLine();

			BeginTest();
			for (int i = 0; i < iter; ++i)
			{
				Matrix4x4 m = matr.transpose;
			}
			sb.AppendLine("Unity Transpose: " + EndTest());
			BeginTest();
			for (int i = 0; i < iter; ++i)
			{
				Matrix4x4 m;
				Matrix4x4Ex.Transpose(ref matr, out m);
			}
			sb.AppendLine("Dest Transpose: " + EndTest());
			sb.AppendLine();

			BeginTest();
			for (int i = 0; i < iter; ++i)
			{
				Matrix4x4 m = matr.inverse;
			}
			sb.AppendLine("Unity Inverse: " + EndTest());
			BeginTest();
			for (int i = 0; i < iter; ++i)
			{
				Matrix4x4 m;
				Matrix4x4Ex.Inverse(ref matr, out m);
			}
			sb.AppendLine("Dest Inverse: " + EndTest());
			sb.AppendLine();

			BeginTest();
			for (int i = 0; i < iter; ++i)
			{
				Matrix4x4 m = matr * matr1;
			}
			sb.AppendLine("Unity MatMult: " + EndTest());
			BeginTest();
			for (int i = 0; i < iter; ++i)
			{
				Matrix4x4 m;
				Matrix4x4Ex.Multiply(ref matr, ref matr1, out m);
			}
			sb.AppendLine("Dest MatMult: " + EndTest());
			BeginTest();
			for (int i = 0; i < iter; ++i)
			{
				Matrix4x4 m = matr1;
				Matrix4x4Ex.MultiplyLeft(ref m, ref matr);
			}
			sb.AppendLine("Dest MatMult2: " + EndTest());
			sb.AppendLine();

			BeginTest();
			for (int i = 0; i < iter; ++i)
			{
				Matrix4x4 m = matr;
			}
			sb.AppendLine("Unity MatCopy: " + EndTest());
			BeginTest();
			for (int i = 0; i < iter; ++i)
			{
				Matrix4x4 m;
				Matrix4x4Ex.CopyMatrix(ref matr, out m);
			}
			sb.AppendLine("Dest MatCopy: " + EndTest());
			sb.AppendLine();

			BeginTest();
			for (int i = 0; i < iter; ++i)
			{
				Vector4 v = matr * vec;
			}
			sb.AppendLine("Unity VecMult: " + EndTest());
			BeginTest();
			for (int i = 0; i < iter; ++i)
			{
				Vector4 v = Matrix4x4Ex.Multiply(ref matr, vec);
			}
			sb.AppendLine("Dest VecMult: " + EndTest());
			sb.AppendLine();

			BeginTest();
			for (int i = 0; i < iter; ++i)
			{
				Matrix4x4 m = Matrix4x4.TRS(position, quat, scale);
			}
			sb.AppendLine("Unity TRS: " + EndTest());
			BeginTest();
			for (int i = 0; i < iter; ++i)
			{
				Matrix4x4 m;
				Matrix4x4Ex.CreateSRT(scale, quat, position, out m);
			}
			sb.AppendLine("Dest SRT: " + EndTest());
			BeginTest();
			for (int i = 0; i < iter; ++i)
			{
				Matrix4x4 m;
				Matrix4x4Ex.CreateSRT(ref scale, ref quat, ref position, out m);
			}
			sb.AppendLine("Dest SRTref: " + EndTest());
			BeginTest();
			for (int i = 0; i < iter; ++i)
			{
				Matrix4x4 m;
				Matrix4x4Ex.CreateRT(quat, position, out m);
			}
			sb.AppendLine("Dest RT: " + EndTest());
			BeginTest();
			for (int i = 0; i < iter; ++i)
			{
				Matrix4x4 m;
				Matrix4x4Ex.CreateRT(ref quat, ref position, out m);
			}
			sb.AppendLine("Dest RTref: " + EndTest());
			BeginTest();
			for (int i = 0; i < iter; ++i)
			{
				Matrix4x4 m;
				Matrix4x4Ex.CreateRT(position2, quat, position, out m);
			}
			sb.AppendLine("Dest RTori: " + EndTest());
			BeginTest();
			for (int i = 0; i < iter; ++i)
			{
				Matrix4x4 m;
				Matrix4x4Ex.CreateRT(ref position2, ref quat, ref position, out m);
			}
			sb.AppendLine("Dest RToriref: " + EndTest());
			BeginTest();
			for (int i = 0; i < iter; ++i)
			{
				Matrix4x4 m;
				Matrix4x4Ex.CreateSRT(scale, position2, quat, position, out m);
			}
			sb.AppendLine("Dest SRTori: " + EndTest());
			BeginTest();
			for (int i = 0; i < iter; ++i)
			{
				Matrix4x4 m;
				Matrix4x4Ex.CreateSRT(ref scale, ref position2, ref quat, ref position, out m);
			}
			sb.AppendLine("Dest SRToriref: " + EndTest());
			BeginTest();
			for (int i = 0; i < iter; ++i)
			{
				Matrix4x4 m;
				Matrix4x4Ex.CreateST(scale, position, out m);
			}
			sb.AppendLine("Dest ST: " + EndTest());
			BeginTest();
			for (int i = 0; i < iter; ++i)
			{
				Matrix4x4 m;
				Matrix4x4Ex.CreateST(ref scale, ref position, out m);
			}
			sb.AppendLine("Dest STref: " + EndTest());
			sb.AppendLine();

			Vector4 LPoi = new Vector4(1, 2, 3, 1);
			BeginTest();
			for (int i = 0; i < iter; ++i)
			{
				Matrix4x4 m;
				Matrix4x4Ex.CreateShadowDirectional(plane, position, out m);
			}
			sb.AppendLine("Dest ShadowDir: " + EndTest());
			BeginTest();
			for (int i = 0; i < iter; ++i)
			{
				Matrix4x4 m;
				Matrix4x4Ex.CreateShadowDirectional(ref plane, ref position, out m);
			}
			sb.AppendLine("Dest ShadowDirref: " + EndTest());
			BeginTest();
			for (int i = 0; i < iter; ++i)
			{
				Matrix4x4 m;
				Matrix4x4Ex.CreateShadowPoint(plane, position, out m);
			}
			sb.AppendLine("Dest ShadowPoi: " + EndTest());
			BeginTest();
			for (int i = 0; i < iter; ++i)
			{
				Matrix4x4 m;
				Matrix4x4Ex.CreateShadowPoint(ref plane, ref position, out m);
			}
			sb.AppendLine("Dest ShadowPoiref: " + EndTest());
			BeginTest();
			for (int i = 0; i < iter; ++i)
			{
				Matrix4x4 m;
				Matrix4x4Ex.CreateShadow(plane, LPoi, out m);
			}
			sb.AppendLine("Dest ShadowGen: " + EndTest());
			BeginTest();
			for (int i = 0; i < iter; ++i)
			{
				Matrix4x4 m;
				Matrix4x4Ex.CreateShadow(ref plane, ref LPoi, out m);
			}
			sb.AppendLine("Dest ShadowGenref: " + EndTest());
			sb.AppendLine();

			Logger.LogWarning(sb.ToString());
		}
		*/
	}
}
