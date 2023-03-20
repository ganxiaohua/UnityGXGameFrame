using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_CubicSpline3 : Test_Base
	{
		private CurveFrame[] _frames;
		private float[]      _curvatures;

		public CubicSpline3 Spline;
		public int          ParametrizationCount;
		public bool         DrawTangents;
		public bool         DrawNormals;
		public bool         DrawCurvatures = true;
		[Range(0, 99)] // Change this to ParametrizationCount-1
		public int          CurvatureIndex;

		private void Update()
		{
			Transform tr = Spline.transform;
			_frames = new CurveFrame[ParametrizationCount];
			_curvatures = new float[ParametrizationCount];
			for (int i = 0; i < ParametrizationCount; ++i)
			{
				float t = (float)i / (ParametrizationCount - 1);
				Spline.EvalFrame(t, out _frames[i]);
				_frames[i].Position = tr.TransformPoint(_frames[i].Position);
				_curvatures[i] = Spline.EvalCurvature(t);
			}
		}

		private void OnDrawGizmos()
		{
			if (_frames != null)
			{
				for (int i = 0; i < ParametrizationCount; ++i)
				{
					Vector3 pos = _frames[i].Position;
					
					Gizmos.color = Color.red;
					if (DrawTangents) Gizmos.DrawLine(pos, pos + _frames[i].Tangent * .5f);

					Gizmos.color = Color.green;
					if (DrawNormals) Gizmos.DrawLine(pos, pos + _frames[i].Normal * .5f);

					Gizmos.color = Color.white;
					Gizmos.DrawWireSphere(pos, .05f);
				}

				if (DrawCurvatures)
				{
					Gizmos.color = Color.yellow;
					CurveFrame frame = _frames[CurvatureIndex];

					float rad = 1f / _curvatures[CurvatureIndex];
					Circle3 circle = new Circle3(frame.Position + frame.Normal * rad, frame.Binormal, rad);
					DrawCircle(ref circle, 100);
				}
			}
		}
	}
}
