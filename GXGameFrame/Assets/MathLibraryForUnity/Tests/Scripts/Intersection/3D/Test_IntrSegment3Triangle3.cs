using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrSegment3Triangle3 : Test_Base
	{
		public Transform P0;
		public Transform P1;
		public Transform V0;
		public Transform V1;
		public Transform V2;

		private void OnDrawGizmos()
		{
			Segment3 segment = CreateSegment3(P0, P1);
			Triangle3 triangle = CreateTriangle3(V0, V1, V2);

			IntersectionTypes intersectionType;
			bool test = Intersection.TestSegment3Triangle3(ref segment, ref triangle, out intersectionType);
			Segment3Triangle3Intr info;
			bool find = Intersection.FindSegment3Triangle3(ref segment, ref triangle, out info);

			FiguresColor();
			DrawSegment(ref segment);
			DrawTriangle(ref triangle);

			if (find)
			{
				ResultsColor();
				DrawPoint(info.Point);
			}

			LogInfo(info.IntersectionType + " " + info.SegmentParameter);
			if (test != find) LogError("test != find");
			if (intersectionType != info.IntersectionType) LogError("intersectionType != info.IntersectionType");
		}
	}
}
