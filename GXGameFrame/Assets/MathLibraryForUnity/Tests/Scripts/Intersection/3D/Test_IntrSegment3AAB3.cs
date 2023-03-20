using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrSegment3AAB3 : Test_Base
	{
		public Transform P0;
		public Transform P1;
		public Transform Box_Point0;
		public Transform Box_Point1;

		private void OnDrawGizmos()
		{
			Segment3 segment = CreateSegment3(P0, P1);
			AAB3 box = CreateAAB3(Box_Point0, Box_Point1);

			bool test = Intersection.TestSegment3AAB3(ref segment, ref box);
			Segment3AAB3Intr info;
			bool find = Intersection.FindSegment3AAB3(ref segment, ref box, out info);

			FiguresColor();
			DrawSegment(ref segment);
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
