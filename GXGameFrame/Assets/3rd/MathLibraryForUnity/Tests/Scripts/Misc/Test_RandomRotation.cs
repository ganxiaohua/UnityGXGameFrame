using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_RandomRotation : Test_Base
	{
		private bool _previous;
		private Vector3[] _points;

		public bool ToggleToGenerate;
		public int Samples;
		public float Scale;

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
			_points = new Vector3[Samples];
			Vector3 v = Vector3ex.One * Scale;
			for (int i = 0; i < Samples; ++i)
			{
				Quaternion q = Rand.Instance.RandomRotation();
				_points[i] = q * v;
			}
		}

		private void OnDrawGizmos()
		{
			if (_points != null)
			{
				DrawPoints(_points);
			}
		}
	}
}
