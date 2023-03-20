using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_DistPoint2Line2 : Test_Base
	{
		public Transform Point;
		public Transform Line;

		private void OnDrawGizmos()
		{
			Vector2 point = Point.position;
			Line2 line = CreateLine2(Line);

			Vector2 closestPoint;
			float dist0 = Distance.Point2Line2(ref point, ref line, out closestPoint);
			float dist1 = line.DistanceTo(point);

			FiguresColor();
			DrawLine(ref line);

			ResultsColor();
			DrawPoint(closestPoint);

			LogInfo(dist0 + "   " + dist1);
		}
	}
}
