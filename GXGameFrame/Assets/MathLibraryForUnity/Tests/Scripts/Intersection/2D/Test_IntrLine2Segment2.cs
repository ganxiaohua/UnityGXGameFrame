using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrLine2Segment2 : Test_Base
	{
		public Transform Line;
		public Transform SegmentStart;
		public Transform SegmentEnd;

		private void OnDrawGizmos()
		{
			Line2 line = CreateLine2(Line);
			Segment2 segment = CreateSegment2(SegmentStart, SegmentEnd);

			IntersectionTypes intersectionType;
			bool test = Intersection.TestLine2Segment2(ref line, ref segment, out intersectionType);
			Line2Segment2Intr info;
			bool find = Intersection.FindLine2Segment2(ref line, ref segment, out info);

			FiguresColor();
			DrawLine(ref line);
			DrawSegment(ref segment);

			if (find)
			{
				ResultsColor();
				if (info.IntersectionType == IntersectionTypes.Point)
				{
					DrawPoint(info.Point);
				}
				else if (info.IntersectionType == IntersectionTypes.Segment)
				{
					DrawSegment(segment.P0, segment.P1);
				}
			}

			LogInfo(intersectionType);
			if (test != find) LogError("test != find");
			if (intersectionType != info.IntersectionType) LogError("intersectionType != info.IntersectionType");
		}
	}
}
