using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrSegment3Circle3 : Test_Base
	{
		public Transform P0, P1;
		public Transform Circle;

		private void OnDrawGizmos()
		{
			Segment3 segment = CreateSegment3(P0, P1);
			Circle3 circle = CreateCircle3(Circle);

			Segment3Circle3Intr info;
			bool find = Intersection.FindSegment3Circle3(ref segment, ref circle, out info);

			FiguresColor();
			DrawCircle(ref circle);
			DrawSegment(ref segment);

			if (find)
			{
				ResultsColor();
				DrawPoint(info.Point);
			}

			LogInfo(info.IntersectionType);
		}
	}
}
