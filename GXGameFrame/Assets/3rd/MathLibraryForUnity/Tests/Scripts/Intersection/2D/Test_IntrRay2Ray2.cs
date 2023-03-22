using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrRay2Ray2 : Test_Base
	{
		public Transform Ray0;
		public Transform Ray1;

		private void OnDrawGizmos()
		{
			Ray2 ray0 = CreateRay2(Ray0);
			Ray2 ray1 = CreateRay2(Ray1);

			IntersectionTypes intersectionType;
			bool test = Intersection.TestRay2Ray2(ref ray0, ref ray1, out intersectionType);
			Ray2Ray2Intr info;
			bool find = Intersection.FindRay2Ray2(ref ray0, ref ray1, out info);

			FiguresColor();
			DrawRay(ref ray0);
			DrawRay(ref ray1);

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
