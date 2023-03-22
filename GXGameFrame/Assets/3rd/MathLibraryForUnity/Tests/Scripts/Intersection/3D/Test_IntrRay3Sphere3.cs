using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrRay3Sphere3 : Test_Base
	{
		public Transform Ray;
		public Transform Sphere;

		private void OnDrawGizmos()
		{
			Ray3 ray = CreateRay3(Ray);
			Sphere3 sphere = CreateSphere3(Sphere);

			bool test = Intersection.TestRay3Sphere3(ref ray, ref sphere);
			Ray3Sphere3Intr info;
			bool find = Intersection.FindRay3Sphere3(ref ray, ref sphere, out info);

			FiguresColor();
			DrawRay(ref ray);
			DrawSphere(ref sphere);

			if (find)
			{
				ResultsColor();
				if (info.IntersectionType == IntersectionTypes.Point)
				{
					DrawPoint(info.Point0);
				}
				else if (info.IntersectionType == IntersectionTypes.Segment)
				{
					DrawSegment(info.Point0, info.Point1);
					DrawPoint(info.Point0);
					DrawPoint(info.Point1);
				}
			}

			LogInfo(info.IntersectionType);
			if (test != find) LogError("test != find");
		}
	}
}