using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrCircle2Circle2 : Test_Base
	{
		public Transform Circle0;
		public Transform Circle1;

		private void OnDrawGizmos()
		{
			Circle2 circle0 = CreateCircle2(Circle0);
			Circle2 circle1 = CreateCircle2(Circle1);

			Circle2Circle2Intr info;
			bool find = Intersection.FindCircle2Circle2(ref circle0, ref circle1, out info);
			
			FiguresColor();
			DrawCircle(ref circle0);
			DrawCircle(ref circle1);

			if (find)
			{
				ResultsColor();
				if (info.IntersectionType == IntersectionTypes.Point)
				{
					if (info.Quantity == 1)
					{
						DrawPoint(info.Point0);
					}
					else if (info.Quantity == 2)
					{
						DrawPoint(info.Point0);
						DrawPoint(info.Point1);
					}
				}
			}

			LogInfo(info.IntersectionType + " " + info.Quantity);
		}
	}
}
