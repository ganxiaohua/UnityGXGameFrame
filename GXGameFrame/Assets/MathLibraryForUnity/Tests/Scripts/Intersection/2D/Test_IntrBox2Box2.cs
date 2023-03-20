using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrBox2Box2 : Test_Base
	{
		public Transform Box0;
		public Transform Box1;

		private void OnDrawGizmos()
		{
			Box2 box0 = CreateBox2(Box0);
			Box2 box1 = CreateBox2(Box1);

			bool test = Intersection.TestBox2Box2(ref box0, ref box1);

			FiguresColor();
			DrawBox(ref box0);
			DrawBox(ref box1);

			LogInfo(test);
		}
	}
}
