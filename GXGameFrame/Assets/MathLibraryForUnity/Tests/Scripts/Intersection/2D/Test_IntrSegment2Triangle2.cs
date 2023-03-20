using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrSegment2Triangle2 : Test_Base
	{
		public Transform P0;
		public Transform P1;
		public Transform V0;
		public Transform V1;
		public Transform V2;

		private void OnDrawGizmos()
		{
			Segment2 segment = CreateSegment2(P0, P1);
			Triangle2 triangle = CreateTriangle2(V0, V1, V2);

			IntersectionTypes intersectionType;
			bool test = Intersection.TestSegment2Triangle2(ref segment, ref triangle, out intersectionType);
			Segment2Triangle2Intr info;
			bool find = Intersection.FindSegment2Triangle2(ref segment, ref triangle, out info);

			FiguresColor();
			DrawSegment(ref segment);
			DrawTriangle(ref triangle);

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

			LogInfo(intersectionType);
			if (test != find) LogError("test != find");
			if (intersectionType != info.IntersectionType) LogError("intersectionType != info.IntersectionType");
		}
	}
}
