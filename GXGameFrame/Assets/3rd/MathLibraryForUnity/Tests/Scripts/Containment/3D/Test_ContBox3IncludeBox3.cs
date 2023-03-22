using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_ContBox3IncludeBox3 : Test_Base
	{
		public Transform Box0;
		public Transform Box1;

		private void OnDrawGizmos()
		{
			Box3 box0 = CreateBox3(Box0);
			Box3 box1 = CreateBox3(Box1);

			Box3 box = box0;
			box.Include(box1); // Box which merges box0 and box1

			FiguresColor();
			DrawBox(ref box0);
			DrawBox(ref box1);
			ResultsColor();
			DrawBox(ref box);
		}
	}
}
