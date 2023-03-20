using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrLine2Ray2 : Test_Base
	{
		public Transform Line;
		public Transform Ray;

		private void OnDrawGizmos()
		{
			Line2 line = CreateLine2(Line);
			Ray2 ray = CreateRay2(Ray);

			IntersectionTypes intersectionType;
			bool test = Intersection.TestLine2Ray2(ref line, ref ray, out intersectionType);
			Line2Ray2Intr info;
			bool find = Intersection.FindLine2Ray2(ref line, ref ray, out info);

			FiguresColor();
			DrawLine(ref line);
			DrawRay(ref ray);			

			if (find)
			{
				ResultsColor();
				DrawPoint(info.Point);
			}

			LogInfo(intersectionType);
			if (test != find) LogError("test != find");
			if (intersectionType != info.IntersectionType) LogError("intersectionType != info.IntersectionType");
		}
	}
}
