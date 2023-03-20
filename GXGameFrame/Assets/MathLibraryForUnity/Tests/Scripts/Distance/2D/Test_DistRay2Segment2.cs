using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_DistRay2Segment2 : Test_Base
	{
		public Transform Ray;
		public Transform P0, P1;

		private void OnDrawGizmos()
		{
			Ray2 ray = CreateRay2(Ray);
			Segment2 segment = CreateSegment2(P0, P1);

			Vector2 closestPoint0, closestPoint1;
			float dist = Distance.Ray2Segment2(ref ray, ref segment, out closestPoint0, out closestPoint1);

			FiguresColor();
			DrawRay(ref ray);
			DrawSegment(ref segment);

			ResultsColor();
			DrawPoint(closestPoint0);
			DrawPoint(closestPoint1);

			LogInfo(dist);
		}
	}
}
