using UnityEngine;
using System.Collections.Generic;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_IntrConvexPolygon2ConvexPolygon2 : Test_Base
	{
		public Transform[] Points0;
		public Transform[] Points1;

		private void OnDrawGizmos()
		{
			Polygon2 pol0 = CreatePolygon2(Points0);
			Polygon2 pol1 = CreatePolygon2(Points1);			

			Orientations orientation0;
			bool pol0Convex = pol0.IsConvex(out orientation0);
			Orientations orientation1;
			bool pol1Convex = pol1.IsConvex(out orientation1);

			FiguresColor();
			DrawPolygon(pol0);
			DrawPolygon(pol1);

			if (pol0Convex && pol1Convex && orientation0 == Orientations.CCW && orientation1 == Orientations.CCW)
			{
				bool test = Intersection.TestConvexPolygon2ConvexPolygon2(pol0, pol1);
				Logger.LogInfo("Intersection: " + test);
			}
			else
			{
				Logger.LogError("Polygons are incorrect." +
					"   Pol0Convex: " + pol0Convex + "   Pol0Ori: " + orientation0 + "   Pol1Convex: " + pol1Convex + "   Pol1Ori: " + orientation1);
			}
		}
	}
}
