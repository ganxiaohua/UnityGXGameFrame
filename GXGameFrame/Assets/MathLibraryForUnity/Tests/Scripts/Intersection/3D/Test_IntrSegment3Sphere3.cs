using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrSegment3Sphere3 : Test_Base
	{
		public Transform P0;
		public Transform P1;
		public Transform Sphere;

		private void OnDrawGizmos()
		{
			Segment3 segment = CreateSegment3(P0, P1);
			Sphere3 sphere = CreateSphere3(Sphere);

			bool test = Intersection.TestSegment3Sphere3(ref segment, ref sphere);
			Segment3Sphere3Intr info;
			bool find = Intersection.FindSegment3Sphere3(ref segment, ref sphere, out info);

			FiguresColor();
			DrawSegment(ref segment);
			DrawSphere(ref sphere);

			if (find)
			{
				ResultsColor();
				if (info.IntersectionType == IntersectionTypes.Point)
				{
					DrawPoint(info.Point0);
				}
				else if (info.IntersectionType == IntersectionTypes.Segment)
				{
					DrawSegment(info.Point0, info.Point1);
					DrawPoint(info.Point0);
					DrawPoint(info.Point1);
				}
			}

			LogInfo(info.IntersectionType);
			if (test != find) LogError("test != find");
		}
	}
}