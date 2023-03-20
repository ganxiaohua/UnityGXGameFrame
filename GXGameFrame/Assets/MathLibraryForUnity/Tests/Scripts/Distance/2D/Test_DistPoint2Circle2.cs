using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_DistPoint2Circle2 : Test_Base
	{
		public Transform Point;
		public Transform Circle;

		private void OnDrawGizmos()
		{
			Vector2 point = Point.position;
			Circle2 circle = CreateCircle2(Circle);

			Vector2 closestPoint;
			float dist = Distance.Point2Circle2(ref point, ref circle, out closestPoint);

			FiguresColor();
			DrawCircle(ref circle);

			ResultsColor();
			DrawPoint(closestPoint);

			LogInfo(dist);
		}
	}
}
