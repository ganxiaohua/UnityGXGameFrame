using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrLine2Triangle2 : Test_Base
	{
		public Transform Line;
		public Transform V0;
		public Transform V1;
		public Transform V2;

		private void OnDrawGizmos()
		{
			Line2 line = CreateLine2(Line);
			Triangle2 triangle = CreateTriangle2(V0, V1, V2);

			IntersectionTypes intersectionType;
			bool test = Intersection.TestLine2Triangle2(ref line, ref triangle, out intersectionType);
			Line2Triangle2Intr info;
			bool find = Intersection.FindLine2Triangle2(ref line, ref triangle, out info);

			FiguresColor();
			DrawLine(ref line);
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
