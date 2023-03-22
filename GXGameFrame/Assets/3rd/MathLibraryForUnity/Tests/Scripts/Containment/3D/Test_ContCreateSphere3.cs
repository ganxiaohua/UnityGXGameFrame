using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_ContCreateSphere3 : Test_Base
	{
		private Sphere3	  _sphere0;
		private Sphere3   _sphere1;
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
			Gizmos.color = Color.red;
			DrawSphere(ref _sphere0);
			Gizmos.color = Color.blue;
			DrawSphere(ref _sphere1);
		}

		private void Update()
		{
			if (ToggleToGenerate != _previous)
			{
				_points = GenerateRandomSet3D(GenerateRadius, GenerateCountMin, GenerateCountMax);
				_sphere0 = Sphere3.CreateFromPointsAAB(_points);
				_sphere1 = Sphere3.CreateFromPointsAverage(_points);
			}
			_previous = ToggleToGenerate;
		}
	}
}
