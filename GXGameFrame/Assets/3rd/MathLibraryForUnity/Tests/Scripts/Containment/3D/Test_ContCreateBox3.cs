using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_ContCreateBox3 : Test_Base
	{
		private Box3      _box;
		private Vector3[] _points;
		private bool      _previous;

		public bool  ToggleToGenerate;
		public float GenerateRadius;
		public int   GenerateCountMin;
		public int   GenerateCountMax;

		private void Awake()
		{
			_points = new Vector3[0];
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
				_points = GenerateRandomSet3D(GenerateRadius, GenerateCountMin, GenerateCountMax);
				_box = Box3.CreateFromPoints(_points);
			}
			_previous = ToggleToGenerate;
		}
	}
}
