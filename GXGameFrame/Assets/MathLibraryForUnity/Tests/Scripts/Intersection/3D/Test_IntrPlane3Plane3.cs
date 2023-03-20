using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrPlane3Plane3 : Test_Base
	{
		public Transform Plane0;
		public Transform Plane1;

		private void OnDrawGizmos()
		{
			Plane3 plane0 = CreatePlane3(Plane0);
			Plane3 plane1 = CreatePlane3(Plane1);

			bool test = Intersection.TestPlane3Plane3(ref plane0, ref plane1);
			Plane3Plane3Intr info;
			bool find = Intersection.FindPlane3Plane3(ref plane0, ref plane1, out info);

			FiguresColor();
			DrawPlane(ref plane0, Plane0);
			DrawPlane(ref plane1, Plane1);

			if (find)
			{
				if (info.IntersectionType == IntersectionTypes.Line)
				{
					ResultsColor();
					DrawLine(ref info.Line);
				}
			}

			LogInfo("test: " + test + " find: " + info.IntersectionType);
		}
	}
}
