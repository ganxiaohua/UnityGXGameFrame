using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrRay2Polygon2 : Test_Base
	{
		public Transform Ray;
		public Transform[] Polygon;

		private void OnDrawGizmos()
		{
			Ray2 ray = CreateRay2(Ray);
			Polygon2 polygon = CreatePolygon2(Polygon);

			Ray2Polygon2Intr info;
			bool find = Intersection.FindRay2Polygon2(ref ray, polygon, out info);

			FiguresColor();
			DrawRay(ref ray);
			DrawPolygon(polygon);

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
		}
	}
}
