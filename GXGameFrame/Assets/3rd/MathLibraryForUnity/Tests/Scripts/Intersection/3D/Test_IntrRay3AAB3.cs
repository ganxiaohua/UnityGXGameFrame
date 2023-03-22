using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrRay3AAB3 : Test_Base
	{
		public Transform Ray;
		public Transform Box_Point0;
		public Transform Box_Point1;

		private void OnDrawGizmos()
		{
			Ray3 ray = CreateRay3(Ray);
			AAB3 box = CreateAAB3(Box_Point0, Box_Point1);

			bool test = Intersection.TestRay3AAB3(ref ray, ref box);
			Ray3AAB3Intr info;
			bool find = Intersection.FindRay3AAB3(ref ray, ref box, out info);

			FiguresColor();
			DrawRay(ref ray);
			DrawAAB(ref box);

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
