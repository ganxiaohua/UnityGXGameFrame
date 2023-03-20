using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_DistRay3Segment3 : Test_Base
	{
		public Transform Ray;
		public Transform P0, P1;

		private void OnDrawGizmos()
		{
			Ray3 ray = CreateRay3(Ray);
			Segment3 segment = CreateSegment3(P0, P1);

			Vector3 closestPoint0, closestPoint1;
			float dist = Distance.Ray3Segment3(ref ray, ref segment, out closestPoint0, out closestPoint1);

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
