using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_CatmullRom3 : Test_Base
	{
		public enum CreateType
		{
			Empty,
			Children,
			Spiral
		}

		private bool _prevCreate;

		public bool       Create;
		public CreateType Type;

		private void OnEnable()
		{
			_prevCreate = Create;
		}

		private void Update()
		{
			if (_prevCreate != Create)
			{
				if (Type == CreateType.Empty) CreateEmpty();
				else if (Type == CreateType.Children) CreateChildren();
				else CreateSpiral();
			}
			_prevCreate = Create;
		}

		private void CreateEmpty()
		{
			CatmullRomSpline3.Create();
		}

		private void CreateChildren()
		{
			Transform thisTransform = transform;
			int childCount = thisTransform.childCount;
			Vector3[] points = new Vector3[childCount];
			for (int i = 0; i < childCount; ++i)
			{
				points[i] = thisTransform.GetChild(i).position.ToVector2XY();
			}
			CatmullRomSpline3.Create(points, SplineTypes.Open);
		}

		private void CreateSpiral()
		{
			CatmullRomSpline3 spline = CatmullRomSpline3.Create();
			int points = 1000;
			for (int i = 0; i < points; ++i)
			{
				float t = i * .1f;
				Vector3 p = .5f * new Vector3(t * Mathf.Cos(t), .5f * t, t * Mathf.Sin(t));
				spline.AddVertexLast(p);
			}
		}
	}
}
