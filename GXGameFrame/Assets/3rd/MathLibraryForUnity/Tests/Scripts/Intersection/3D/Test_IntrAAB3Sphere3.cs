using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrAAB3Sphere3 : Test_Base
	{
		public Transform Box_Point0;
		public Transform Box_Point1;
		public Transform Sphere;

		private void OnDrawGizmos()
		{
			AAB3 box = CreateAAB3(Box_Point0, Box_Point1);
			Sphere3 sphere = CreateSphere3(Sphere);

			bool test = Intersection.TestAAB3Sphere3(ref box, ref sphere);

			FiguresColor();
			DrawAAB(ref box);
			DrawSphere(ref sphere);

			LogInfo(test);
		}
	}
}
