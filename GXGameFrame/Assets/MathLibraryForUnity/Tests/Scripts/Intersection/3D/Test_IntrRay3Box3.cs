using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrRay3Box3 : Test_Base
	{
		public Transform Ray;
		public Transform Box;

		private void OnDrawGizmos()
		{
			Ray3 ray = CreateRay3(Ray);
			Box3 box = CreateBox3(Box);

			bool test = Intersection.TestRay3Box3(ref ray, ref box);
			Ray3Box3Intr info;
			bool find = Intersection.FindRay3Box3(ref ray, ref box, out info);

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
