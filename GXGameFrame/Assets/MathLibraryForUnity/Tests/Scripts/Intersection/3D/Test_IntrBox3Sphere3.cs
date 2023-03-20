using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrBox3Sphere3 : Test_Base
	{
		public Transform Box;
		public Transform Sphere;

		private void OnDrawGizmos()
		{
			Box3 box = CreateBox3(Box);
			Sphere3 sphere = CreateSphere3(Sphere);

			bool test = Intersection.TestBox3Sphere3(ref box, ref sphere);

			FiguresColor();
			DrawBox(ref box);
			DrawSphere(ref sphere);

			LogInfo(test);
		}
	}
}
