using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_DistSegment3Segment3 : Test_Base
	{
		public Transform P0, P1;
		public Transform R0, R1;

		private void OnDrawGizmos()
		{
			Segment3 segment0 = CreateSegment3(P0, P1);
			Segment3 segment1 = CreateSegment3(R0, R1);

			Vector3 closestPoint0, closestPoint1;
			float dist = Distance.Segment3Segment3(ref segment0, ref segment1, out closestPoint0, out closestPoint1);

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
