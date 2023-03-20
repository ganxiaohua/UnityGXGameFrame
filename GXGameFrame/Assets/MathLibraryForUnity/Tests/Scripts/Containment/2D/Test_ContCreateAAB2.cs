using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_ContCreateAAB2 : Test_Base
	{
		private AAB2      _aab;
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
			DrawAAB(ref _aab);
		}

		private void Update()
		{
			if (ToggleToGenerate != _previous)
			{
				_points = GenerateRandomSet2D(GenerateRadius, GenerateCountMin, GenerateCountMax);
				_aab = AAB2.CreateFromPoints(_points);
			}
			_previous = ToggleToGenerate;
		}
	}
}
