using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrRay2ConvexPolygon2 : Test_Base
	{
		public Transform Ray;
		public Transform[] ConvexPolygon;

		private void OnDrawGizmos()
		{
			Ray2 ray = CreateRay2(Ray);
			Polygon2 convexPolygon = CreatePolygon2(ConvexPolygon);

			bool test = Intersection.TestRay2ConvexPolygon2(ref ray, convexPolygon);
			Ray2ConvexPolygon2Intr info;
			bool find = Intersection.FindRay2ConvexPolygon2(ref ray, convexPolygon, out info);

			FiguresColor();
			DrawRay(ref ray);
			DrawPolygon(convexPolygon);

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
			if (!convexPolygon.IsConvex()) LogError("Polygon is non-convex");
		}
	}
}
