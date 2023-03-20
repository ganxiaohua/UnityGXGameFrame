using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrBox2Circle2 : Test_Base
	{
		public Transform Box;
		public Transform Circle;

		private void OnDrawGizmos()
		{
			Box2 box = CreateBox2(Box);
			Circle2 circle = CreateCircle2(Circle);

			bool test = Intersection.TestBox2Circle2(ref box, ref circle);

			FiguresColor();
			DrawBox(ref box);
			DrawCircle(ref circle);

			LogInfo(test);
		}
	}
}
