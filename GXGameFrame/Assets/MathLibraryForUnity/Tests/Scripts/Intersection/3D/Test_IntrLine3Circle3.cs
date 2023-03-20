using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrLine3Circle3 : Test_Base
	{
		public Transform Line;
		public Transform Circle;

		private void OnDrawGizmos()
		{
			Line3 line = CreateLine3(Line);
			Circle3 circle = CreateCircle3(Circle);
						
			Line3Circle3Intr info;
			bool find = Intersection.FindLine3Circle3(ref line, ref circle, out info);

			FiguresColor();
			DrawCircle(ref circle);
			DrawLine(ref line);

			if (find)
			{
				ResultsColor();
				DrawPoint(info.Point);
			}

			LogInfo(info.IntersectionType);
		}
	}
}
