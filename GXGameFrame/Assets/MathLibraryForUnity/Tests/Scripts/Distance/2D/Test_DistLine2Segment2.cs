using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_DistLine2Segment2 : Test_Base
	{
		public Transform Line;
		public Transform P0, P1;

		private void OnDrawGizmos()
		{
			Line2 line = CreateLine2(Line);
			Segment2 segment = CreateSegment2(P0, P1);

			Vector2 closestPoint0, closestPoint1;
			float dist = Distance.Line2Segment2(ref line, ref segment, out closestPoint0, out closestPoint1);

			FiguresColor();
			DrawLine(ref line);
			DrawSegment(ref segment);

			ResultsColor();
			DrawPoint(closestPoint0);
			DrawPoint(closestPoint1);

			LogInfo(dist);
		}
	}
}
