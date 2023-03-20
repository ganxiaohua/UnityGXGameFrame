using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_ContCreateCircle2 : Test_Base
	{
		private Circle2	  _circle0;
		private Circle2	  _circle1;
		private Vector2[] _points;
		private bool      _previous;

		public bool  ToggleToGenerate;
		public float GenerateRadius;
		public int   GenerateCountMin;
		public int   GenerateCountMax;

		private void Awake()
		{
			_points = new Vector2[0];
		}

		private void OnDrawGizmos()
		{
			DrawPoints(_points);
			Gizmos.color = Color.red;
			DrawCircle(ref _circle0);
			Gizmos.color = Color.blue;
			DrawCircle(ref _circle1);
		}

		private void Update()
		{
			if (ToggleToGenerate != _previous)
			{
				_points = GenerateRandomSet2D(GenerateRadius, GenerateCountMin, GenerateCountMax);
				_circle0 = Circle2.CreateFromPointsAAB(_points);
				_circle1 = Circle2.CreateFromPointsAverage(_points);
			}
			_previous = ToggleToGenerate;
		}
	}
}
