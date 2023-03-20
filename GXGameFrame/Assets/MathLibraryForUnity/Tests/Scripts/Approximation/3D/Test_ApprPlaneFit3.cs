using UnityEngine;
using Dest.Math;

namespace Dest.Math.Tests
{
	[ExecuteInEditMode]
	public class Test_ApprPlaneFit3 : Test_Base
	{
		public Transform[] Points;

		private void OnDrawGizmos()
		{
			Vector3[] points = CreatePoints3(Points);
			if (points.Length > 1)
			{
				Plane3 plane = Approximation.LeastSquaresPlaneFit3(points);

				FiguresColor();
				DrawPoints(points);
				ResultsColor();
				DrawPlane(ref plane, Points[0]);
			}
		}
	}
}
