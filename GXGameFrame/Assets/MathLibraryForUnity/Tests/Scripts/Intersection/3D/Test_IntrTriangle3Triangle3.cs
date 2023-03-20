using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrTriangle3Triangle3 : Test_Base
	{
		public Transform V0;
		public Transform V1;
		public Transform V2;
		public Transform P0;
		public Transform P1;
		public Transform P2;

		private void OnDrawGizmos()
		{
			Triangle3 triangle0 = CreateTriangle3(V0, V1, V2);
			Triangle3 triangle1 = CreateTriangle3(P0, P1, P2);

			FiguresColor();
			DrawTriangle(ref triangle0);
			DrawTriangle(ref triangle1);

			bool test = Intersection.TestTriangle3Triangle3(ref triangle0, ref triangle1);
			Triangle3Triangle3Intr info;
			bool find = Intersection.FindTriangle3Triangle3(ref triangle0, ref triangle1, out info, true);

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
				else if (info.IntersectionType == IntersectionTypes.Plane)
				{
					for (int i = 0; i < info.Quantity; ++i)
					{
						DrawSegment(info[i], info[(i + 1) % info.Quantity]);
						DrawPoint(info[i]);
					}
				}
			}

			LogInfo("Type: " + info.IntersectionType + "      CoplanarType: " + info.CoplanarIntersectionType + "      Touching: " + info.Touching + "      Quantity: " + info.Quantity);
			if (test != find) LogError("test != find");
		}
	}
}
