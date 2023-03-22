using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_ContPoint2Polygon2 : Test_Base
	{
		public Transform Point;
		public Transform[] Polygon;

		private void OnDrawGizmos()
		{
			Vector2 point = Point.position;
			Polygon2 polygon = CreatePolygon2(Polygon);

			bool cont = polygon.ContainsSimple(point);

			FiguresColor();
			DrawPolygon(polygon);
			if (cont) ResultsColor();
			DrawPoint(point);

			LogInfo(cont);
		}
	}
}
