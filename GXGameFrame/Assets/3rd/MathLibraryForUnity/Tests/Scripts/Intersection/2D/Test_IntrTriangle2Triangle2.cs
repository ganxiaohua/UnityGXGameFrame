using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrTriangle2Triangle2 : Test_Base
	{
		public Transform V0;
		public Transform V1;
		public Transform V2;
		public Transform P0;
		public Transform P1;
		public Transform P2;

		private void OnDrawGizmos()
		{
			Triangle2 triangle0 = CreateTriangle2(V0, V1, V2);
			Triangle2 triangle1 = CreateTriangle2(P0, P1, P2);

			bool test = Intersection.TestTriangle2Triangle2(ref triangle0, ref triangle1);
			Triangle2Triangle2Intr info;
			bool find = Intersection.FindTriangle2Triangle2(ref triangle0, ref triangle1, out info);
			
			FiguresColor();
			DrawTriangle(ref triangle0);
			DrawTriangle(ref triangle1);

			if (find)
			{
				ResultsColor();
				for (int i = 0; i < info.Quantity; ++i)
				{
					Vector2 p = info[i];
					DrawSegment(p, info[(i + 1) % info.Quantity]);
					DrawPoint(p);
				}
			}

			LogInfo(info.IntersectionType + " " + info.Quantity);
			if (test != find) LogError("test != find");
		}
	}
}
