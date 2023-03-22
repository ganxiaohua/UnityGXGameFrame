using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrLine2Line2 : Test_Base
	{
		public Transform Line0;
		public Transform Line1;
		
		private void OnDrawGizmos()
		{
			Line2 line0 = CreateLine2(Line0);
			Line2 line1 = CreateLine2(Line1);

			IntersectionTypes intersectionType;
			bool test = Intersection.TestLine2Line2(ref line0, ref line1, out intersectionType);
			Line2Line2Intr info;
			bool find = Intersection.FindLine2Line2(ref line0, ref line1, out info);

			FiguresColor();
			DrawLine(ref line0);
			DrawLine(ref line1);

			if (find)
			{
				ResultsColor();
				if (info.IntersectionType == IntersectionTypes.Point)
				{
					DrawPoint(info.Point);
				}
			}

			LogInfo(intersectionType);
			if (test != find) LogError("test != find");
			if (intersectionType != info.IntersectionType) LogError("intersectionType != info.IntersectionType");
		}
	}
}
