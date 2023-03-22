using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrPlane3Box3 : Test_Base
	{
		public Transform Plane;
		public Transform Box;

		private void OnDrawGizmos()
		{
			Plane3 plane = CreatePlane3(Plane);
			Box3 box = CreateBox3(Box);

			bool test = Intersection.TestPlane3Box3(ref plane, ref box);

			FiguresColor();
			DrawPlane(ref plane, Plane);
			DrawBox(ref box);

			LogInfo("Intersection: " + test);
		}
	}
}
