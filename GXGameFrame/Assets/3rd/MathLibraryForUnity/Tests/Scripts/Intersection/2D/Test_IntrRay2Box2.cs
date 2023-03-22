using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrRay2Box2 : Test_Base
	{
		public Transform Ray;
		public Transform Box;

		private void OnDrawGizmos()
		{
			Ray2 ray = CreateRay2(Ray);
			Box2 box = CreateBox2(Box);

			bool test = Intersection.TestRay2Box2(ref ray, ref box);
			Ray2Box2Intr info;
			bool find = Intersection.FindRay2Box2(ref ray, ref box, out info);

			FiguresColor();
			DrawRay(ref ray);
			DrawBox(ref box);

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
