using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_ApprLineFit3 : Test_Base
	{
		public Transform[] Points;

		private void OnDrawGizmos()
		{
			Vector3[] points = CreatePoints3(Points);
			if (points.Length > 1)
			{
				Line3 line = Approximation.LeastsSquaresLineFit3(points);

				FiguresColor();
				DrawPoints(points);
				ResultsColor();
				DrawLine(ref line);
			}
		}
	}
}
