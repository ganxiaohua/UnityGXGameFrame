using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_RandomInTriangle : Test_Base
	{
		private Vector3[] _points;

		private bool _previous;
		public bool ToggleToGenerate;

		public int Count;
		public Transform V0;
		public Transform V1;
		public Transform V2;

		private void Update()
		{
			if (ToggleToGenerate != _previous) Generate();
			_previous = ToggleToGenerate;
		}

		private void Generate()
		{
			Rand rand = Rand.Instance;
			_points = new Vector3[Count];
			Vector3 v0 = V0.position;
			Vector3 v1 = V1.position;
			Vector3 v2 = V2.position;
			for (int i = 0; i < Count; ++i)
			{
				_points[i] = rand.InTriangle(ref v0, ref v1, ref v2);
			}
		}

		private void OnDrawGizmos()
		{
			if (_points != null) DrawPoints(_points);
			DrawSegment(V0.position, V1.position);
			DrawSegment(V1.position, V2.position);
			DrawSegment(V2.position, V0.position);
		}
	}
}
