using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_ContCreateBox2 : Test_Base
	{
		private Box2      _box;
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
			DrawBox(ref _box);
		}

		private void Update()
		{
			if (ToggleToGenerate != _previous)
			{
				_points = GenerateRandomSet2D(GenerateRadius, GenerateCountMin, GenerateCountMax);
				_box = Box2.CreateFromPoints(_points);
			}
			_previous = ToggleToGenerate;
		}
	}
}
