using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_Triangle2Sides : Test_Base
	{
		public Transform V0, V1, V2;
		public Transform Point;

		private void OnDrawGizmos()
		{
			Triangle2 triangle = CreateTriangle2(V0, V1, V2);
			Vector2 point = Point.position;

			Orientations orientation = triangle.CalcOrientation();
			int ccwSide = triangle.QuerySideCCW(point);
			int cwSide  = triangle.QuerySideCW(point);

			FiguresColor();
			DrawTriangle(ref triangle);
			if (orientation == Orientations.CCW)
			{
				SetColor(ccwSide);
			}
			else if (orientation == Orientations.CW)
			{
				SetColor(cwSide);
			}
			DrawPoint(point);

			LogInfo("Orientation: " + orientation + "      CCWSide: " + ccwSide + "     CWSide: " + cwSide);
		}

		private void SetColor(int side)
		{
			if (side < 0)
			{
				Gizmos.color = Color.blue;
			}
			else if (side > 0)
			{
				Gizmos.color = Color.red;
			}
			else
			{
				Gizmos.color = new Color(.4f, .4f, .4f);
			}
		}
	}
}
