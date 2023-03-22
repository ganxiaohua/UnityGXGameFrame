using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrRay3Rectangle3 : Test_Base
	{
		public Transform Ray;
		public Transform Rectangle;

		private void OnDrawGizmos()
		{
			Ray3 ray = CreateRay3(Ray);
			Rectangle3 rectangle = CreateRectangle3(Rectangle);

			
			Ray3Rectangle3Intr info;
			bool find = Intersection.FindRay3Rectangle3(ref ray, ref rectangle, out info);

			FiguresColor();
			DrawRectangle(ref rectangle);
			DrawRay(ref ray);

			if (find)
			{
				ResultsColor();
				DrawPoint(info.Point);
			}

			LogInfo(info.IntersectionType);
		}
	}
}
