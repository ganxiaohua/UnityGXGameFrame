using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	public class Test_FollowSpline : MonoBehaviour
	{
		private PositionTangent[] _visualPoints;
		private float             _time;
		private float             _distance;
		private float             _splineLength;

		public CatmullRomSpline3 Spline;
		public Transform         Target;
		public int               ParametrizationCount;
		public bool              ConstantSpeedTraversal;
		public float             TimeSpeed;		
		public float             ConstantSpeed;

		private void Awake()
		{
			if (ConstantSpeedTraversal)
			{
				// This must be called before calling any of the parametrization
				// evaluation methods!
				_splineLength = Spline.ParametrizeByArcLength(ParametrizationCount);

				_visualPoints = new PositionTangent[ParametrizationCount];
				for (int i = 0; i < _visualPoints.Length; ++i)
				{
					_visualPoints[i] = Spline.EvalPositionTangentParametrized((float)i / (_visualPoints.Length - 1) * _splineLength);
					_visualPoints[i].Position = Spline.transform.TransformPoint(_visualPoints[i].Position);
				}
			}
			else
			{
				_visualPoints = new PositionTangent[ParametrizationCount];
				for (int i = 0; i < _visualPoints.Length; ++i)
				{
					_visualPoints[i] = Spline.EvalPositionTangent((float)i / (_visualPoints.Length - 1));
					_visualPoints[i].Position = Spline.transform.TransformPoint(_visualPoints[i].Position);
				}
			}
		}

		private void Update()
		{
			if (ConstantSpeedTraversal)
			{
				_distance += ConstantSpeed * UnityEngine.Time.deltaTime;
				if (_distance > _splineLength) _distance = 0f;

				PositionTangent data;
				Spline.EvalPositionTangentParametrized(_distance, out data);
				Target.position = Spline.transform.TransformPoint(data.Position);
				Target.forward = data.Tangent;
			}
			else
			{
				_time += TimeSpeed * UnityEngine.Time.deltaTime;
				if (_time > 1f) _time = 0f;

				PositionTangent data;
				Spline.EvalPositionTangent(_time, out data);
				Target.position = Spline.transform.TransformPoint(data.Position);
				Target.forward = data.Tangent;
			}
		}

		private void OnDrawGizmos()
		{
			if (_visualPoints != null)
			{
				foreach (var entry in _visualPoints)
				{
					Gizmos.DrawLine(entry.Position, entry.Position + entry.Tangent * 5f);
					Gizmos.DrawWireSphere(entry.Position, .5f);
				}
			}
		}
	}
}
