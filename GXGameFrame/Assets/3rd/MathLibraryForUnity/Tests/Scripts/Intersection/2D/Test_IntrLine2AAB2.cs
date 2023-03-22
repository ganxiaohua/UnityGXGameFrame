using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrLine2AAB2 : Test_Base
	{
		public Transform Line;
		public Transform Box_Point0;
		public Transform Box_Point1;

		private void OnDrawGizmos()
		{
			Line2 line = CreateLine2(Line);
			AAB2 box = CreateAAB2(Box_Point0, Box_Point1);

			bool test = Intersection.TestLine2AAB2(ref line, ref box);
			Line2AAB2Intr info;
			bool find = Intersection.FindLine2AAB2(ref line, ref box, out info);

			FiguresColor();
			DrawLine(ref line);
			DrawAAB(ref box);

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
