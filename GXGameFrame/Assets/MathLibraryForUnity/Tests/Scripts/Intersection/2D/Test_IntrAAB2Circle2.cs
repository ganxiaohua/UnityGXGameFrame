using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrAAB2Circle2 : Test_Base
	{
		public Transform Box_Point0;
		public Transform Box_Point1;
		public Transform Circle;

		private void OnDrawGizmos()
		{
			AAB2 box = CreateAAB2(Box_Point0, Box_Point1);
			Circle2 circle = CreateCircle2(Circle);

			bool test = Intersection.TestAAB2Circle2(ref box, ref circle);

			FiguresColor();
			DrawAAB(ref box);
			DrawCircle(ref circle);

			LogInfo(test);
		}
	}
}
