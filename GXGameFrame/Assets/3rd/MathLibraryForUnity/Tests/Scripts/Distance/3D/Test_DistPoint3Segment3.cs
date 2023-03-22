using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_DistPoint3Segment3 : Test_Base
	{
		public Transform Point;
		public Transform P0, P1;

		private void OnDrawGizmos()
		{
			Vector3 point = Point.position;
			Segment3 segment = CreateSegment3(P0, P1);

			Vector3 closestPoint;
			float dist0 = Distance.Point3Segment3(ref point, ref segment, out closestPoint);
			float dist1 = segment.DistanceTo(point);

			FiguresColor();
			DrawSegment(ref segment);

			ResultsColor();
			DrawPoint(closestPoint);

			LogInfo(dist0 + "   " + dist1);
		}
	}
}
