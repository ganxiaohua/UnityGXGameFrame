using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrSegment3Rectangle3 : Test_Base
	{
		public Transform P0, P1;
		public Transform Rectangle;

		private void OnDrawGizmos()
		{
			Segment3 segment = CreateSegment3(P0, P1);
			Rectangle3 rectangle = CreateRectangle3(Rectangle);


			Segment3Rectangle3Intr info;
			bool find = Intersection.FindSegment3Rectangle3(ref segment, ref rectangle, out info);

			FiguresColor();
			DrawRectangle(ref rectangle);
			DrawSegment(ref segment);

			if (find)
			{
				ResultsColor();
				DrawPoint(info.Point);
			}

			LogInfo(info.IntersectionType);
		}
	}
}
