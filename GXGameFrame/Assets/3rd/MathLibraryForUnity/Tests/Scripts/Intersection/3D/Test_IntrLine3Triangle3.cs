using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrLine3Triangle3 : Test_Base
	{
		public Transform Line;
		public Transform V0;
		public Transform V1;
		public Transform V2;

		private void OnDrawGizmos()
		{
			Line3 line = new Line3(Line.position, Line.forward);
			Triangle3 triangle = CreateTriangle3(V0, V1, V2);

			IntersectionTypes intersectionType;
			bool test = Intersection.TestLine3Triangle3(ref line, ref triangle, out intersectionType);
			Line3Triangle3Intr info;
			bool find = Intersection.FindLine3Triangle3(ref line, ref triangle, out info);

			FiguresColor();
			DrawLine(ref line);
			DrawTriangle(ref triangle);

			if (find)
			{
				ResultsColor();
				DrawPoint(info.Point);
			}

			LogInfo(info.IntersectionType);
			if (test != find) LogError("test != find");
			if (intersectionType != info.IntersectionType) LogError("intersectionType != info.IntersectionType");
		}
	}
}
