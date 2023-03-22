using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrLine3Sphere3 : Test_Base
	{
		public Transform Line;
		public Transform Sphere;

		private void OnDrawGizmos()
		{
			Line3 line = CreateLine3(Line);
			Sphere3 sphere = CreateSphere3(Sphere);

			bool test = Intersection.TestLine3Sphere3(ref line, ref sphere);
			Line3Sphere3Intr info;
			bool find = Intersection.FindLine3Sphere3(ref line, ref sphere, out info);

			FiguresColor();
			DrawLine(ref line);
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
