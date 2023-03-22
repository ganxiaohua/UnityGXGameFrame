using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrLine3Plane3 : Test_Base
	{
		public Transform Line;
		public Transform Plane;

		private void OnDrawGizmos()
		{
			Line3 line = CreateLine3(Line);
			Plane3 plane = CreatePlane3(Plane);

			IntersectionTypes intersectionType;
			bool test = Intersection.TestLine3Plane3(ref line, ref plane, out intersectionType);
			Line3Plane3Intr info;
			bool find = Intersection.FindLine3Plane3(ref line, ref plane, out info);

			FiguresColor();
			DrawPlane(ref plane, Plane);
			DrawLine(ref line);

			if (find)
			{
				ResultsColor();
				DrawPoint(info.Point);
			}

			LogInfo(info.IntersectionType + " " + info.LineParameter);
			if (test != find) LogError("test != find");
			if (intersectionType != info.IntersectionType) LogError("intersectionType != info.IntersectionType");
		}
	}
}
