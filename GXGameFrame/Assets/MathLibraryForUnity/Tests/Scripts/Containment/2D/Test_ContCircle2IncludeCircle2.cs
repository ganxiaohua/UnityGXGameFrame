using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_ContCircle2IncludeCircle2 : Test_Base
	{
		public Transform Circle0;
		public Transform Circle1;

		private void OnDrawGizmos()
		{
			Circle2 circle0 = CreateCircle2(Circle0);
			Circle2 circle1 = CreateCircle2(Circle1);

			Circle2 circle = circle0;
			circle.Include(circle1); // Circle which merges circle0 and circle1

			FiguresColor();
			DrawCircle(ref circle0);
			DrawCircle(ref circle1);
			ResultsColor();
			DrawCircle(ref circle);
		}
	}
}
