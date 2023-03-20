using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrRay3Plane3 : Test_Base
	{
		public Transform Ray;
		public Transform Plane;

		private void OnDrawGizmos()
		{
			Ray3 ray = CreateRay3(Ray);
			Plane3 plane = CreatePlane3(Plane);

			IntersectionTypes intersectionType;
			bool test = Intersection.TestRay3Plane3(ref ray, ref plane, out intersectionType);
			Ray3Plane3Intr info;
			bool find = Intersection.FindRay3Plane3(ref ray, ref plane, out info);

			FiguresColor();
			DrawPlane(ref plane, Plane);
			DrawRay(ref ray);

			if (find)
			{
				ResultsColor();
				DrawPoint(info.Point);
			}

			LogInfo(info.IntersectionType + " " + info.RayParameter);
			if (test != find) LogError("test != find");
			if (intersectionType != info.IntersectionType) LogError("intersectionType != info.IntersectionType");
		}
	}
}
