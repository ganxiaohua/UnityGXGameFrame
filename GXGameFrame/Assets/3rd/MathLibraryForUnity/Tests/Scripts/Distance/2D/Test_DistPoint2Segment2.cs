using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_DistPoint2Segment2 : Test_Base
	{
		public Transform Point;
		public Transform P0, P1;

		private void OnDrawGizmos()
		{
			Vector2 point = Point.position;
			Segment2 segment = CreateSegment2(P0, P1);

			Vector2 closestPoint;
			float dist0 = Distance.Point2Segment2(ref point, ref segment, out closestPoint);
			float dist1 = segment.DistanceTo(point);

			FiguresColor();
			DrawSegment(ref segment);

			ResultsColor();
			DrawPoint(closestPoint);

			LogInfo(dist0 + "   " + dist1);
		}
	}
}
