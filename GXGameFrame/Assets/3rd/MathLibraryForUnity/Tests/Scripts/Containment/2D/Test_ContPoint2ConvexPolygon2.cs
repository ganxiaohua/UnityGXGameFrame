using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_ContPoint2ConvexPolygon2 : Test_Base
	{
		public Transform Point;
		public Transform[] ConvexPolygon;

		private void OnDrawGizmos()
		{
			Vector2 point = Point.position;
			Polygon2 convexPolygon = CreatePolygon2(ConvexPolygon);

			Orientations orientation;
			bool convex = convexPolygon.IsConvex(out orientation);
			if (convex)
			{
				bool cont;
				if (orientation == Orientations.CCW)
				{
					cont = convexPolygon.ContainsConvexCCW(point);
					//cont = convexPolygon.ContainsConvexQuadCCW(point); // You can use this if polygon has 4 vertices
				}
				else // CW
				{
					cont = convexPolygon.ContainsConvexCW(point);
					//cont = convexPolygon.ContainsConvexQuadCW(point);
				}

				FiguresColor();
				DrawPolygon(convexPolygon);
				if (cont) ResultsColor();
				DrawPoint(point);

				LogInfo("Orientation: " + orientation + "     Contained: " + cont);
			}
			else
			{
				FiguresColor();
				DrawPolygon(convexPolygon);
				DrawPoint(point);

				LogError("polygon is non-convex");
			}
		}
	}
}
