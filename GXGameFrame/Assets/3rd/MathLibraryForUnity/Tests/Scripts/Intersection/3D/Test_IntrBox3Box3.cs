using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrBox3Box3 : Test_Base
	{
		public Transform Box0;
		public Transform Box1;

		private void OnDrawGizmos()
		{
			Box3 box0 = CreateBox3(Box0);
			Box3 box1 = CreateBox3(Box1);

			bool test = Intersection.TestBox3Box3(ref box0, ref box1);

			FiguresColor();
			DrawBox(ref box0);
			DrawBox(ref box1);

			LogInfo(test);
		}
	}
}
