using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrSegment2ConvexPolygon2 : Test_Base
	{
		public Transform P0, P1;
		public Transform[] ConvexPolygon;

		private void OnDrawGizmos()
		{
			Segment2 segment = CreateSegment2(P0, P1);
			Polygon2 convexPolygon = CreatePolygon2(ConvexPolygon);

			bool test = Intersection.TestSegment2ConvexPolygon2(ref segment, convexPolygon);
			Segment2ConvexPolygon2Intr info;
			bool find = Intersection.FindSegment2ConvexPolygon2(ref segment, convexPolygon, out info);

			FiguresColor();
			DrawSegment(ref segment);
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
