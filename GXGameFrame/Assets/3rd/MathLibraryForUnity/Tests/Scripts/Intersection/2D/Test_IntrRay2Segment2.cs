using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrRay2Segment2 : Test_Base
	{
		public Transform Ray;
		public Transform SegmentStart;
		public Transform SegmentEnd;

		private void OnDrawGizmos()
		{
			Ray2 ray = CreateRay2(Ray);
			Segment2 segment = CreateSegment2(SegmentStart, SegmentEnd);

			IntersectionTypes intersectionType;
			bool test = Intersection.TestRay2Segment2(ref ray, ref segment, out intersectionType);
			Ray2Segment2Intr info;
			bool find = Intersection.FindRay2Segment2(ref ray, ref segment, out info);

			FiguresColor();
			DrawRay(ref ray);
			DrawSegment(ref segment);

			if (find)
			{
				ResultsColor();
				if (info.IntersectionType == IntersectionTypes.Point)
				{
					DrawPoint(info.Point0);
				}
				else
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
