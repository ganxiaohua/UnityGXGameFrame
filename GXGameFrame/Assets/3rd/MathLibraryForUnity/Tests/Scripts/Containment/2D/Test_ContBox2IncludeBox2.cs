using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_ContBox2IncludeBox2 : Test_Base
	{
		public Transform Box0;
		public Transform Box1;

		private void OnDrawGizmos()
		{
			Box2 box0 = CreateBox2(Box0);
			Box2 box1 = CreateBox2(Box1);

			Box2 box = box0;
			box.Include(box1); // Box which merges box0 and box1

			FiguresColor();
			DrawBox(ref box0);
			DrawBox(ref box1);
			ResultsColor();
			DrawBox(ref box);
		}
	}
}
