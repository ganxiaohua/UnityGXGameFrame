using System;
using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrSegment3Plane3 : Test_Base
	{
		public Transform P0;
		public Transform P1;
		public Transform Plane;

		private void OnDrawGizmos()
		{
			Segment3 segment = CreateSegment3(P0, P1);
			Plane3 plane = CreatePlane3(Plane);

			IntersectionTypes intersectionType;
			bool test = Intersection.TestSegment3Plane3(ref segment, ref plane, out intersectionType);
			Segment3Plane3Intr info;
			bool find = Intersection.FindSegment3Plane3(ref segment, ref plane, out info);

			FiguresColor();
			DrawPlane(ref plane, Plane);
			DrawSegment(ref segment);

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
