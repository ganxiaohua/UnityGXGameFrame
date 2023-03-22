using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrLine3Rectangle3 : Test_Base
	{
		public Transform Line;
		public Transform Rectangle;

		private void OnDrawGizmos()
		{
			Line3 line = CreateLine3(Line);
			Rectangle3 rectangle = CreateRectangle3(Rectangle);

			
			Line3Rectangle3Intr info;
			bool find = Intersection.FindLine3Rectangle3(ref line, ref rectangle, out info);

			FiguresColor();
			DrawRectangle(ref rectangle);
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
