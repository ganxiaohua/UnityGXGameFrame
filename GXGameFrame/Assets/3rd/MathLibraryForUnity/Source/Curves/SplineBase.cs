using UnityEngine;
using System.Collections.Generic;
using SF = UnityEngine.SerializeField;
using NS = System.NonSerializedAttribute;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Dest.Math
{
	public enum SplineTypes
	{
		Open,
		Closed
	}

	public enum SplinePlaneTypes
	{
		XZ,
		XY,
		YZ,
	}

	public struct PositionTangent
	{
		public Vector3 Position;
		public Vector3 Tangent;
	}

	public struct CurveFrame
	{
		public Vector3 Position;
		public Vector3 Tangent;
		public Vector3 Normal;
		public Vector3 Binormal;
	}

	public abstract class SplineBase : MonoBehaviour
	{
		[System.Serializable]
		protected class ItemData
		{
			// Vertex data
			public Vector3 Position;

			// Segment data
			public Vector3 A;
			public Vector3 B;
			public Vector3 C;
			public Vector3 D;
			public float   Length;
			public float   AccumulatedLength;

			#region Editor

			private const int   SegmentCount      = 10; // Change this to affect visual appearance in the editor
			private const int   SegmentCountPlus1 = SegmentCount + 1;
			private const float DeltaTime         = 1f / SegmentCount;

			public Vector3[] RenderPoints;

			public bool EnsureRenderPointsValidity()
			{
				if (RenderPoints == null)
				{
					RenderPoints = new Vector3[SegmentCountPlus1];
					return true;
				}
				else if (RenderPoints.Length != SegmentCountPlus1)
				{
					RenderPoints = new Vector3[SegmentCountPlus1];
					return true;
				}
				return false;
			}

			public void UpdateRenderPoints()
			{
				float time = 0f;
				for (int i = 0; i <= SegmentCount; ++i)
				{
					RenderPoints[i] = EvalPosition(time);
					time += DeltaTime;
				}
			}

			#endregion

			public Vector3 EvalPosition(float t)
			{
				return A + t * (B + t * (C + t * D));
			}

			public Vector3 EvalFirstDerivative(float t)
			{
				return B + t * (C * 2f + D * (3f * t));
			}

			public Vector3 EvalSecondDerivative(float t)
			{
				return 2f * C + 6f * t * D;
			}

			public Vector3 EvalThirdDerivative(float t)
			{
				return 6f * D;
			}

			public float EvalSpeed(float t)
			{
				return (B + t * (C * 2f + D * (3f * t))).magnitude; // First derivative length
			}

			public Vector3 EvalTangent(float t)
			{
				return (B + t * (C * 2f + D * (3f * t))).normalized; // First derivative normalized
			}

			public float EvalLength()
			{
				return Integrator.GaussianQuadrature(EvalSpeed, 0f, 1f);
			}

			public float EvalLength(float t0, float t1)
			{
				return Integrator.GaussianQuadrature(EvalSpeed, t0, t1);
			}

			public float ProcessLength(float currentLength)
			{
				Length = EvalLength();
				AccumulatedLength = currentLength + Length;
				return AccumulatedLength;
			}
		}

		protected class ArcLengthParametrization
		{
			public float[] sSample;
			public float[] tSample;
			public float[] tsSlope;
			public float   L;

			public float GetApproximateTimeParameter(float s)
			{
				if (s <= 0f) return 0f;
				if (s >= L) return 1f;

				int i = System.Array.BinarySearch<float>(sSample, s);
				if (i < 0) i = ~i;

				float t = tSample[i - 1] + tsSlope[i] * (s - sSample[i - 1]);
				return t;
			}
		}

#if UNITY_EDITOR
		private static Vector3 RenderPointSize = new Vector3(.1f, .1f, .1f);
#endif

		protected ArcLengthParametrization _parametrization;

		[SF]protected List<ItemData>   _data = new List<ItemData>();
		[SF]protected SplineTypes      _type;
		[SF]protected bool             _recalcSegmentsLength = true;
		[SF]protected Color            _renderColor = Color.white;
		[SF]protected SplinePlaneTypes _creationPlane;


		protected int SegmentCount { get { return _type == SplineTypes.Open ? _data.Count - 1 : _data.Count; } }

		/// <summary>
		/// Gets spline vertex count
		/// </summary>
		public int VertexCount  { get { return _data.Count;  } }

		/// <summary>
		/// Returns true if spline is valid (i.e. contains 2 or more points) false otherwise
		/// </summary>
		public bool Valid { get { return _data.Count > 1; } }

		/// <summary>
		/// Gets or set spline type.
		/// </summary>
		public abstract SplineTypes SplineType { get; set; }

#if UNITY_EDITOR
		/// <summary>
		/// Gets or sets spline color in the editor.
		/// </summary>
		public Color RenderColor { get { return _renderColor; } set { _renderColor = value; } }

#else
		private void Awake()
		{
			// Get rid of all arrays which could have been created
			// for the rendering in the editor mode
			PrepareForRuntime();
		}
#endif

		/// <summary>
		/// Constructor is private to prevent user from calling it. Static methods must be used.
		/// </summary>
		protected SplineBase()
		{
		}


		protected void GetSegmentIndexAndTime(float time, out int segmentIndex, out float segmentTime)
		{
			if (time <= 0f)
			{
				segmentIndex = 0;
				segmentTime = 0f;
			}
			else if (time >= 1f)
			{
				segmentIndex = SegmentCount - 1;
				segmentTime = 1f;
			}
			else
			{
				float segmentCount = (float)SegmentCount;
				segmentIndex = (int)(segmentCount * time);
				float delta = 1f / segmentCount;
				segmentTime = (time - segmentIndex * delta) / delta;
			}
		}

		protected void PrepareForRuntime()
		{
			for (int i = 0, len = _data.Count; i < len; ++i)
			{
				_data[i].RenderPoints = null;
			}
		}

		protected void RecalcSegmentsLength()
		{
			if (_recalcSegmentsLength)
			{
				float accumulatedLength = _data[0].ProcessLength(0);
				for (int i = 1, len = SegmentCount; i < len; ++i)
				{
					// Calc segment's Length and AccumulatedLength
					accumulatedLength = _data[i].ProcessLength(accumulatedLength);
				}
				_recalcSegmentsLength = false;
			}
		}


		/// <summary>
		/// Adds vertex in the beginning of the spline.
		/// </summary>
		public abstract void AddVertexFirst(Vector3 position);

		/// <summary>
		/// Adds vertex to the end of the spline.
		/// </summary>
		public abstract void AddVertexLast(Vector3 position);

		/// <summary>
		/// Removes vertex from the spline. Valid index is [0..VertexCount-1].
		/// </summary>
		public abstract void RemoveVertex(int index);

		/// <summary>
		/// Removes all vertices.
		/// </summary>
		public abstract void Clear();

		/// <summary>
		/// Inserts vertex before specified index. Valid index is [0..VertexCount].
		/// </summary>
		public abstract void InsertBefore(int vertexIndex, Vector3 position);

		/// <summary>
		/// Inserts vertex after specified index. Valid index is [-1..VertexCount-1]
		/// </summary>
		public abstract void InsertAfter(int vertexIndex, Vector3 position);

		/// <summary>
		/// Gets vertex position. Valid index is [0..VertexCount-1].
		/// </summary>
		public abstract Vector3 GetVertex(int vertexIndex);

		/// <summary>
		/// Sets vertex position. Valid index is [0..VertexCount-1].
		/// </summary>
		public abstract void SetVertex(int vertexIndex, Vector3 position);


		/// <summary>
		/// Evaluates position on the spline. Valid time is [0..1],
		/// where 0 is spline first point, 1 - spline end point.
		/// Caller must ensure that spline is valid before calling this method!
		/// </summary>
		public Vector3 EvalPosition(float time)
		{
			int i;
			float t;
			GetSegmentIndexAndTime(time, out i, out t);
			return _data[i].EvalPosition(t);
		}

		/// <summary>
		/// Evaluates tangent on the spline. Valid time is [0..1],
		/// where 0 is spline first point, 1 - spline end point.
		/// Caller must ensure that spline is valid before calling this method!
		/// </summary>
		public Vector3 EvalTangent(float time)
		{
			int i;
			float t;
			GetSegmentIndexAndTime(time, out i, out t);
			return _data[i].EvalTangent(t);
		}		

		/// <summary>
		/// Evaluates position and tangent on the spline. Valid time is [0..1],
		/// where 0 is spline first point, 1 - spline end point.
		/// Caller must ensure that spline is valid before calling this method!
		/// </summary>
		public PositionTangent EvalPositionTangent(float time)
		{
			int i;
			float t;
			GetSegmentIndexAndTime(time, out i, out t);
			ItemData segment = _data[i];
			PositionTangent result;
			result.Position = segment.EvalPosition(t);
			result.Tangent = segment.EvalTangent(t);
			return result;
		}

		/// <summary>
		/// Evaluates position on the spline. Valid time is [0..1],
		/// where 0 is spline first point, 1 - spline end point.
		/// Caller must ensure that spline is valid before calling this method!
		/// </summary>
		public void EvalPosition(float time, out Vector3 position)
		{
			int i;
			float t;
			GetSegmentIndexAndTime(time, out i, out t);
			position = _data[i].EvalPosition(t);
		}

		/// <summary>
		/// Evaluates tangent on the spline. Valid time is [0..1],
		/// where 0 is spline first point, 1 - spline end point.
		/// Caller must ensure that spline is valid before calling this method!
		/// </summary>
		public void EvalTangent(float time, out Vector3 tangent)
		{
			int i;
			float t;
			GetSegmentIndexAndTime(time, out i, out t);
			tangent = _data[i].EvalTangent(t);
		}

		/// <summary>
		/// Evaluates position and tangent on the spline. Valid time is [0..1],
		/// where 0 is spline first point, 1 - spline end point.
		/// Caller must ensure that spline is valid before calling this method!
		/// </summary>
		public void EvalPositionTangent(float time, out PositionTangent positionTangent)
		{
			int i;
			float t;
			GetSegmentIndexAndTime(time, out i, out t);
			ItemData segment = _data[i];
			positionTangent.Position = segment.EvalPosition(t);
			positionTangent.Tangent = segment.EvalTangent(t);
		}

		/// <summary>
		/// Evaluates Frene frame on the spline. Valid time is [0..1],
		/// where 0 is spline first point, 1 - spline end point.
		/// Caller must ensure that spline is valid before calling this method!
		/// </summary>
		public void EvalFrame(float time, out CurveFrame frame)
		{
			int i;
			float t;
			GetSegmentIndexAndTime(time, out i, out t);
			ItemData segment = _data[i];

			frame.Position = segment.EvalPosition(t);
			Vector3 velocity = segment.EvalFirstDerivative(t);
			Vector3 acceleration = segment.EvalSecondDerivative(t);
			float VDotV = velocity.Dot(velocity);
			float VDotA = velocity.Dot(acceleration);
			frame.Normal = VDotV * acceleration - VDotA * velocity;
			frame.Normal.Normalize();
			frame.Tangent = velocity;
			frame.Tangent.Normalize();
			frame.Binormal = frame.Tangent.Cross(frame.Normal);
		}

		/// <summary>
		/// Evaluates curvature on the spline. Valid time is [0..1],
		/// where 0 is spline first point, 1 - spline end point.
		/// Caller must ensure that spline is valid before calling this method!
		/// </summary>
		public float EvalCurvature(float time)
		{
			int i;
			float t;
			GetSegmentIndexAndTime(time, out i, out t);
			ItemData segment = _data[i];

			Vector3 velocity = segment.EvalFirstDerivative(t);
			float speedSqr = velocity.sqrMagnitude;

			if (speedSqr >= Mathfex.ZeroTolerance)
			{
				Vector3 acceleration = segment.EvalSecondDerivative(t);
				Vector3 cross = velocity.Cross(acceleration);
				float numer = cross.magnitude;
				float denom = Mathf.Pow(speedSqr, 1.5f);
				return numer / denom;
			}

			return 0f;
		}

		/// <summary>
		/// Evaluates torsion on the spline. Valid time is [0..1],
		/// where 0 is spline first point, 1 - spline end point.
		/// Caller must ensure that spline is valid before calling this method!
		/// </summary>
		public float EvalTorsion(float time)
		{
			int i;
			float t;
			GetSegmentIndexAndTime(time, out i, out t);
			ItemData segment = _data[i];

			Vector3 velocity = segment.EvalFirstDerivative(t);
			Vector3 acceleration = segment.EvalSecondDerivative(t);
			Vector3 cross = velocity.Cross(acceleration);
			float denom = cross.sqrMagnitude;

			if (denom >= Mathfex.ZeroTolerance)
			{
				Vector3 jerk = segment.EvalThirdDerivative(t);
				float numer = cross.Dot(jerk);
				return numer / denom;
			}
			
			return 0f;
		}


		/// <summary>
		/// Evaluates position on the spline using curve distance from the start. Valid length is [0..TotalLength].
		/// Caller must ensure that spline is valid before calling this method!
		/// </summary>
		public Vector3 EvalPositionParametrized(float length)
		{
			int i;
			float t;
			GetSegmentIndexAndTime(_parametrization.GetApproximateTimeParameter(length), out i, out t);
			return _data[i].EvalPosition(t);
		}

		/// <summary>
		/// Evaluates tangent on the spline using curve distance from the start. Valid length is [0..TotalLength].
		/// Caller must ensure that spline is valid before calling this method!
		/// </summary>
		public Vector3 EvalTangentParametrized(float length)
		{
			int i;
			float t;
			GetSegmentIndexAndTime(_parametrization.GetApproximateTimeParameter(length), out i, out t);
			return _data[i].EvalTangent(t);
		}

		/// <summary>
		/// Evaluates position and tangent on the spline using curve distance from the start. Valid length is [0..TotalLength].
		/// Caller must ensure that spline is valid before calling this method!
		/// </summary>
		public PositionTangent EvalPositionTangentParametrized(float length)
		{
			int i;
			float t;
			GetSegmentIndexAndTime(_parametrization.GetApproximateTimeParameter(length), out i, out t);
			ItemData segment = _data[i];
			PositionTangent result;
			result.Position = segment.EvalPosition(t);
			result.Tangent = segment.EvalTangent(t);
			return result;
		}

		/// <summary>
		/// Evaluates position on the spline using curve distance from the start. Valid length is [0..TotalLength].
		/// Caller must ensure that spline is valid before calling this method!
		/// </summary>
		public void EvalPositionParametrized(float length, out Vector3 position)
		{
			int i;
			float t;
			GetSegmentIndexAndTime(_parametrization.GetApproximateTimeParameter(length), out i, out t);
			position = _data[i].EvalPosition(t);
		}

		/// <summary>
		/// Evaluates tangent on the spline using curve distance from the start. Valid length is [0..TotalLength].
		/// Caller must ensure that spline is valid before calling this method!
		/// </summary>
		public void EvalTangentParametrized(float length, out Vector3 tangent)
		{
			int i;
			float t;
			GetSegmentIndexAndTime(_parametrization.GetApproximateTimeParameter(length), out i, out t);
			tangent = _data[i].EvalTangent(t);
		}

		/// <summary>
		/// Evaluates position and tangent on the spline using curve distance from the start. Valid length is [0..TotalLength].
		/// Caller must ensure that spline is valid before calling this method!
		/// </summary>
		public void EvalPositionTangentParametrized(float length, out PositionTangent positionTangent)
		{
			int i;
			float t;
			GetSegmentIndexAndTime(_parametrization.GetApproximateTimeParameter(length), out i, out t);
			ItemData segment = _data[i];
			positionTangent.Position = segment.EvalPosition(t);
			positionTangent.Tangent = segment.EvalTangent(t);
		}

		/// <summary>
		/// Evaluates Frenet frame on the spline using curve distance from the start. Valid length is [0..TotalLength].
		/// Caller must ensure that spline is valid before calling this method!
		/// </summary>
		public void EvalFrameParametrized(float length, out CurveFrame frame)
		{
			int i;
			float t;
			GetSegmentIndexAndTime(_parametrization.GetApproximateTimeParameter(length), out i, out t);
			ItemData segment = _data[i];

			frame.Position = segment.EvalPosition(t);
			Vector3 velocity = segment.EvalFirstDerivative(t);
			Vector3 acceleration = segment.EvalSecondDerivative(t);
			float VDotV = velocity.Dot(velocity);
			float VDotA = velocity.Dot(acceleration);
			frame.Normal = VDotV * acceleration - VDotA * velocity;
			frame.Normal.Normalize();
			frame.Tangent = velocity;
			frame.Tangent.Normalize();
			frame.Binormal = frame.Tangent.Cross(frame.Normal);
		}

		/// <summary>
		/// Evaluates curvature on the spline using curve distance from the start. Valid length is [0..TotalLength].
		/// Caller must ensure that spline is valid before calling this method!
		/// </summary>
		public float EvalCurvatureParametrized(float length)
		{
			int i;
			float t;
			GetSegmentIndexAndTime(_parametrization.GetApproximateTimeParameter(length), out i, out t);
			ItemData segment = _data[i];

			Vector3 velocity = segment.EvalFirstDerivative(t);
			float speedSqr = velocity.sqrMagnitude;

			if (speedSqr >= Mathfex.ZeroTolerance)
			{
				Vector3 acceleration = segment.EvalSecondDerivative(t);
				Vector3 cross = velocity.Cross(acceleration);
				float numer = cross.magnitude;
				float denom = Mathf.Pow(speedSqr, 1.5f);
				return numer / denom;
			}

			return 0f;
		}

		/// <summary>
		/// Evaluates torsion on the spline using curve distance from the start. Valid length is [0..TotalLength].
		/// Caller must ensure that spline is valid before calling this method!
		/// </summary>
		public float EvalTorsionParametrized(float length)
		{
			int i;
			float t;
			GetSegmentIndexAndTime(_parametrization.GetApproximateTimeParameter(length), out i, out t);
			ItemData segment = _data[i];

			Vector3 velocity = segment.EvalFirstDerivative(t);
			Vector3 acceleration = segment.EvalSecondDerivative(t);
			Vector3 cross = velocity.Cross(acceleration);
			float denom = cross.sqrMagnitude;

			if (denom >= Mathfex.ZeroTolerance)
			{
				Vector3 jerk = segment.EvalThirdDerivative(t);
				float numer = cross.Dot(jerk);
				return numer / denom;
			}

			return 0f;
		}


		/// <summary>
		/// Returns total length of the spline.
		/// </summary>
		public float CalcTotalLength()
		{
			if (_data.Count < 2) return 0f;
			RecalcSegmentsLength();
			return _data[SegmentCount - 1].AccumulatedLength;
		}

		/// <summary>
		/// Converts length parameter [0..TotalLength] to time parameter [0..1]
		/// </summary>
		/// <param name="length">Distance parameter to convert into time parameter</param>
		/// <param name="iterations">Number of iterations used in internal calculations, default is 32</param>
		/// <param name="tolerance">Small positive number, e.g. 1e-5f</param>
		public float LengthToTime(float length, int iterations, float tolerance)
		{
			// Update length information if necessary
			if (_data.Count < 2) return 0f;
			RecalcSegmentsLength();

			int segmentCount = SegmentCount;
			int lastSegment = segmentCount - 1;

			// Check out of bounds length
			if (length <= 0f) return 0f;
			if (length >= _data[lastSegment].AccumulatedLength) return 1f;

			//TODO: use binary search
			int segmentIndex;
			for (segmentIndex = 0; segmentIndex < segmentCount; ++segmentIndex)
			{
				if (length < _data[segmentIndex].AccumulatedLength)
				{
					break;
				}
			}

			ItemData segment = _data[segmentIndex];
			float segmentStartTime = (float)segmentIndex; // Uniform parametrization with unit length time per segment

			float len0 = segmentIndex == 0 ? length : (length - _data[segmentIndex - 1].AccumulatedLength);
			float len1 = _data[segmentIndex].Length;

			// If L(t) is the length function for t in [tmin,tmax], the derivative is
			// L'(t) = |x'(t)| >= 0 (the magnitude of speed).  Therefore, L(t) is a
			// nondecreasing function (and it is assumed that x'(t) is zero only at
			// isolated points; that is, no degenerate curves allowed).  The second
			// derivative is L"(t).  If L"(t) >= 0 for all t, L(t) is a convex
			// function and Newton's method for root finding is guaranteed to
			// converge.  However, L"(t) can be negative, which can lead to Newton
			// iterates outside the domain [tmin,tmax].  The algorithm here avoids
			// this problem by using a hybrid of Newton's method and bisection.

			// Initial guess for Newton's method is dt0.
			float dt1 = 1f;
			float dt0 = dt1 * len0 / len1;

			// Initial root-bounding interval for bisection.
			float lower = 0f;
			float upper = dt1;

			for (int i = 0; i < iterations; ++i)
			{
				float difference = segment.EvalLength(0, dt0) - len0;
				if (UnityEngine.Mathf.Abs(difference) <= tolerance)
				{
					// |L(mTimes[key]+dt0)-length| is close enough to zero, report
					// mTimes[key]+dt0 as the time at which 'length' is attained.
					return (segmentStartTime + dt0) / segmentCount;
				}

				// Generate a candidate for Newton's method.
				float dt0Candidate = dt0 - difference / segment.EvalSpeed(dt0);

				// Update the root-bounding interval and test for containment of the candidate.
				if (difference > 0f)
				{
					upper = dt0;
					if (dt0Candidate <= lower)
					{
						// Candidate is outside the root-bounding interval. Use bisection instead.
						dt0 = 0.5f * (upper + lower);
					}
					else
					{
						// There is no need to compare to 'upper' because the tangent line has
						// positive slope, guaranteeing that the t-axis intercept is smaller than 'upper'.
						dt0 = dt0Candidate;
					}
				}
				else
				{
					lower = dt0;
					if (dt0Candidate >= upper)
					{
						// Candidate is outside the root-bounding interval. Use bisection instead.
						dt0 = 0.5f * (upper + lower);
					}
					else
					{
						// There is no need to compare to 'lower' because the tangent line has
						// positive slope, guaranteeing that the t-axis intercept is larger than 'lower'.
						dt0 = dt0Candidate;
					}
				}
			}

			// A root was not found according to the specified number of iterations
			// and tolerance.  You might want to increase iterations or tolerance or
			// integration accuracy.  However, in this application it is likely that
			// the time values are oscillating, due to the limited numerical
			// precision of 32-bit floats.  It is safe to use the last computed time.
			return (segmentStartTime + dt0) / segmentCount;
		}

		/// <summary>
		/// Converts length parameter [0..TotalLength] to time parameter [0..1]
		/// </summary>
		/// <param name="length">Distance parameter to convert into time parameter</param>
		public float LengthToTime(float length)
		{
			return LengthToTime(length, 32, Mathfex.ZeroTolerance);
		}

		/// <summary>
		/// Parametrizes the spline using arc length. This method must be called before calling
		/// any parametrized evaluation methods, otherwise they will throw an exception.
		/// Returns spline total length.
		/// </summary>
		/// <param name="pointCount">Number of points which will be used to divide
		/// total length of the spline into equal intervals when parametrizing.</param>
		/// <returns></returns>
		public float ParametrizeByArcLength(int pointCount)
		{
			if (!Valid) return -1f;

			float L = CalcTotalLength();
			float L_div_pointCount = L / pointCount;

			int count = pointCount + 1;
			float[] sSample = new float[count];
			float[] tSample = new float[count];
			float[] tsSlope = new float[count];

			sSample[0] = 0f;
			tSample[0] = 0f;
			tsSlope[0] = 0f;

			for (int i = 1; i < pointCount; ++i)
			{
				float s = i * L_div_pointCount;
				sSample[i] = s;
				tSample[i] = LengthToTime(s, 32, Mathfex.ZeroTolerance);
				tsSlope[i] = (tSample[i] - tSample[i - 1]) / (sSample[i] - sSample[i - 1]);
			}

			sSample[pointCount] = L;
			tSample[pointCount] = 1f;
			tsSlope[pointCount] = (tSample[pointCount] - tSample[pointCount - 1]) / (sSample[pointCount] - sSample[pointCount - 1]);

			_parametrization = new ArcLengthParametrization()
			{
				sSample = sSample,
				tSample = tSample,
				tsSlope = tsSlope,
				L = L
			};

			return L;
		}


#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			Gizmos.color = _renderColor;
			Gizmos.matrix = transform.localToWorldMatrix;

			int segmentCount = SegmentCount;
			if (_data.Count > 1 && segmentCount > 0)
			{
				for (int segmentIndex = 0; segmentIndex < segmentCount; ++segmentIndex)
				{
					ItemData segment = _data[segmentIndex];
					if (segment.EnsureRenderPointsValidity()) segment.UpdateRenderPoints();

					Vector3[] renderPoints = segment.RenderPoints;
					Vector3 prev = renderPoints[0];
					Vector3 curr;
					for (int renderPointIndex = 1, len = renderPoints.Length; renderPointIndex < len; ++renderPointIndex)
					{
						curr = renderPoints[renderPointIndex];
						Gizmos.DrawLine(curr, prev);
						prev = curr;
					}
				}
			}

			for (int vertexIndex = 0; vertexIndex < _data.Count; ++vertexIndex)
			{
				Vector3 position = _data[vertexIndex].Position;
				Gizmos.DrawCube(position, RenderPointSize * HandleUtility.GetHandleSize(position));
			}

			Gizmos.matrix = Matrix4x4.identity;
		}

		public int _SelectVertex(Vector2 click, Camera camera)
		{
			float radSqr = 100;
			Transform thisTransform = transform;
			for (int i = 0, len = _data.Count; i < len; ++i)
			{
				Vector3 vertex = thisTransform.TransformPoint(_data[i].Position);
				Vector2 point = HandleUtility.WorldToGUIPoint(vertex);

				float deltaX = click.x - point.x;
				float deltaXSqr = deltaX * deltaX;
				if (deltaXSqr > radSqr) continue;

				float deltaY = click.y - point.y;
				float deltaYSqr = deltaY * deltaY;
				if (deltaYSqr > radSqr) continue;

				if (deltaXSqr + deltaYSqr > radSqr)
				{
					continue;
				}
				else
				{
					return i;
				}
			}
			return -1;
		}

		private bool GetClickPoint(Vector2 click, Camera camera, out Vector3 position)
		{
			Ray ray = HandleUtility.GUIPointToWorldRay(click);
			Plane plane;
			switch (_creationPlane)
			{
				case SplinePlaneTypes.XZ: plane = new Plane(Vector3.up, Vector3.zero); break;
				case SplinePlaneTypes.XY: plane = new Plane(Vector3.forward, Vector3.zero); break;
				default:
				case SplinePlaneTypes.YZ: plane = new Plane(Vector3.right, Vector3.zero); break;
			}
			float enter;
			if (plane.Raycast(ray, out enter))
			{
				position = ray.GetPoint(enter);
				position = transform.InverseTransformPoint(position);
				return true;
			}
			position = Vector3ex.Zero;
			return false;
		}

		public void _AddVertexFirst(Vector2 click, Camera camera)
		{
			Vector3 position;
			if (GetClickPoint(click, camera, out position))
			{
				AddVertexFirst(position);
			}
		}

		public void _AddVertexLast(Vector2 click, Camera camera)
		{
			Vector3 position;
			if (GetClickPoint(click, camera, out position))
			{
				AddVertexLast(position);
			}
		}

		public abstract void _ForceUpdate();
#endif
	}
}
