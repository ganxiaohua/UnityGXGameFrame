using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_ContPoint2Circle2 : Test_Base
	{
		public Transform Point;
		public Transform Circle;

		private void OnDrawGizmos()
		{
			Vector2 point = Point.position;
			Circle2 circle = CreateCircle2(Circle);

			bool cont = circle.Contains(point);

			FiguresColor();
			DrawCircle(ref circle);
			if (cont) ResultsColor();
			DrawPoint(point);

			LogInfo(cont);
		}
	}
}
