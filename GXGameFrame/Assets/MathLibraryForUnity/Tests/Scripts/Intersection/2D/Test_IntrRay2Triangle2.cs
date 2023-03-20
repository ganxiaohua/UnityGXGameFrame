using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrRay2Triangle2 : Test_Base
	{
		public Transform Ray;
		public Transform V0;
		public Transform V1;
		public Transform V2;

		private void OnDrawGizmos()
		{
			Ray2 ray = CreateRay2(Ray);
			Triangle2 triangle = CreateTriangle2(V0, V1, V2);

			IntersectionTypes intersectionType;
			bool test = Intersection.TestRay2Triangle2(ref ray, ref triangle, out intersectionType);
			Ray2Triangle2Intr info;
			bool find = Intersection.FindRay2Triangle2(ref ray, ref triangle, out info);

			FiguresColor();
			DrawRay(ref ray);
			DrawTriangle(ref triangle);

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

			LogInfo(intersectionType);
			if (test != find) LogError("test != find");
			if (intersectionType != info.IntersectionType) LogError("intersectionType != info.IntersectionType");
		}
	}
}
