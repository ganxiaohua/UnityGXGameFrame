using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_DistLine3Segment3 : Test_Base
	{
		public Transform Line;
		public Transform P0, P1;

		private void OnDrawGizmos()
		{
			Line3 line = CreateLine3(Line);
			Segment3 segment = CreateSegment3(P0, P1);

			Vector3 closestPoint0, closestPoint1;
			float dist = Distance.Line3Segment3(ref line, ref segment, out closestPoint0, out closestPoint1);

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
