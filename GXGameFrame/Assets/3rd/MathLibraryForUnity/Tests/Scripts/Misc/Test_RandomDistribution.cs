using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_RandomDistribution : Test_Base
	{
		private Vector3[] _points;
		private bool _previous;

		public bool ToggleToGenerate;
		public int Count;
		public int IntMin;
		public int IntMax;
		public Vector2 Scale;

		private void Update()
		{
			if (ToggleToGenerate != _previous)
			{
				Generate();
			}
			_previous = ToggleToGenerate;
		}

		private void Generate()
		{
			int range = IntMax - IntMin;
			_points = new Vector3[range];
			int[] buckets = new int[range];
			for (int i = 0; i < Count; ++i)
			{
				int value = Rand.Instance.NextInt(IntMin, IntMax);
				++buckets[value - IntMin];
			}
			for (int i = 0; i < range; ++i)
			{
				_points[i] = new Vector3((i + IntMin) * Scale.x, buckets[i] * Scale.y / Count, 0f);
			}
		}

		private void OnDrawGizmos()
		{
			if (_points != null)
			{
				DrawPoint(_points[0]);
				for (int i = 0; i < _points.Length - 1; ++i)
				{
					DrawSegment(_points[i], _points[i + 1]);
					DrawPoint(_points[i + 1]);
				}
				Vector3 v0 = new Vector3(IntMin, 0f, 0f);
				Vector3 vx = new Vector3(IntMax - 1, 0f, 0f);
				Vector3 vy = new Vector3(IntMin, Count, 0f);
				Gizmos.DrawLine(v0, vx);
				Gizmos.DrawLine(v0, vy);
			}
		}
	}
}
