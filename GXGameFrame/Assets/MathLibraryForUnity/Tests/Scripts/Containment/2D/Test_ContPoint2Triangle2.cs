using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_ContPoint2Triangle2 : Test_Base
	{
		public Transform Point;
		public Transform V0, V1, V2;

		private void OnDrawGizmos()
		{
			Vector2 point = Point.position;
			Triangle2 triangle = CreateTriangle2(V0, V1, V2);

			Orientations orientation = triangle.CalcOrientation();
			if (orientation == Orientations.CCW)
			{
				bool cont = triangle.Contains(point);
				bool cont1 = triangle.ContainsCCW(point); // Use this if you know triangle orientation

				FiguresColor();
				DrawTriangle(ref triangle);
				if (cont) ResultsColor();
				DrawPoint(point);

				LogInfo("Orientation: " + orientation + "    Contained: " + cont);
				if (cont != cont1) LogError("cont != cont1");
			}
			else if (orientation == Orientations.CW)
			{
				bool cont = triangle.Contains(point);
				bool cont1 = triangle.ContainsCW(point); // Use this if you know triangle orientation

				FiguresColor();
				DrawTriangle(ref triangle);
				if (cont) ResultsColor();
				DrawPoint(point);

				LogInfo("Orientation: " + orientation + "    Contained: " + cont);
				if (cont != cont1) LogError("cont != cont1");
			}
			else // Degenerate
			{
				LogError("Triangle is degenerate");
			}
		}
	}
}
