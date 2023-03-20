using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_ApprLineFit2 : Test_Base
	{
		public Transform[] Points;

		private void OnDrawGizmos()
		{
			Vector2[] points = CreatePoints2(Points);
			if (points.Length > 1)
			{
				Line2 line = Approximation.LeastSquaresLineFit2(points);

				FiguresColor();
				DrawPoints(points);
				ResultsColor();
				DrawLine(ref line);
			}
		}
	}
}
