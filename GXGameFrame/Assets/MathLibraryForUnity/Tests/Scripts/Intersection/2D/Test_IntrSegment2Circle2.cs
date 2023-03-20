using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrSegment2Circle2 : Test_Base
	{
		public Transform P0;
		public Transform P1;
		public Transform Circle;

		private void OnDrawGizmos()
		{
			Segment2 segment = CreateSegment2(P0, P1);
			Circle2 circle = CreateCircle2(Circle);

			bool test = Intersection.TestSegment2Circle2(ref segment, ref circle);
			Segment2Circle2Intr info;
			bool find = Intersection.FindSegment2Circle2(ref segment, ref circle, out info);

			FiguresColor();
			DrawSegment(ref segment);
			DrawCircle(ref circle);

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
		}
	}
}
