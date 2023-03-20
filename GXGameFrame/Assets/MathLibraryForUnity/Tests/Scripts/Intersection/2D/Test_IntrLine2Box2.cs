using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrLine2Box2 : Test_Base
	{
		public Transform Line;
		public Transform Box;

		private void OnDrawGizmos()
		{
			Line2 line = CreateLine2(Line);
			Box2 box = CreateBox2(Box);

			bool test = Intersection.TestLine2Box2(ref line, ref box);
			Line2Box2Intr info;
			bool find = Intersection.FindLine2Box2(ref line, ref box, out info);

			FiguresColor();
			DrawLine(ref line);
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
