using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrPlane3Triangle3 : Test_Base
	{
		public Transform Plane;
		public Transform V0;
		public Transform V1;
		public Transform V2;

		private void OnDrawGizmos()
		{
			Plane3 plane = CreatePlane3(Plane);
			Triangle3 triangle = CreateTriangle3(V0, V1, V2);

			bool test = Intersection.TestPlane3Triangle3(ref plane, ref triangle);
			Plane3Triangle3Intr info;
			bool find = Intersection.FindPlane3Triangle3(ref plane, ref triangle, out info);

			FiguresColor();
			DrawPlane(ref plane, Plane);
			DrawTriangle(ref triangle);

			if (find)
			{
				ResultsColor();
				if (info.Quantity == 2)
				{
					DrawSegment(info.Point0, info.Point1);
				}
			}

			LogInfo("test: " + test + " find: " + info.IntersectionType);
		}
	}
}
