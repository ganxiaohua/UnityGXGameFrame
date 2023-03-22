using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrSegment2Box2 : Test_Base
	{
		public Transform P0;
		public Transform P1;
		public Transform Box;

		private void OnDrawGizmos()
		{
			Segment2 segment = CreateSegment2(P0, P1);
			Box2 box = CreateBox2(Box);

			bool test = Intersection.TestSegment2Box2(ref segment, ref box);
			Segment2Box2Intr info;
			bool find = Intersection.FindSegment2Box2(ref segment, ref box, out info);

			FiguresColor();
			DrawSegment(ref segment);
			DrawBox(ref box);

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
