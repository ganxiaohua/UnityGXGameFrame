using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrSegment2Segment2 : Test_Base
	{
		public Transform Seg0Start;
		public Transform Seg0End;
		public Transform Seg1Start;
		public Transform Seg1End;
		
		private void OnDrawGizmos()
		{
			Segment2 segment0 = CreateSegment2(Seg0Start, Seg0End);
			Segment2 segment1 = CreateSegment2(Seg1Start, Seg1End);
			
			IntersectionTypes intersectionType;
			bool test = Intersection.TestSegment2Segment2(ref segment0, ref segment1, out intersectionType);
			Segment2Segment2Intr info;
			bool find = Intersection.FindSegment2Segment2(ref segment0, ref segment1, out info);

			FiguresColor();
			DrawSegment(ref segment0);
			DrawSegment(ref segment1);

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
			if (intersectionType != info.IntersectionType) LogError("intersectionType != info.IntersectionType");
		}
	}
}
