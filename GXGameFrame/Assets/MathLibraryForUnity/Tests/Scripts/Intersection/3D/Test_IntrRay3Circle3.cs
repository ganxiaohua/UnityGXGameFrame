using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrRay3Circle3 : Test_Base
	{
		public Transform Ray;
		public Transform Circle;

		private void OnDrawGizmos()
		{
			Ray3 ray = CreateRay3(Ray);
			Circle3 circle = CreateCircle3(Circle);

			Ray3Circle3Intr info;
			bool find = Intersection.FindRay3Circle3(ref ray, ref circle, out info);

			FiguresColor();
			DrawCircle(ref circle);
			DrawRay(ref ray);

			if (find)
			{
				ResultsColor();
				DrawPoint(info.Point);
			}

			LogInfo(info.IntersectionType);
		}
	}
}
