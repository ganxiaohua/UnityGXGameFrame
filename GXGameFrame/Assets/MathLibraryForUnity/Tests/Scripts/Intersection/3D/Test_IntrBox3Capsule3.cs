using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrBox3Capsule3 : Test_Base
	{
		public Transform P0, P1;
		public float Radius;
		public Transform Box;

		private void OnDrawGizmos()
		{
			Box3 box = CreateBox3(Box);
			Capsule3 capsule = CreateCapsule3(P0, P1, Radius);

			bool intr = Intersection.TestBox3Capsule3(ref box, ref capsule);

			FiguresColor();
			DrawBox(ref box);
			DrawCapsule(ref capsule);

			LogInfo("Intr: " + intr);
		}
	}
}
