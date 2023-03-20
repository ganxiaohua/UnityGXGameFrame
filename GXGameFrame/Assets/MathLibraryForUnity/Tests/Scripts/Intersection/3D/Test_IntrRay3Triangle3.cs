using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrRay3Triangle3 : Test_Base
	{
		public Transform Ray;
		public Transform V0;
		public Transform V1;
		public Transform V2;

		private void OnDrawGizmos()
		{
			Ray3 ray = CreateRay3(Ray);
			Triangle3 triangle = CreateTriangle3(V0, V1, V2);

			IntersectionTypes intersectionType;
			bool test = Intersection.TestRay3Triangle3(ref ray, ref triangle, out intersectionType);
			Ray3Triangle3Intr info;
			bool find = Intersection.FindRay3Triangle3(ref ray, ref triangle, out info);

			FiguresColor();
			DrawRay(ref ray);
			DrawTriangle(ref triangle);

			if (find)
			{
				ResultsColor();
				DrawPoint(info.Point);
			}

			LogInfo(info.IntersectionType);
			if (test != find) LogError("test != find");
			if (intersectionType != info.IntersectionType) LogError("intersectionType != info.IntersectionType");
		}
	}
}
