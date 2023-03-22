using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrRay2Circle2 : Test_Base
	{
		public Transform Ray;
		public Transform Circle;

		private void OnDrawGizmos()
		{
			Ray2 ray = CreateRay2(Ray);
			Circle2 circle = CreateCircle2(Circle);

			bool test = Intersection.TestRay2Circle2(ref ray, ref circle);
			Ray2Circle2Intr info;
			bool find = Intersection.FindRay2Circle2(ref ray, ref circle, out info);

			FiguresColor();
			DrawRay(ref ray);
			DrawCircle(ref circle);

			if (find)
			{
				Gizmos.color = Color.blue;
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
