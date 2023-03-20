using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrRay2AAB2 : Test_Base
	{
		public Transform Ray;
		public Transform Box_Point0;
		public Transform Box_Point1;

		private void OnDrawGizmos()
		{
			Ray2 ray = CreateRay2(Ray);
			AAB2 box = CreateAAB2(Box_Point0, Box_Point1);

			bool test = Intersection.TestRay2AAB2(ref ray, ref box);
			Ray2AAB2Intr info;
			bool find = Intersection.FindRay2AAB2(ref ray, ref box, out info);

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
