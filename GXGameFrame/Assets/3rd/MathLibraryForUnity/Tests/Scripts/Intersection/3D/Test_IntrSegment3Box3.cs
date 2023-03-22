using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrSegment3Box3 : Test_Base
	{
		public Transform P0;
		public Transform P1;
		public Transform Box;

		private void OnDrawGizmos()
		{
			Segment3 segment = CreateSegment3(P0, P1);
			Box3 box = CreateBox3(Box);

			bool test = Intersection.TestSegment3Box3(ref segment, ref box);
			Segment3Box3Intr info;
			bool find = Intersection.FindSegment3Box3(ref segment, ref box, out info);

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
