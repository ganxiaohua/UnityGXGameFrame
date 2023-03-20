using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrLine2Circle2 : Test_Base
	{
		public Transform Line;
		public Transform Circle;

		private void OnDrawGizmos()
		{
			Line2 line = CreateLine2(Line);
			Circle2 circle = CreateCircle2(Circle);

			bool test = Intersection.TestLine2Circle2(ref line, ref circle);
			Line2Circle2Intr info;
			bool find = Intersection.FindLine2Circle2(ref line, ref circle, out info);

			FiguresColor();
			DrawLine(ref line);
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
