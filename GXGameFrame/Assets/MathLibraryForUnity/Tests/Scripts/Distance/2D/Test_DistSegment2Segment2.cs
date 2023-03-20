using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_DistSegment2Segment2 : Test_Base
	{
		public Transform P0, P1;
		public Transform R0, R1;

		private void OnDrawGizmos()
		{
			Segment2 segment0 = CreateSegment2(P0, P1);
			Segment2 segment1 = CreateSegment2(R0, R1);

			Vector2 closestPoint0, closestPoint1;
			float dist = Distance.Segment2Segment2(ref segment0, ref segment1, out closestPoint0, out closestPoint1);

			FiguresColor();
			DrawSegment(ref segment0);
			DrawSegment(ref segment1);

			ResultsColor();
			DrawPoint(closestPoint0);
			DrawPoint(closestPoint1);

			LogInfo(dist);
		}
	}
}
