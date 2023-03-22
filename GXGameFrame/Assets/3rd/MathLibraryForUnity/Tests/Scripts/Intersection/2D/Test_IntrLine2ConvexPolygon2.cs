using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrLine2ConvexPolygon2 : Test_Base
	{
		public Transform Line;
		public Transform[] ConvexPolygon;

		private void OnDrawGizmos()
		{
			Line2 line = CreateLine2(Line);
			Polygon2 convexPolygon = CreatePolygon2(ConvexPolygon);

			bool test = Intersection.TestLine2ConvexPolygon2(ref line, convexPolygon);
			Line2ConvexPolygon2Intr info;
			bool find = Intersection.FindLine2ConvexPolygon2(ref line, convexPolygon, out info);

			FiguresColor();
			DrawLine(ref line);
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
