using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrPlane3AAB3 : Test_Base
	{
		public Transform Plane;
		public Transform Box_Point0;
		public Transform Box_Point1;

		private void OnDrawGizmos()
		{
			Plane3 plane = CreatePlane3(Plane);
			AAB3 box = CreateAAB3(Box_Point0, Box_Point1);

			bool test = Intersection.TestPlane3AAB3(ref plane, ref box);

			FiguresColor();
			DrawPlane(ref plane, Plane);
			DrawAAB(ref box);

			LogInfo("Intersection: " + test);
		}
	}
}
