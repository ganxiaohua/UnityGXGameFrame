using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrLine3Box3 : Test_Base
	{
		public Transform Line;
		public Transform Box;

		private void OnDrawGizmos()
		{
			Line3 line = CreateLine3(Line);
			Box3 box = CreateBox3(Box);

			bool test = Intersection.TestLine3Box3(ref line, ref box);
			Line3Box3Intr info;
			bool find = Intersection.FindLine3Box3(ref line, ref box, out info);

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
