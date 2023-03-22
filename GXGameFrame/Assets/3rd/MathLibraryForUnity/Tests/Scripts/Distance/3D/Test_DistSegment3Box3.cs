using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_DistSegment3Box3 : Test_Base
	{
		public Transform P0, P1;
		public Transform Box;

		private void OnDrawGizmos()
		{
			Segment3 segment = CreateSegment3(P0, P1);
			Box3 box = CreateBox3(Box);

			Vector3 closestPoint0, closestPoint1;
			float dist = Distance.Segment3Box3(ref segment, ref box, out closestPoint0, out closestPoint1);

			FiguresColor();
			DrawSegment(ref segment);
			DrawBox(ref box);

			ResultsColor();
			DrawPoint(closestPoint0);
			DrawPoint(closestPoint1);

			LogInfo("Dist: " + dist);
		}
	}
}
